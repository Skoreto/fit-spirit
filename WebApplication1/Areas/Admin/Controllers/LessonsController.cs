using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAccess.Dao;
using DataAccess.Model;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Authorize]
    public class LessonsController : Controller
    {
        // GET: /Admin/Lessons/

        /// <summary> Akce pro zobrazení seznamu lekcí vybrané aktivity </summary>
        /// <param name="isActive">výběr lekcí aktivních/uplynulých nebo všech (null)</param>
        /// <param name="activityTypeId">Id vybrané aktivity</param>
        /// <param name="page">číslo strany výpisu</param>
        public ActionResult Index(bool? isActive, int? activityTypeId, int? page)
        {
            // Nastavení uskutečněných lekcí jako neaktivních
            LessonDao lessonDao = new LessonDao();
            lessonDao.SetExpiredLessons();

            // Pro výpis labelů filtru aktivit.
            ViewBag.ActivityTypes = new ActivityTypeDao().GetAll();

            // Tato proměnná by správně neměla být definována uvnitř controlleru, ale měla by to být konfigurační konstanta.
            const int itemsOnPage = 10;
            // Pokud přijde do parametru hodnota, tak se do "page" přiřadí pageId. Pokud je parametr null tak page = 1.
            int pg = page.HasValue ? page.Value : 1;
            int totalLessons;
            
            // Větvení, zda je požadováno třídění dle aktivních/uplynulých či druhů aktivit lekcí.
            IList<Lesson> listLessonsPerPage;
            if (isActive.HasValue)
            {
                if (activityTypeId.HasValue)
                    listLessonsPerPage = lessonDao.GetRestrictedLessonsByActivityTypeIdPaged(activityTypeId, isActive, itemsOnPage, pg, out totalLessons);
                else
                    listLessonsPerPage = lessonDao.GetRestrictedLessonsPaged(isActive, itemsOnPage, pg, out totalLessons);           
            }
            else
            {
                if (activityTypeId.HasValue)
                    listLessonsPerPage = lessonDao.GetLessonsByActivityTypeIdPaged(activityTypeId, itemsOnPage, pg, out totalLessons);
                else
                    listLessonsPerPage = lessonDao.GetLessonsPaged(itemsOnPage, pg, out totalLessons);       
            }

            ViewBag.CurrentIsActive = isActive; // Pamatovat si nastavení výběru uplynulých/aktivních lekcí.
            ViewBag.CurrentActivityTypeId = activityTypeId; // Pamatovat si zvolený filtr při procházení stránek.
            /* Výpočet počtu stránek.
             * Nelze dělit jednoduše integery, osekla by se desetinná část po celém čísle. Ceiling zaokrouhlí nahoru. Tzn. když chci
             * 30 knížek po dvaceti na stránku, vrátí počet stránek 2.
             */
            ViewBag.Pages = (int)Math.Ceiling((double)totalLessons / (double)itemsOnPage);
            ViewBag.CurrentPage = pg;                       

            // Vyznačení rezervovaných lekcí
            FitnessCentreUser user = new FitnessCentreUserDao().GetByLogin(User.Identity.Name);
            if (user.Role.Identificator == "client")
            {
                IList<Reservation> listClientsReservations = new ReservationDao().GetClientsReservations(user.Id);

                foreach (Lesson lesson in listLessonsPerPage)
                {
                    lesson.IsReserved = false;  // Pro jistotu vyplnění vlastnosti. Zamezení null hodnotám.

                    foreach (Reservation reservation in listClientsReservations)
                    {
                        if (reservation.Lesson.Id == lesson.Id)
                        {
                            lesson.IsReserved = true;
                        }
                    }
                }
            }

            ViewBag.UserId = user.Id;     // Pamatovat si Id přihlášeného uživatele pro rozlišení rezervací
            
            if (user.Role.Identificator == "staff")
            {
                if (Request.IsAjaxRequest())
                    return PartialView("IndexStaff", listLessonsPerPage);
                else
                    return View("IndexStaff", listLessonsPerPage);
            }

            if (user.Role.Identificator == "instructor")
            {
                if (Request.IsAjaxRequest())
                    return PartialView("IndexInstructor", listLessonsPerPage);
                else
                    return View("IndexInstructor", listLessonsPerPage);
            }

            if (Request.IsAjaxRequest())
                return PartialView("IndexClient", listLessonsPerPage);
            else
                return View("IndexClient", listLessonsPerPage);
        }

        /// <summary> Akce pro zobrazení detailních informací o vybrané lekci. </summary>
        /// <param name="id"> Id lekce </param>
        public ActionResult Detail(int id)
        {
            LessonDao lessonDao = new LessonDao();
            Lesson lesson = lessonDao.GetById(id);

            // Získání seznamu registrovaných klientů na vybranou lekci.
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            IList<FitnessCentreUser> listReservedClients = fitnessCentreUserDao.GetReservedClients(id);
            ViewBag.ListReservedClients = listReservedClients;

            FitnessCentreUser user = new FitnessCentreUserDao().GetByLogin(User.Identity.Name);

            if (Request.IsAjaxRequest() && user.Role.Identificator == "staff")
                return PartialView("DetailStaff", lesson);

            if (user.Role.Identificator == "staff")
                return View("DetailStaff", lesson);

            if (Request.IsAjaxRequest())
                return PartialView("Detail", lesson);

            return View("Detail", lesson);
        }

        /*
         * View formuláře pro vytvoření nové lekce.
         * Předání dat pro select list aktivit/místností.
         * Předání přihlášeného instruktora pro výpis v TextBoxu.
         */
        [Authorize(Roles = "instructor, staff")]
        public ActionResult Create()
        {
            ActivityTypeDao activityTypeDao = new ActivityTypeDao();
            IList<ActivityType> listActivityTypes = activityTypeDao.GetAll();
            ViewBag.ListActivityTypes = listActivityTypes;

            RoomDao roomDao = new RoomDao();
            IList<Room> listRooms = roomDao.GetAll();
            ViewBag.ListRooms = listRooms;

            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            //IList<FitnessCentreUser> listInstructors = fitnessCentreUserDao.GetUsersByRole("instructor");
            //ViewBag.ListInstructors = listInstructors;

            FitnessCentreUser instructor = fitnessCentreUserDao.GetByLogin(User.Identity.Name);
            ViewBag.Instructor = instructor;

            return View();
        }

        /*
         * Metoda pro přidání nové lekce dle parametrů vyplněných do formuláře view Create.
         * Přijímá Id objektů komplexních tříd ze select listů.
         */
        [HttpPost]
        [Authorize(Roles = "instructor, staff")]
        public ActionResult Add(string startTime, string endTime, int activityTypeId, int roomId, int originalCapacity)
        {
            Lesson lesson = new Lesson();
            if (ModelState.IsValid)
            {               
                // Nastavení času lekce
                lesson.StartTime = DateTime.ParseExact(startTime, "dd.MM.yyyy H:mm", CultureInfo.CurrentCulture);
                lesson.EndTime = DateTime.ParseExact(endTime, "dd.MM.yyyy H:mm", CultureInfo.CurrentCulture);
                              
                // Přiřazení vybrané aktivity ze select listu vytvářené lekci
                ActivityTypeDao activityTypeDao = new ActivityTypeDao();
                ActivityType activityType = activityTypeDao.GetById(activityTypeId);
                lesson.ActivityType = activityType;

                // Přiřazení vybrané místnosti ze select listu vytvářené lekci
                RoomDao roomDao = new RoomDao();
                Room room = roomDao.GetById(roomId);
                lesson.Room = room;

                // Přiřazení přihlášeného instruktora vytvářené lekci
                FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
                FitnessCentreUser instructor = fitnessCentreUserDao.GetByLogin(User.Identity.Name);
                lesson.Instructor = instructor;

                // Aktuální kapacita lekce je rovna původní kapacitě.
                lesson.OriginalCapacity = originalCapacity;
                lesson.ActualCapacity = originalCapacity;

                lesson.IsActive = true;

                // Vytvoření lekce
                LessonDao lessonDao = new LessonDao();
                lessonDao.Create(lesson);

                TempData["message-success"] = "Lekce " + lesson.ActivityType.Name + " byla úspěšně přidána.";
            }
            else
            {
                TempData["message-error"] = "Lekce nebyla přidána";
                return View("Create", lesson);
            }

            return RedirectToAction("Index", "Lessons", new { isActive = true });
        }

        /*
         * View formuláře pro úpravu zvolené lekce.
         * Předání dat pro select list aktivit/místností/trenérů tentokrát přímo (bez IList).
         * Abychom mohli předvyplnit původně vybrané položky v select listech.
         * Předání vybrané lekce pro úpravu do View.
         */
        [Authorize(Roles = "instructor, staff")]
        public ActionResult Edit(int id)
        {
            ViewBag.ListActivityTypes = new ActivityTypeDao().GetAll();
            ViewBag.ListRooms = new RoomDao().GetAll();

            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            ViewBag.ListInstructors = fitnessCentreUserDao.GetUsersByRole("instructor");

            Lesson lesson = new LessonDao().GetById(id);

            FitnessCentreUser user = fitnessCentreUserDao.GetByLogin(User.Identity.Name);
            if (user.Role.Identificator == "staff")
                return View("EditStaff", lesson);

            return View("EditInstructor", lesson);
        }

        [HttpPost]
        [Authorize(Roles = "instructor, staff")]
        public ActionResult Update(int idLesson, string startTime, string endTime, int activityTypeId, int roomId, int instructorId, int originalCapacity, bool isActive)
        {            
            try
            {
                Lesson lesson = new Lesson();
                lesson.Id = idLesson;

                // Nastavení času lekce
                lesson.StartTime = DateTime.ParseExact(startTime, "dd.MM.yyyy H:mm", CultureInfo.CurrentCulture);
                lesson.EndTime = DateTime.ParseExact(endTime, "dd.MM.yyyy H:mm", CultureInfo.CurrentCulture);

                // Přiřazení vybrané aktivity ze select listu vytvářené lekci
                ActivityTypeDao activityTypeDao = new ActivityTypeDao();
                ActivityType activityType = activityTypeDao.GetById(activityTypeId);
                lesson.ActivityType = activityType;

                // Přiřazení vybrané místnosti ze select listu vytvářené lekci
                RoomDao roomDao = new RoomDao();
                Room room = roomDao.GetById(roomId);
                lesson.Room = room;

                // Přiřazení vybraného trenéra ze select listu vytvářené lekci
                FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
                FitnessCentreUser instructor = fitnessCentreUserDao.GetById(instructorId);
                lesson.Instructor = instructor;

                // Aktuální kapacita lekce je rovna původní kapacitě.
                lesson.OriginalCapacity = originalCapacity;
                lesson.ActualCapacity = originalCapacity;

                lesson.IsActive = isActive;

                // Upravení lekce
                LessonDao lessonDao = new LessonDao();
                lessonDao.Update(lesson);

                TempData["message-success"] = "Lekce " + lesson.ActivityType.Name + " byla úspěšně upravena.";
            }
            catch (Exception)
            {

                throw;
            }

            return RedirectToAction("Index", "Lessons", new { isActive = true });
        }

        [Authorize(Roles = "instructor, staff")]
        public ActionResult Delete(int id)
        {
            try
            {
                LessonDao lessonDao = new LessonDao();
                Lesson lesson = lessonDao.GetById(id);
                lessonDao.Delete(lesson);

                TempData["message-success"] = "Lekce " + lesson.ActivityType.Name + " byla úspěšně smazána.";
            }
            catch (Exception)
            {
                // mechanismus zachytávání výjimek doporučuje dobře si nastudovat
                throw;
            }

            return RedirectToAction("Index", "Lessons", new { isActive = true });
        }
	}
}