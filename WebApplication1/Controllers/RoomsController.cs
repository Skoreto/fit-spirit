using System;
using DataAccess.Model;
using System.Web.Mvc;
using DataAccess.Dao;
using System.Collections.Generic;

namespace WebApplication1.Controllers
{
    public class RoomsController : Controller
    {
        // GET: /Rooms/
        public ActionResult Index(int? page)
        {
            const int itemsOnPage = 6;
            int pg = page.HasValue ? page.Value : 1;
            int totalRooms;

            RoomDao roomDao = new RoomDao();
            IList<Room> listRoomsPerPage = roomDao.GetRoomsPaged(itemsOnPage, pg, out totalRooms);

            ViewBag.Pages = (int)Math.Ceiling((double)totalRooms / (double)itemsOnPage);
            ViewBag.CurrentPage = pg;

            return View("Index", listRoomsPerPage);
        }
	}
}