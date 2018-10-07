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
    public class MenuController : Controller
    {
        [ChildActionOnly]   // Způsobí, že tato daná akce může být volána pouze zevnitř jinou akcí.
        // GET: /Admin/Menu/
        public ActionResult Index()
        {
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            FitnessCentreUser fitnessCentreUser = fitnessCentreUserDao.GetByLogin(User.Identity.Name);

            return View(fitnessCentreUser);
        }
	}
}