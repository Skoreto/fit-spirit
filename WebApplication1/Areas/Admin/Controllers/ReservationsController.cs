using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAccess.Dao;
using DataAccess.Model;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        // GET: /Admin/Reservations/
        [Authorize(Roles = "client")]
        public ActionResult Index(bool? isActive, int? page)
        {
            // Nastavení uskutečněných lekcí jako neaktivních
            LessonDao lessonDao = new LessonDao();
            lessonDao.SetExpiredLessons();
          
            // Tato proměnná by správně neměla být definována uvnitř controlleru, ale měla by to být konfigurační konstanta.
            const int itemsOnPage = 10;
            int pg = page.HasValue ? page.Value : 1;
            int totalClientsReservations;

            FitnessCentreUser user = new FitnessCentreUserDao().GetByLogin(User.Identity.Name);
            ReservationDao reservationDao = new ReservationDao();

            // Větvení pro výběr uplynulých/aktivních či všech rezervací.
            IList<Reservation> listClientsReservationsPerPage;
            if (isActive.HasValue)
                listClientsReservationsPerPage = reservationDao.GetRestrictedClientsReservationsPaged(isActive, user.Id, itemsOnPage, pg, out totalClientsReservations);
            else
                listClientsReservationsPerPage = reservationDao.GetClientsReservationsPaged(user.Id, itemsOnPage, pg, out totalClientsReservations);            

            ViewBag.CurrentIsActive = isActive; // Pamatovat si nastavení výběru uplynulých/aktivních lekcí.
            // Výpočet počtu stránek.
            ViewBag.Pages = (int)Math.Ceiling((double)totalClientsReservations / (double)itemsOnPage);
            ViewBag.CurrentPage = pg;

            // Rezervaci nelze zrušit 6h před zahájením lekce.
            foreach (Reservation reservation in listClientsReservationsPerPage)
            {
                DateTime sixHoursBeforeStart = reservation.Lesson.StartTime.AddHours(-6);  // Výpočet času 6h před zahájením lekce.
                reservation.IsCancellable = true;   // Pro jistotu předvyplnění vlastnosti. Zabránění null hodnotám.

                // Porovnání CompareTo vrací hodnoty -1, 0, 1. Pokud aktuální čas je v rozmezí 6 hodin před zahájením lekce a více.
                if (DateTime.Now.CompareTo(sixHoursBeforeStart) > 0)          
                {
                    reservation.IsCancellable = false;
                }
            }

            if (Request.IsAjaxRequest())
                return PartialView(listClientsReservationsPerPage);
            else
                return View(listClientsReservationsPerPage);
        }

        /*
         * Akce pro zarezervování přihlášeného klienta na vybranou lekci.
         */
        [Authorize(Roles = "client")]
        public ActionResult Reserve(int id)
        {
            // Získání instance vybrané lekce.
            LessonDao lessonDao = new LessonDao();
            Lesson lesson = lessonDao.GetById(id);

            // Získání instance přihlášeného klienta.
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            FitnessCentreUser user = fitnessCentreUserDao.GetByLogin(User.Identity.Name);

            // Ověření, zda lekce je aktivní (z důvodu času či aktivity instruktora).
            if (lesson.IsActive)
            {
                // Ověření, zda na lekci je volné místo pro registraci.
                if (lesson.ActualCapacity > 0)
                {
                    // Ověření, zda má klient dostatečný kredit pro rezervaci aktivity.
                    if (user.Credit >= lesson.ActivityType.Price)
                    {
                        // Nastavení parametrů rezervace.
                        Reservation reservation = new Reservation();
                        reservation.ReservationTime = DateTime.Now;
                        reservation.Lesson = lesson;
                        reservation.Client = user;

                        // Vložení rezervace do databáze.
                        ReservationDao reservationDao = new ReservationDao();
                        reservationDao.Create(reservation);

                        // Odečtení ceny aktivity z kreditu klienta a odečtení 1 volného místa z kapacity lekce. Update hodnot v databázi.
                        user.Credit -= lesson.ActivityType.Price;
                        fitnessCentreUserDao.Update(user);
                        lesson.ActualCapacity -= 1;
                        lessonDao.Update(lesson);

                        TempData["message-success"] = "Lekce aktivity " + lesson.ActivityType.Name + " v " + lesson.StartTime.ToString("dddd d.M.") +
                                                      " byla zarezervována.";
                        return RedirectToAction("Index", "Lessons", new {isActive = true});
                    }
                    else
                    {
                        TempData["message-error"] = "Nemáte dostatek kreditu pro registraci na aktivitu " +
                                                    lesson.ActivityType.Name + ".";
                        return RedirectToAction("Index", "Lessons", new { isActive = true });
                    }
                }
                else
                {
                    TempData["message-error"] = "Na lekci není žádné volné místo.";
                    return RedirectToAction("Index", "Lessons", new { isActive = true });
                }
            }
            else
            {
                TempData["message-error"] = "Lekce není aktivní.";
                return RedirectToAction("Index", "Lessons", new { isActive = true });
            }
        }

        /*
         * Akce pro zrušení vybrané rezervace.
         */
        [Authorize(Roles = "client")]
        public ActionResult Cancel(int id)
        {
            try
            {
                // Získání instance vybrané rezervace pro zrušení.
                ReservationDao reservationDao = new ReservationDao();
                Reservation reservation = reservationDao.GetById(id);

                // Píše chybu v dvojité session
                //reservation.Client.Credit += reservation.Lesson.ActivityType.Price;
                //FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
                //fitnessCentreUserDao.Update(reservation.Client);

                //reservation.Lesson.Capacity += 1;
                //LessonDao lessonDao = new LessonDao();
                //lessonDao.Update(reservation.Lesson);

                // Přičtení ceny aktivity zpět ke kreditu klienta a uvolnění 1 místa v kapacitě lekce. Zápis změn do databáze.
                reservation.Client.Credit += reservation.Lesson.ActivityType.Price;
                FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
                FitnessCentreUser client = fitnessCentreUserDao.GetByLogin(reservation.Client.Login);
                fitnessCentreUserDao.Update(client);

                reservation.Lesson.ActualCapacity += 1;
                LessonDao lessonDao = new LessonDao();
                Lesson lesson = lessonDao.GetById(reservation.Lesson.Id);
                lessonDao.Update(lesson);

                TempData["message-success"] = "Rezervace aktivity " + reservation.Lesson.ActivityType.Name + " byla úspěšně zrušena.";

                // Smazání rezervace z databáze.
                reservationDao.Delete(reservation);
            }
            catch (Exception)
            {
                // mechanismus zachytávání výjimek doporučuje dobře si nastudovat
                throw;
            }

            return RedirectToAction("Index", new {isActive = true});
        }
	}
}