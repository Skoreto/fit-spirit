using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAccess.Dao;
using DataAccess.Model;
using WebApplication1.Class;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Authorize]
    public class InstructorsController : Controller
    {
        // GET: /Admin/Instructors/
        public ActionResult Index()
        {
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            IList<FitnessCentreUser> listInstructors = fitnessCentreUserDao.GetUsersByRole("instructor");

            FitnessCentreUser user = fitnessCentreUserDao.GetByLogin(User.Identity.Name);

            // Lze smazat pouze instruktory, kteří nevypsali do systému žádnou lekci.
            if (user.Role.Identificator == "staff")
            {
                IList<Lesson> listLessons = new LessonDao().GetAll();

                foreach (FitnessCentreUser instructor in listInstructors)
                {
                    instructor.IsDeletable = true;  // Pro jistotu předvyplnění vlastnosti. Zabránění null hodnotám.

                    foreach (Lesson lesson in listLessons)
                    {
                        if (lesson.Instructor.Id == instructor.Id)
                        {
                            instructor.IsDeletable = false;
                            // break;
                        }
                    }
                }            
             }

            if (user.Role.Identificator == "staff")
                return View("IndexStaff", listInstructors);

            return View("Index", listInstructors);
        }

        public ActionResult Detail(int id)
        {
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            FitnessCentreUser instructor = fitnessCentreUserDao.GetById(id);

            FitnessCentreUser user = fitnessCentreUserDao.GetByLogin(User.Identity.Name);
            if (user.Role.Identificator == "staff")
                return View("DetailStaff", instructor);

            return View("Detail", instructor);
        }

        [Authorize(Roles = "staff")]
        public ActionResult Create()
        {
            return View();
        }

        /*
         * Zjištění, jestli je model validní. Controller má na sobě objekt ModelState a ten má na sobě vlastnost isValid.
         * Nastavení ID trenéra dle pořadí v seznamu druhů aktivit a nastavení jeho dalších parametrů, přesamplování fotky, pokud je to nutné.
         * Přidání nového trenéra do seznamu.
         * Pokud není validní, uživatel dostane předvyplněný formulář zpátky a je varován.
         * Nenavracím pohled akce Add, ale cizí pohled Create, protože ho uživateli znovu vrátím i s daty, které už vyplnil "instructor".
         */
        [HttpPost]
        [Authorize(Roles = "staff")]
        public ActionResult Add(FitnessCentreUser user, HttpPostedFileBase profilePhoto)
        {
            if (ModelState.IsValid)
            {
                if (profilePhoto != null)
                {
                    if (profilePhoto.ContentType == "image/jpeg" || profilePhoto.ContentType == "image/png")
                    {
                        Image image = Image.FromStream(profilePhoto.InputStream);

                        if (image.Height > 200 || image.Width > 200)
                        {
                            Image smallImage = ImageHelper.ScaleImage(image, 200, 200);     // zmenšení fotografie
                            Bitmap b = new Bitmap(smallImage);

                            Guid guid = Guid.NewGuid();     // vytvoření jména fotografie
                            string imageName = guid.ToString() + ".jpg";

                            b.Save(Server.MapPath("~/uploads/profilePhoto/" + imageName), ImageFormat.Jpeg);    // uložení fotografie; formát jpg

                            smallImage.Dispose();   // vyčištění po e-disposable objektech
                            b.Dispose();

                            user.ProfilePhotoName = imageName;   // vyplnění parametru názvu fotografie
                        }
                        else
                        {
                            profilePhoto.SaveAs(Server.MapPath("~/uploads/profilePhoto/") + profilePhoto.FileName);     // uložení v případě, že fotografii není potřeba zmenšovat
                            user.ProfilePhotoName = profilePhoto.FileName;   // TomSko chybělo vyplnění parametru názvu fotografie
                        }
                    }
                }
                else
                {
                    // Pokud není vybrána fotografie, přiřadíme defaultní siluetu.
                    user.ProfilePhotoName = "manSilhouette.png";
                }

                //ProfilePhotoHelper.AddProfilePhoto(user, profilePhoto);

                FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
                FitnessCentreRoleDao fitnessCentreRoleDao = new FitnessCentreRoleDao();

                // == TVORBA LOGINU ==
                // Spoj prvních 5 písmen z příjmení s prvními 2 písmeny ze jména uživatele. Převeď string na malá písmena.
                string loginName = user.LastName.ToLowerInvariant().Substring(0, 5) + user.FirstName.ToLowerInvariant().Substring(0, 2);
                string cleanLoginName = Utilities.RemoveDiacritics(loginName); // Odstraň ze stringu diakritiku.

                // Za každého uživatele se stejným cleanLoginName zvyš loginNumber o 1.
                int loginNumber = 1;
                IList<FitnessCentreUser> listUsers = fitnessCentreUserDao.GetAll();
                foreach (FitnessCentreUser u in listUsers)
                {
                    if (u.Login.Substring(0, 7).Equals(cleanLoginName))
                        loginNumber++;
                }

                // Vytvoř Login spojením cleanLoginName a loginNumber.
                user.Login = cleanLoginName + loginNumber.ToString();

                user.Credit = 0;      // Nastavíme počáteční kredit 0 Kč
                user.Role = fitnessCentreRoleDao.GetById(2);    // Přiřadíme uživateli roli instruktora (vybereme ze seznamu rolí podle RoleId == 2)
                user.IsActive = true;
                fitnessCentreUserDao.Create(user);

                /*
                 * Notifikace uživatele o tom, že trenér byl úspěšně přidán.
                 * TempData - proměnná, která má platnost v rámci jednoho cyklu dotaz-odpověď. Vytvoříme klíč message-success.
                 */
                TempData["message-success"] = "Instruktor byl úspěšně přidán. Login: " + user.Login;
            }
            else
            {
                TempData["message-error"] = "Instruktor nebyl přidán"; // Notifikace uživatele o tom, že instruktor nebyl přidán. Při vyplém JavaScriptu.
                return View("Create", user);
            }

            return RedirectToAction("Index");   // přesměrování na Index tak, aby nešel po F5 refreshi znovu odeslat formulář
        }

        [Authorize(Roles = "staff")]
        public ActionResult Edit(int id)
        {
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            FitnessCentreUser user = fitnessCentreUserDao.GetById(id);

            return View(user);
        }

        /*
         * Akce pro upravení údajů vybraného instruktora.
         */
        [HttpPost]
        [Authorize(Roles = "staff")]
        public ActionResult Update(FitnessCentreUser user, HttpPostedFileBase profilePhoto)
        {
            try
            {
                FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
                FitnessCentreRoleDao fitnessCentreRoleDao = new FitnessCentreRoleDao();

                if (profilePhoto != null)
                {
                    if (profilePhoto.ContentType == "image/jpeg" || profilePhoto.ContentType == "image/png")
                    {
                        Image image = Image.FromStream(profilePhoto.InputStream);

                        Guid guid = Guid.NewGuid();
                        string imageName = guid.ToString() + ".jpg";

                        if (image.Height > 200 || image.Width > 200)
                        {
                            Image smallImage = ImageHelper.ScaleImage(image, 200, 200);
                            Bitmap b = new Bitmap(smallImage);

                            b.Save(Server.MapPath("~/uploads/profilePhoto/" + imageName), ImageFormat.Jpeg);

                            smallImage.Dispose();
                            b.Dispose();

                            // TomSko přesunuto: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                            System.IO.File.Delete(Server.MapPath("~/uploads/profilePhoto/" + user.ProfilePhotoName));

                            // TomSko přesunuto: přiřadíme nový soubor, který už je nahraný
                            user.ProfilePhotoName = imageName;
                        }
                        else
                        {
                            // TomSko přesunuto: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                            System.IO.File.Delete(Server.MapPath("~/uploads/profilePhoto/" + user.ProfilePhotoName));
                            profilePhoto.SaveAs(Server.MapPath("~/uploads/profilePhoto/") + profilePhoto.FileName);
                            user.ProfilePhotoName = profilePhoto.FileName;   // TomSko chybělo vyplnění parametru názvu fotografie
                        }
                    }
                }

                user.Role = fitnessCentreRoleDao.GetById(2);
                fitnessCentreUserDao.Update(user);

                TempData["message-success"] = "Instruktor " + user.FirstName + " " + user.LastName + " byl úspěšně upraven.";
            }
            catch (Exception)
            {

                throw;
            }

            return RedirectToAction("Index", "Instructors");
        }

        /// <summary> Akce pro deaktivaci účtu vybraného instruktora </summary>
        /// <param name="id"> Id vybraného instruktora </param>
        [Authorize(Roles = "staff")]
        public ActionResult Deactivate(int id)
        {
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            FitnessCentreUser instructor = fitnessCentreUserDao.GetById(id);

            LessonDao lessonDao = new LessonDao();
            IList<Lesson> listLessons = lessonDao.GetAll();

            // Automatická deaktivace všech lekcí daného instruktora
            foreach (Lesson lesson in listLessons)
            {
                if (lesson.Instructor.Id == instructor.Id)
                {
                    lesson.IsActive = false;
                    lessonDao.Update(lesson);
                }              
            }

            TempData["message-success"] = "Účet instruktora " + instructor.FirstName + " " + instructor.LastName + " byl úspěšně deaktivován.";

            // Deaktivace účtu instruktora
            instructor.IsActive = false;
            fitnessCentreUserDao.Update(instructor);

            return RedirectToAction("Index");
        }

        /// <summary> Akce pro aktivaci účtu vybraného instruktora </summary>
        /// <param name="id"> Id vybraného instruktora </param>
        [Authorize(Roles = "staff")]
        public ActionResult Activate(int id)
        {
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            FitnessCentreUser instructor = fitnessCentreUserDao.GetById(id);

            LessonDao lessonDao = new LessonDao();
            IList<Lesson> listLessons = lessonDao.GetAll();

            // Automatická aktivace všech lekcí daného instruktora, které se ještě neuskutečnily.
            foreach (Lesson lesson in listLessons)
            {
                // Porovnání CompareTo vrací hodnoty -1, 0, 1. Pokud aktuální čas je dřívěji než zahájení lekce.
                if (lesson.Instructor.Id == instructor.Id && DateTime.Now.CompareTo(lesson.StartTime) < 0)
                {
                    lesson.IsActive = true;
                    lessonDao.Update(lesson);
                }
            }

            TempData["message-success"] = "Účet instruktora " + instructor.FirstName + " " + instructor.LastName + " byl úspěšně aktivován.";

            // Aktivace účtu instruktora
            instructor.IsActive = true;
            fitnessCentreUserDao.Update(instructor);

            return RedirectToAction("Index");
        }

        /// <summary> Akce pro smazání účtu vybraného instruktora </summary>
        /// <param name="id"> Id vybraného instruktora </param>
        [Authorize(Roles = "staff")]
        public ActionResult Delete(int id)
        {
            try
            {
                FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
                FitnessCentreUser instructor = fitnessCentreUserDao.GetById(id);

                // Pokud uživatel neměl nastavenu pouze defaultní fotografii, ještě před smazáním uživatele, smaž jeho fotografii.
                if (!instructor.ProfilePhotoName.Equals("manSilhouette.png"))
                    System.IO.File.Delete(Server.MapPath("~/uploads/profilePhoto/" + instructor.ProfilePhotoName));
              
                TempData["message-success"] = "Instruktor " + instructor.FirstName + " " + instructor.LastName + " byl úspěšně smazán.";

                fitnessCentreUserDao.Delete(instructor);
            }
            catch (Exception)
            {
                // mechanismus zachytávání výjimek doporučuje dobře si nastudovat
                throw;
            }

            return RedirectToAction("Index");
        }
	}
}