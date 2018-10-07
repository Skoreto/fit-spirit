using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAccess.Dao;
using DataAccess.Model;

namespace WebApplication1.Controllers
{
    
    public class LessonsController : Controller
    {
        // GET: /Lessons/

        /// <summary> Akce pro zobrazení seznamu lekcí vybrané aktivity </summary>
        /// <param name="isActive">výběr lekcí aktivních/uplynulých nebo všech (null)</param>
        /// <param name="activityTypeId">Id vybrané aktivity</param>
        /// <param name="page">číslo strany výpisu</param>
        public ActionResult Index(bool? isActive, int? activityTypeId, int? page)
        {
            // Nastavení uskutečněných lekcí jako neaktivních
            LessonDao lessonDao = new LessonDao();
            lessonDao.SetExpiredLessons();

            // Pro výpis labelů filtru aktivit.
            ViewBag.ActivityTypes = new ActivityTypeDao().GetAll();

            const int itemsOnPage = 10;
            int pg = page.HasValue ? page.Value : 1;
            int totalLessons;

            // Větvení, zda je požadováno třídění dle aktivních/uplynulých či druhů aktivit lekcí.
            IList<Lesson> listLessonsPerPage;
            if (isActive.HasValue)
            {
                if (activityTypeId.HasValue)
                    listLessonsPerPage = lessonDao.GetRestrictedLessonsByActivityTypeIdPaged(activityTypeId, isActive, itemsOnPage, pg, out totalLessons);
                else
                    listLessonsPerPage = lessonDao.GetRestrictedLessonsPaged(isActive, itemsOnPage, pg, out totalLessons);
            }
            else
            {
                if (activityTypeId.HasValue)
                    listLessonsPerPage = lessonDao.GetLessonsByActivityTypeIdPaged(activityTypeId, itemsOnPage, pg, out totalLessons);
                else
                    listLessonsPerPage = lessonDao.GetLessonsPaged(itemsOnPage, pg, out totalLessons);
            }

            ViewBag.CurrentIsActive = isActive; // Pamatovat si nastavení výběru uplynulých/aktivních lekcí.
            ViewBag.CurrentActivityTypeId = activityTypeId; // Pamatovat si zvolený filtr při procházení stránek.
            ViewBag.Pages = (int)Math.Ceiling((double)totalLessons / (double)itemsOnPage);
            ViewBag.CurrentPage = pg;

            if (Request.IsAjaxRequest())
                return PartialView("Index", listLessonsPerPage);

            return View("Index", listLessonsPerPage);
        }

        /// <summary> Akce pro zobrazení detailních informací o vybrané lekci. </summary>
        /// <param name="id"> Id lekce </param>
        public ActionResult Detail(int id)
        {
            LessonDao lessonDao = new LessonDao();
            Lesson lesson = lessonDao.GetById(id);

            // Získání seznamu registrovaných klientů na vybranou lekci.
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            IList<FitnessCentreUser> listReservedClients = fitnessCentreUserDao.GetReservedClients(id);
            ViewBag.ListReservedClients = listReservedClients;

            return View("Detail", lesson);
        }
	}
}