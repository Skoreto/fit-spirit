using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAccess.Dao;
using DataAccess.Model;

namespace WebApplication1.Controllers
{
    public class SideBarController : Controller
    {
        // GET: SideBar
        public ActionResult EventsBarIndex()
        {
            LessonDao lessonDao = new LessonDao();
            IList<Lesson> listLessons = lessonDao.GetRestrictedLessons(true);

            return View(listLessons);
        }

        public ActionResult ProfileBarUnauthenticated()
        {
            return View();
        }
    }
}