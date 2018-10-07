using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAccess.Dao;
using DataAccess.Model;

namespace WebApplication1.Controllers
{
    public class OthersController : Controller
    {
        // GET: Others
        public ActionResult ProjectObjective()
        {
            return View();
        }

        public ActionResult ImplementedFunctions()
        {
            return View();
        }

        public ActionResult PriceList(int? page)
        {
            const int itemsOnPage = 15;
            int pg = page.HasValue ? page.Value : 1;
            int totalActivityTypes;

            ActivityTypeDao activityTypeDao = new ActivityTypeDao();
            IList<ActivityType> listActivityTypesPerPage = activityTypeDao.GetActivityTypesPaged(itemsOnPage, pg, out totalActivityTypes);

            ViewBag.Pages = (int)Math.Ceiling((double)totalActivityTypes / (double)itemsOnPage);
            ViewBag.CurrentPage = pg;

            return View("PriceList", listActivityTypesPerPage);
        }
    }
}