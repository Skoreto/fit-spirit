using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAccess.Dao;
using DataAccess.Model;

namespace WebApplication1.Areas.Admin.Controllers
{   
    [Authorize]
    public class SideBarController : Controller
    {
        // GET: /Admin/SideBar/
        public ActionResult EventsBarIndex()
        {
            LessonDao lessonDao = new LessonDao();
            IList<Lesson> listLessons = lessonDao.GetRestrictedLessons(true);

            return View(listLessons);
        }

        public ActionResult ProfileBarAuthenticated()
        {
            FitnessCentreUser user = new FitnessCentreUserDao().GetByLogin(User.Identity.Name);
            return View(user);                    
        }
	}
}