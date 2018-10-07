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
    public class GalleryController : Controller
    {
        // GET: /Admin/Gallery/
        public ActionResult Index(int? page)
        {
            const int itemsOnPage = 9;
            int pg = page.HasValue ? page.Value : 1;
            int totalPhotographs;

            PhotographyDao photographyDao = new PhotographyDao();
            IList<Photography> listPhotographsPerPage = photographyDao.GetPhotographsPaged(itemsOnPage, pg, out totalPhotographs);

            ViewBag.Pages = (int)Math.Ceiling((double)totalPhotographs / (double)itemsOnPage);
            ViewBag.CurrentPage = pg;

            FitnessCentreUser user = new FitnessCentreUserDao().GetByLogin(User.Identity.Name);
            if (user.Role.Identificator == "staff")
                return View("IndexStaff", listPhotographsPerPage);

            return View("Index", listPhotographsPerPage);
        }

        [Authorize(Roles = "staff")]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "staff")]
        public ActionResult Add(Photography photography, HttpPostedFileBase photographyImage)
        {
            if (ModelState.IsValid)
            {
                if (photographyImage != null)
                {
                    if (photographyImage.ContentType == "image/jpeg" || photographyImage.ContentType == "image/png")
                    {
                        Image image = Image.FromStream(photographyImage.InputStream);

                        // Velký náhled
                        if (image.Height > 1080 || image.Width > 1920)
                        {
                            Image smallImage = ImageHelper.ScaleImage(image, 1920, 1080);     // zmenšení fotografie
                            Bitmap b = new Bitmap(smallImage);

                            Guid guid = Guid.NewGuid();     // vytvoření jména fotografie
                            string imageName = guid.ToString() + ".jpg";

                            b.Save(Server.MapPath("~/uploads/photographyImage/" + imageName), ImageFormat.Jpeg);    // uložení fotografie; formát jpg

                            smallImage.Dispose();   // vyčištění po e-disposable objektech
                            b.Dispose();

                            photography.IllustrationImageName = imageName;   // vyplnění parametru názvu fotografie
                        }
                        else
                        {
                            photographyImage.SaveAs(Server.MapPath("~/uploads/photographyImage/") + photographyImage.FileName);     // uložení v případě, že fotografii není potřeba zmenšovat
                            photography.IllustrationImageName = photographyImage.FileName;   // TomSko asi chybělo vyplnění parametru názvu fotografie
                        }

                        // Zmenšený náhled
                        if (image.Height > 310 || image.Width > 560)
                        {
                            Image smallImage = ImageHelper.ScaleImage(image, 560, 310);     // zmenšení fotografie
                            Bitmap b = new Bitmap(smallImage);

                            Guid guid = Guid.NewGuid();     // vytvoření jména fotografie
                            string thumbImageName = guid.ToString() + ".jpg";

                            b.Save(Server.MapPath("~/uploads/photographyThumbImage/" + thumbImageName), ImageFormat.Jpeg);    // uložení fotografie; formát jpg

                            smallImage.Dispose();   // vyčištění po e-disposable objektech
                            b.Dispose();

                            photography.IllustrationThumbImageName = thumbImageName;   // vyplnění parametru názvu fotografie
                        }
                        else
                        {
                            photographyImage.SaveAs(Server.MapPath("~/uploads/photographyThumbImage/") + photographyImage.FileName);     // uložení v případě, že fotografii není potřeba zmenšovat
                            photography.IllustrationThumbImageName = photographyImage.FileName;   // TomSko asi chybělo vyplnění parametru názvu fotografie
                        }
                    }
                }
                else
                {
                    // Pokud nebyl nalinkován nový obrázek, nastav cesty k obrázkům místnosti na null
                    photography.IllustrationImageName = null;
                    photography.IllustrationThumbImageName = null;
                }

                PhotographyDao photographyDao = new PhotographyDao();
                photographyDao.Create(photography);

                TempData["message-success"] = "Fotografie byla úspěšně přidána";
            }
            else
            {
                TempData["message-error"] = "Fotografie nebyla přidána";
                return View("Create", photography);
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "staff")]
        public ActionResult Delete(int id)
        {
            try
            {
                PhotographyDao photographyDao = new PhotographyDao();
                Photography photography = photographyDao.GetById(id);

                // TomSko: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                if (photography.IllustrationImageName != null)
                    System.IO.File.Delete(Server.MapPath("~/uploads/photographyImage/" + photography.IllustrationImageName));

                // TomSko: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                if (photography.IllustrationThumbImageName != null)
                    System.IO.File.Delete(Server.MapPath("~/uploads/photographyThumbImage/" + photography.IllustrationThumbImageName));

                photographyDao.Delete(photography);

                TempData["message-success"] = "Fotografie byla úspěšně smazána.";
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