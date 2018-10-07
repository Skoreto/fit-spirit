using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using DataAccess.Dao;
using DataAccess.Model;
using WebApplication1.Class;

namespace WebApplication1.Controllers
{
    public class RegistrationController : Controller
    {
        // GET: Registration
        public ActionResult Create()
        {
            return View();
        }

        /*
         * Akce pro přidání nového klienta.
         * Zjištění, jestli je model validní. Controller má na sobě objekt ModelState a ten má na sobě vlastnost isValid.
         * Nastavení ID trenéra dle pořadí v seznamu druhů aktivit a nastavení jeho dalších parametrů, přesamplování fotky, pokud je to nutné.
         * Přidání nového trenéra do seznamu.
         * Pokud není validní, uživatel dostane předvyplněný formulář zpátky a je varován.
         * Nenavracím pohled akce Add, ale cizí pohled Create, protože ho uživateli znovu vrátím i s daty, které už vyplnil "instructor".
         */
        [HttpPost]
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
                            // BUG asi chyby v závorkách a mazání null obrázku viz. activityController
                            profilePhoto.SaveAs(Server.MapPath("~/uploads/profilePhoto/") + profilePhoto.FileName);     // uložení v případě, že fotografii není potřeba zmenšovat
                            user.ProfilePhotoName = profilePhoto.FileName;   // TomSko asi chybělo vyplnění parametru názvu fotografie
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
                user.Role = fitnessCentreRoleDao.GetById(3);    // Přiřadíme uživateli roli klienta (vybereme ze seznamu rolí podle RoleId == 3)
                user.IsActive = false;
                fitnessCentreUserDao.Create(user);
            }
            else
            {
                TempData["message-error"] = "Klient nebyl přidán";
                return View("Create", user);
            }

            return RedirectToAction("Success", user);   // přesměrování na jinou stránku tak, aby nešel po F5 refreshi znovu odeslat formulář
        }

        
        public ActionResult Success(FitnessCentreUser user)
        {
            return View(user);
        }
    }
}