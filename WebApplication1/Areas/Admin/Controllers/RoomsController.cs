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
    public class RoomsController : Controller
    {
        // GET: /Admin/Rooms/
        public ActionResult Index(int? page)
        {
            const int itemsOnPage = 6;
            int pg = page.HasValue ? page.Value : 1;
            int totalRooms;

            RoomDao roomDao = new RoomDao();
            IList<Room> listRoomsPerPage = roomDao.GetRoomsPaged(itemsOnPage, pg, out totalRooms);

            ViewBag.Pages = (int)Math.Ceiling((double)totalRooms / (double)itemsOnPage);
            ViewBag.CurrentPage = pg;

            FitnessCentreUser user = new FitnessCentreUserDao().GetByLogin(User.Identity.Name);
            if (user.Role.Identificator == "staff")
                return View("IndexStaff", listRoomsPerPage);

            return View("Index", listRoomsPerPage);
        }

        [Authorize(Roles = "staff")]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "staff")]
        public ActionResult AddRoom(Room room, HttpPostedFileBase roomImage)
        {
            if (ModelState.IsValid)
            {
                if (roomImage != null)
                {
                    if (roomImage.ContentType == "image/jpeg" || roomImage.ContentType == "image/png")
                    {
                        Image image = Image.FromStream(roomImage.InputStream);

                        // Velký náhled
                        if (image.Height > 300 || image.Width > 750)
                        {
                            Image smallImage = ImageHelper.ScaleImage(image, 750, 300);     // zmenšení fotografie
                            Bitmap b = new Bitmap(smallImage);

                            Guid guid = Guid.NewGuid();     // vytvoření jména fotografie
                            string imageName = guid.ToString() + ".jpg";

                            b.Save(Server.MapPath("~/uploads/roomImage/" + imageName), ImageFormat.Jpeg);    // uložení fotografie; formát jpg

                            smallImage.Dispose();   // vyčištění po e-disposable objektech
                            b.Dispose();

                            room.IllustrationImageName = imageName;   // vyplnění parametru názvu fotografie
                        }
                        else
                        {
                            roomImage.SaveAs(Server.MapPath("~/uploads/roomImage/") + roomImage.FileName);     // uložení v případě, že fotografii není potřeba zmenšovat
                            room.IllustrationImageName = roomImage.FileName;   // TomSko asi chybělo vyplnění parametru názvu fotografie
                        }

                        // Zmenšený náhled
                        if (image.Height > 310 || image.Width > 560)
                        {
                            Image smallImage = ImageHelper.ScaleImage(image, 560, 310);     // zmenšení fotografie
                            Bitmap b = new Bitmap(smallImage);

                            Guid guid = Guid.NewGuid();     // vytvoření jména fotografie
                            string thumbImageName = guid.ToString() + ".jpg";

                            b.Save(Server.MapPath("~/uploads/roomThumbImage/" + thumbImageName), ImageFormat.Jpeg);    // uložení fotografie; formát jpg

                            smallImage.Dispose();   // vyčištění po e-disposable objektech
                            b.Dispose();

                            room.IllustrationThumbImageName = thumbImageName;   // vyplnění parametru názvu fotografie
                        }
                        else
                        {
                            roomImage.SaveAs(Server.MapPath("~/uploads/roomThumbImage/") + roomImage.FileName);     // uložení v případě, že fotografii není potřeba zmenšovat
                            room.IllustrationThumbImageName = roomImage.FileName;   // TomSko asi chybělo vyplnění parametru názvu fotografie
                        }
                    }
                }
                else
                {
                    // Pokud nebyl nalinkován nový obrázek, nastav cesty k obrázkům místnosti na null
                    room.IllustrationImageName = null;
                    room.IllustrationThumbImageName = null;
                }

                RoomDao roomDao = new RoomDao();
                roomDao.Create(room);

                TempData["message-success"] = "Místnost " + room.Name + " byla úspěšně přidána";
            }
            else
            {
                TempData["message-error"] = "Místnost nebyla přidána";
                return View("Create", room);
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "staff")]
        public ActionResult Edit(int id)
        {
            RoomDao roomDao = new RoomDao();
            Room room = roomDao.GetById(id);

            return View(room);
        }

        [Authorize(Roles = "staff")]
        public ActionResult Update(Room room, HttpPostedFileBase roomImage)
        {
            try
            {
                if (roomImage != null)
                {
                    // TomSko: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                    if (room.IllustrationImageName != null)
                        System.IO.File.Delete(Server.MapPath("~/uploads/roomImage/" + room.IllustrationImageName));

                    if (room.IllustrationThumbImageName != null)
                        System.IO.File.Delete(Server.MapPath("~/uploads/roomThumbImage/" + room.IllustrationThumbImageName));

                    if (roomImage.ContentType == "image/jpeg" || roomImage.ContentType == "image/png")
                    {
                        Image image = Image.FromStream(roomImage.InputStream);

                        // Velký náhled 16:9
                        if (image.Height > 576 || image.Width > 1024)
                        {
                            Image smallImage = ImageHelper.ScaleImage(image, 1024, 576);     // zmenšení fotografie
                            Bitmap b = new Bitmap(smallImage);

                            Guid guid = Guid.NewGuid();     // vytvoření jména fotografie
                            string imageName = guid.ToString() + ".jpg";

                            b.Save(Server.MapPath("~/uploads/roomImage/" + imageName), ImageFormat.Jpeg);    // uložení fotografie; formát jpg

                            smallImage.Dispose();   // vyčištění po e-disposable objektech
                            b.Dispose();

                            // TomSko: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                            if (room.IllustrationImageName != null)
                                System.IO.File.Delete(Server.MapPath("~/uploads/roomImage/" + room.IllustrationImageName));

                            room.IllustrationImageName = imageName;   // vyplnění parametru názvu fotografie
                        }
                        else
                        {
                            // TomSko: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                            if (room.IllustrationImageName != null)
                                System.IO.File.Delete(Server.MapPath("~/uploads/roomImage/" + room.IllustrationImageName));

                            roomImage.SaveAs(Server.MapPath("~/uploads/roomImage/" + roomImage.FileName));     // uložení v případě, že fotografii není potřeba zmenšovat
                            room.IllustrationImageName = roomImage.FileName;   // TomSko asi chybělo vyplnění parametru názvu fotografie
                        }

                        // Zmenšený náhled
                        if (image.Height > 310 || image.Width > 560)
                        {
                            Image smallImage = ImageHelper.ScaleImage(image, 560, 310);     // zmenšení fotografie
                            Bitmap b = new Bitmap(smallImage);

                            Guid guid = Guid.NewGuid();     // vytvoření jména fotografie
                            string thumbImageName = guid.ToString() + ".jpg";

                            b.Save(Server.MapPath("~/uploads/roomThumbImage/" + thumbImageName), ImageFormat.Jpeg);    // uložení fotografie; formát jpg

                            smallImage.Dispose();   // vyčištění po e-disposable objektech
                            b.Dispose();

                            // TomSko: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                            if (room.IllustrationThumbImageName != null)
                                System.IO.File.Delete(Server.MapPath("~/uploads/roomThumbImage/" + room.IllustrationThumbImageName));

                            room.IllustrationThumbImageName = thumbImageName;   // vyplnění parametru názvu fotografie
                        }
                        else
                        {
                            // TomSko: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                            if (room.IllustrationThumbImageName != null)
                                System.IO.File.Delete(Server.MapPath("~/uploads/roomThumbImage/" + room.IllustrationThumbImageName));

                            roomImage.SaveAs(Server.MapPath("~/uploads/roomThumbImage/" + roomImage.FileName));     // uložení v případě, že fotografii není potřeba zmenšovat
                            room.IllustrationThumbImageName = roomImage.FileName;   // TomSko asi chybělo vyplnění parametru názvu fotografie
                        }
                    }
                }
                else
                {
                    // Pokud nebyl nalinkován nový obrázek, smaž původní obrázky a nastav cesty k obrázkům místnosti na null
                    if (room.IllustrationImageName != null)
                        System.IO.File.Delete(Server.MapPath("~/uploads/roomImage/" + room.IllustrationImageName));

                    if (room.IllustrationThumbImageName != null)
                        System.IO.File.Delete(Server.MapPath("~/uploads/roomThumbImage/" + room.IllustrationThumbImageName));

                    room.IllustrationImageName = null;
                    room.IllustrationThumbImageName = null;
                }

                RoomDao roomDao = new RoomDao();
                roomDao.Update(room);

                TempData["message-success"] = "Místnost " + room.Name + " byla úspěšně upravena.";
            }
            catch (Exception)
            {

                throw;
            }

            return RedirectToAction("Index", "Rooms");
        }

        public ActionResult Delete(int id)
        {
            try
            {
                RoomDao roomDao = new RoomDao();
                Room room = roomDao.GetById(id);

                // TomSko: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                if (room.IllustrationImageName != null)
                    System.IO.File.Delete(Server.MapPath("~/uploads/roomImage/" + room.IllustrationImageName));

                // TomSko: ještě předtím, než vyčistím jméno, je potřeba, abych smazal starý soubor
                if (room.IllustrationThumbImageName != null)
                    System.IO.File.Delete(Server.MapPath("~/uploads/roomThumbImage/" + room.IllustrationThumbImageName));

                roomDao.Delete(room);

                TempData["message-success"] = "Místnost " + room.Name + " byla úspěšně smazána.";
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