using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        // GET: /Admin/Home/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AjaxRequest()
        {
            return View();
        }
	}
}