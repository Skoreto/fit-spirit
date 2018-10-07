using DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAccess.Dao;
using WebApplication1.Class;

namespace WebApplication1.Controllers
{
    public class InstructorsController : Controller
    {
        // GET: /Instructors/
        public ActionResult Index()
        {
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            IList<FitnessCentreUser> listInstructors = fitnessCentreUserDao.GetUsersByRole("instructor");

            return View("Index", listInstructors);
        }

        public ActionResult Detail(int id)
        {
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            FitnessCentreUser instructor = fitnessCentreUserDao.GetById(id);

            return View("Detail", instructor);
        }
	}
}