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
    public class ActivityTypesController : Controller
    {
        // GET: /Admin/ActivityTypes/
        public ActionResult Index(int? page)
        {       
            const int itemsOnPage = 6;
            int pg = page.HasValue ? page.Value : 1;
            int totalActivityTypes;

            ActivityTypeDao activityTypeDao = new ActivityTypeDao();
            IList<ActivityType> listActivityTypesPerPage = activityTypeDao.GetActivityTypesPaged(itemsOnPage, pg, out totalActivityTypes);

            ViewBag.Pages = (int)Math.Ceiling((double)totalActivityTypes / (double)itemsOnPage);
            ViewBag.CurrentPage = pg;                   

            FitnessCentreUser user = new FitnessCentreUserDao().GetByLogin(User.Identity.Name);
            if (user.Role.Identificator == "staff")
                return View("IndexStaff", listActivityTypesPerPage);

            return View("Index", listActivityTypesPerPage);
        }

        public ActionResult Detail(int id)
        {
            ActivityTypeDao activityTypeDao = new ActivityTypeDao();
            ActivityType activityType = activityTypeDao.GetById(id);

            if (Request.IsAjaxRequest())
                return PartialView(activityType);

            return View(activityType);
        }

        [Authorize(Roles = "staff")]
        public ActionResult Create()
        {
            return View();
        }

        /*
         * Svazování formuláře s modelem.
         * Nastavení ID druhu aktivity dle pořadí v seznamu druhů aktivit.
         * Přidání nového druhu aktivity do seznamu.
         * Zjištění, jestli je model validní. Controller má na sobě objekt ModelState a ten má na sobě vlastnost isValid.
         * Pokud není validní, uživatel dostane předvyplněný formulář zpátky a je varován.
         * Nenavracím pohled akce Add, ale cizí pohled Create, protože ho uživateli znovu vrátím i s daty, které už vyplnil "activityType".
         */
        [Authorize(Roles = "staff")]
        public ActionResult Add(ActivityType activityType, HttpPostedFileBase activityImage)
        {
            if (ModelState.IsValid)
            {
                if (activityImage != null)
                {
                    if (activityImage.ContentType == "image/jpeg" || activityImage.ContentType == "image/png")
                    {
                        Image image = Image.FromStream(activityImage.InputStream);

                        // Velký náhled
                        if (image.Height > 300 || image.Width > 750)
                        {
                            Image smallImage = ImageHelper.ScaleImage(image, 750, 300);     // zmenšení fotografie
                            Bitmap b = new Bitmap(smallImage);

                            Guid guid = Guid.NewGuid();     // vytvoření jména fotografie
                            string imageName = guid.ToString() + ".jpg";

                            b.Save(Server.MapPath("~/uploads/activityImage/" + imageName), ImageFormat.Jpeg);    // uložení fotografie; formát jpg

                            smallImage.Dispose();   // vyčištění po e-disposable objektech
                            b.Dispose();

                            activityType.IllustrationImageName = imageName;   // vyplnění parametru názvu fotografie
                        }
                        else
                        {
                            activityImage.SaveAs(Server.MapPath("~/uploads/activityImage/") + activityImage.FileName);     // uložení v případě, že fotografii není potřeba zmenšovat
                            activityType.IllustrationImageName = activityImage.FileName;   // TomSko asi chybělo vyplnění parametru názvu fotografie
                        }

                        // Zmenšený náhled
                        if (image.Height > 310 || image.Width > 560)
                        {
                            Image smallImage = ImageHelper.ScaleImage(image, 560, 310);     // zmenšení fotografie
                            Bitmap b = new Bitmap(smallImage);

                            Guid guid = Guid.NewGuid();     // vytvoření jména fotografie
                            string thumbImageName = guid.ToString() + ".jpg";

                            b.Save(Server.MapPath("~/uploads/activityThumbImage/" + thumbImageName), ImageFormat.Jpeg);    // uložení fotografie; formát jpg

                            smallImage.Dispose();   // vyčištění po e-disposable objektech
                            b.Dispose();

                            activityType.IllustrationThumbImageName = thumbImageName;   // vyplnění parametru názvu fotografie
                        }
                        else
                        {
                            activityImage.SaveAs(Server.MapPath("~/uploads/activityThumbImage/") + activityImage.FileName);     // uložení v případě, že fotografii není potřeba zmenšovat
                            activityType.IllustrationThumbImageName = activityImage.FileName;   // TomSko asi chybělo vyplnění parametru názvu fotografie
                        }
                    }
                }

                ActivityTypeDao activityTypeDao = new ActivityTypeDao();
                activityTypeDao.Create(activityType);

                TempData["message-success"] = "Aktivita " + activityType.Name + " byla úspěšně přidána";
            }
            else
            {
                TempData["message-error"] = "Aktivita nebyla přidána";
                return View("Create", activityType);
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "staff")]
        public ActionResult Edit(int id)
        {
            ActivityTypeDao activityTypeDao = new ActivityTypeDao();
            ActivityType activityType = activityTypeDao.GetById(id);

            return View(activityType);
        }

        [Authorize(Roles = "staff")]
        public ActionResult Update(ActivityType activityType, HttpPostedFileBase activityImage)
        {
            try
            {
                if (activityImage != null)
                {
                    // TomSko: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                    if (activityType.IllustrationImageName != null)
                        System.IO.File.Delete(Server.MapPath("~/uploads/activityImage/" + activityType.IllustrationImageName));

                    if (activityType.IllustrationThumbImageName != null)
                        System.IO.File.Delete(Server.MapPath("~/uploads/activityThumbImage/" + activityType.IllustrationThumbImageName));

                    if (activityImage.ContentType == "image/jpeg" || activityImage.ContentType == "image/png")
                    {
                        Image image = Image.FromStream(activityImage.InputStream);

                        // Velký náhled
                        if (image.Height > 300 || image.Width > 750)
                        {
                            Image smallImage = ImageHelper.ScaleImage(image, 750, 300);     // zmenšení fotografie
                            Bitmap b = new Bitmap(smallImage);

                            Guid guid = Guid.NewGuid();     // vytvoření jména fotografie
                            string imageName = guid.ToString() + ".jpg";

                            b.Save(Server.MapPath("~/uploads/activityImage/" + imageName), ImageFormat.Jpeg);    // uložení fotografie; formát jpg

                            smallImage.Dispose();   // vyčištění po e-disposable objektech
                            b.Dispose();

                            // TomSko: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                            if (activityType.IllustrationImageName != null)
                                System.IO.File.Delete(Server.MapPath("~/uploads/activityImage/" + activityType.IllustrationImageName));                                                     

                            activityType.IllustrationImageName = imageName;   // vyplnění parametru názvu fotografie
                        }
                        else
                        {
                            // TomSko: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                            if (activityType.IllustrationImageName != null)
                                System.IO.File.Delete(Server.MapPath("~/uploads/activityImage/" + activityType.IllustrationImageName));

                            activityImage.SaveAs(Server.MapPath("~/uploads/activityImage/" + activityImage.FileName));     // uložení v případě, že fotografii není potřeba zmenšovat
                            activityType.IllustrationImageName = activityImage.FileName;   // TomSko asi chybělo vyplnění parametru názvu fotografie
                        }

                        // Zmenšený náhled
                        if (image.Height > 310 || image.Width > 560)
                        {
                            Image smallImage = ImageHelper.ScaleImage(image, 560, 310);     // zmenšení fotografie
                            Bitmap b = new Bitmap(smallImage);

                            Guid guid = Guid.NewGuid();     // vytvoření jména fotografie
                            string thumbImageName = guid.ToString() + ".jpg";

                            b.Save(Server.MapPath("~/uploads/activityThumbImage/" + thumbImageName), ImageFormat.Jpeg);    // uložení fotografie; formát jpg

                            smallImage.Dispose();   // vyčištění po e-disposable objektech
                            b.Dispose();

                            // TomSko: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                            if (activityType.IllustrationThumbImageName != null)
                                System.IO.File.Delete(Server.MapPath("~/uploads/activityThumbImage/" + activityType.IllustrationThumbImageName));
                                                        
                            activityType.IllustrationThumbImageName = thumbImageName;   // vyplnění parametru názvu fotografie
                        }
                        else
                        {
                            // TomSko: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                            if (activityType.IllustrationThumbImageName != null)
                                System.IO.File.Delete(Server.MapPath("~/uploads/activityThumbImage/" + activityType.IllustrationThumbImageName));
                                                      
                            activityImage.SaveAs(Server.MapPath("~/uploads/activityThumbImage/" + activityImage.FileName));     // uložení v případě, že fotografii není potřeba zmenšovat
                            activityType.IllustrationThumbImageName = activityImage.FileName;   // TomSko asi chybělo vyplnění parametru názvu fotografie
                        }
                    }
                }
             

                ActivityTypeDao activityTypeDao = new ActivityTypeDao();               
                activityTypeDao.Update(activityType);

                TempData["message-success"] = "Aktivita " + activityType.Name + " byla úspěšně upravena.";
            }
            catch (Exception)
            {

                throw;
            }

            return RedirectToAction("Index", "ActivityTypes");
        }

        [Authorize(Roles = "staff")]
        public ActionResult Delete(int id)
        {
            try
            {
                ActivityTypeDao activityTypeDao = new ActivityTypeDao();
                ActivityType activityType = activityTypeDao.GetById(id);

                // TomSko: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                if (activityType.IllustrationImageName != null)
                    System.IO.File.Delete(Server.MapPath("~/uploads/activityImage/" + activityType.IllustrationImageName)); 

                // TomSko: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                if (activityType.IllustrationThumbImageName != null)
                    System.IO.File.Delete(Server.MapPath("~/uploads/activityThumbImage/" + activityType.IllustrationThumbImageName));

                activityTypeDao.Delete(activityType);

                TempData["message-success"] = "Aktivita " + activityType.Name + " byla úspěšně smazána.";
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