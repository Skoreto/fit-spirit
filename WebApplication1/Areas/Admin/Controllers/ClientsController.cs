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
    public class ClientsController : Controller
    {
        // GET: /Admin/Clients/

        /// <summary> 
        /// Akce pro stránkování při filtraci klientů dle zadané fráze.
        /// Standardně fráze prázdná => nalezne všechny klienty.
        /// </summary>
        /// <param name="phrase">Filtrovaná fráze</param>
        /// <param name="page">Zvolená stránka</param>
        public ActionResult Index(string phrase, int? page)
        {
            const int itemsOnPage = 10;
            int pg = page.HasValue ? page.Value : 1;
            int totalClientsFound;

            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            IList<FitnessCentreUser> listClientsFoundPerPage = fitnessCentreUserDao.SearchClientsPaged(phrase, itemsOnPage, pg, out totalClientsFound);

            ViewBag.PagesSearched = (int)Math.Ceiling((double)totalClientsFound / (double)itemsOnPage);
            ViewBag.CurrentPageSearched = pg;
            ViewBag.CurrentPhraseSearched = phrase; // Předám dalším stránkám frázi, která je vyhledávána.

            if (Request.IsAjaxRequest())
                return PartialView("Index", listClientsFoundPerPage);

            return View("Index", listClientsFoundPerPage);
        }

        /// <summary> Akce pro nápovědu při zadávání vyhledávané fráze. </summary>
        /// <param name="query">Rozepsaná fráze</param>
        public JsonResult SearchClients(string query)
        {
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            IList<FitnessCentreUser> listClients = fitnessCentreUserDao.SearchClients(query);

            // Chceme vrátit IEnumerable, ToList() implementuje IEnumerable a vrátí nám to kolekci stringů.
            List<string> names = (from FitnessCentreUser client in listClients select client.LastName).ToList();

            return Json(names, JsonRequestBehavior.AllowGet);
        } 

        public ActionResult Detail(int id)
        {
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            FitnessCentreUser user = fitnessCentreUserDao.GetById(id);

            return View(user);
        }

        public ActionResult Edit(int id)
        {
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            FitnessCentreUser user = fitnessCentreUserDao.GetById(id);

            return View(user);
        }

        /// <summary> Akce pro upravení údajů vybraného klienta. </summary>
        /// <param name="user">Předání vyplněného modelu uživatele</param>
        /// <param name="profilePhoto">Nahraná fotografie</param>
        [HttpPost]
        [Authorize(Roles = "staff")]
        public ActionResult Update(FitnessCentreUser user, HttpPostedFileBase profilePhoto)
        {
            try
            {
                if (profilePhoto != null)
                {
                    if (profilePhoto.ContentType == "image/jpeg" || profilePhoto.ContentType == "image/png")
                    {
                        Image image = Image.FromStream(profilePhoto.InputStream);
                      
                        if (image.Height > 200 || image.Width > 200)
                        {
                            Image smallImage = ImageHelper.ScaleImage(image, 200, 200);
                            Bitmap b = new Bitmap(smallImage);

                            Guid guid = Guid.NewGuid();
                            string imageName = guid.ToString() + ".jpg";

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

                FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
                FitnessCentreRoleDao fitnessCentreRoleDao = new FitnessCentreRoleDao();
                user.Role = fitnessCentreRoleDao.GetById(3);
                user.IsActive = true;
                fitnessCentreUserDao.Update(user);

                TempData["message-success"] = "Klient " + user.FirstName + " " + user.LastName + " byl úspěšně upraven.";
            }
            catch (Exception)
            {
                throw;
            }

            return RedirectToAction("Index", "Clients");
        }

        /// <summary> Akce pro aktivaci účtu vybraného klienta </summary>
        /// <param name="id"> Id vybraného klienta </param>
        [Authorize(Roles = "staff")]
        public ActionResult Activate(int id)
        {
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            FitnessCentreUser client = fitnessCentreUserDao.GetById(id);

            client.IsActive = true;
            fitnessCentreUserDao.Update(client);

            TempData["message-success"] = "Účet klienta " + client.FirstName + " " + client.LastName + " byl úspěšně aktivován.";

            return RedirectToAction("Index");
        }

        /// <summary> Akce pro smazání vybraného klienta. </summary>
        /// <param name="id">Id vybraného klienta</param>
        [Authorize(Roles = "staff")]
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

                TempData["message-success"] = "Klient " + user.FirstName + " " + user.LastName + " byl úspěšně smazán.";
            }
            catch (Exception)
            {
                throw;
            }

            return RedirectToAction("Index");
        }
	}
}