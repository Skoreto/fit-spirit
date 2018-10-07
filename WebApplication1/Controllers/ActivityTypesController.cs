using DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAccess.Dao;

namespace WebApplication1.Controllers
{
    public class ActivityTypesController : Controller
    {
        // GET: /ActivityTypes/
        public ActionResult Index(int? page)
        {
            const int itemsOnPage = 6;
            int pg = page.HasValue ? page.Value : 1;
            int totalActivityTypes;

            ActivityTypeDao activityTypeDao = new ActivityTypeDao();
            IList<ActivityType> listActivityTypesPerPage = activityTypeDao.GetActivityTypesPaged(itemsOnPage, pg, out totalActivityTypes);

            ViewBag.Pages = (int)Math.Ceiling((double)totalActivityTypes / (double)itemsOnPage);
            ViewBag.CurrentPage = pg;

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
	}
}