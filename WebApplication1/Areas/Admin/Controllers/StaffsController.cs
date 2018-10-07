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
    public class StaffsController : Controller
    {
        // GET: /Admin/Staffs/
        [Authorize(Roles = "staff")]
        public ActionResult Index()
        {
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            IList<FitnessCentreUser> listStaffs = fitnessCentreUserDao.GetUsersByRole("staff");

            return View(listStaffs);
        }

        [Authorize(Roles = "staff")]
        public ActionResult Detail(int id)
        {
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            FitnessCentreUser user = fitnessCentreUserDao.GetById(id);

            return View(user);
        }

        public ActionResult Create()
        {
            return View();
        }

        /*
         * Akce pro přidání nové obsluhy.
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
                        }
                    }
                }
                else
                {
                    // Pokud není vybrána fotografie, přiřadíme defaultní siluetu.
                    user.ProfilePhotoName = "manSilhouette.png";
                }

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
                user.Role = fitnessCentreRoleDao.GetById(1);    // Přiřadíme uživateli roli obsluhy (vybereme ze seznamu rolí podle RoleId == 1)
                user.IsActive = true;
                fitnessCentreUserDao.Create(user);

                /*
                 * Notifikace uživatele o tom, že trenér byl úspěšně přidán.
                 * TempData - proměnná, která má platnost v rámci jednoho cyklu dotaz-odpověď. Vytvoříme klíč message-success.
                 */
                TempData["message-success"] = "Obsluha byla úspěšně přidána. Login: " + user.Login;
            }
            else
            {
                TempData["message-error"] = "Obsluha nebyla přidána";
                return View("Create", user);
            }

            return RedirectToAction("Index");   // přesměrování na Index tak, aby nešel po F5 refreshi znovu odeslat formulář
        }

        public ActionResult Edit(int id)
        {
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            FitnessCentreUser user = fitnessCentreUserDao.GetById(id);

            return View(user);
        }

        /*
         * Akce pro upravení údajů vybraného klienta.
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
                            user.ProfilePhotoName = profilePhoto.FileName;   // TomSko asi chybělo vyplnění parametru názvu fotografie
                        }
                    }
                }

                user.Role = fitnessCentreRoleDao.GetById(1);
                fitnessCentreUserDao.Update(user);

                TempData["message-success"] = "Obsluha " + user.FirstName + " " + user.LastName + " byla úspěšně upravena.";
            }
            catch (Exception)
            {

                throw;
            }

            return RedirectToAction("Index", "Staffs");
        }

        public ActionResult Delete(int id)
        {
            try
            {
                FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
                FitnessCentreUser user = fitnessCentreUserDao.GetById(id);

                // Pokud uživatel neměl nastavenu pouze defaultní fotografii, ještě před smazáním uživatele, smaž jeho fotografii.
                if (!user.ProfilePhotoName.Equals("manSilhouette.png"))
                    System.IO.File.Delete(Server.MapPath("~/uploads/profilePhoto/" + user.ProfilePhotoName));

                fitnessCentreUserDao.Delete(user);

                TempData["message-success"] = "Obsluha " + user.FirstName + " " + user.LastName + " byla úspěšně smazána.";
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