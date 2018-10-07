using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAccess.Dao;
using DataAccess.Model;

namespace WebApplication1.Controllers
{
    public class GalleryController : Controller
    {
        // GET: Gallery
        public ActionResult Index(int? page)
        {
            const int itemsOnPage = 9;
            int pg = page.HasValue ? page.Value : 1;
            int totalPhotographs;

            PhotographyDao photographyDao = new PhotographyDao();
            IList<Photography> listPhotographsPerPage = photographyDao.GetPhotographsPaged(itemsOnPage, pg, out totalPhotographs);

            ViewBag.Pages = (int)Math.Ceiling((double)totalPhotographs / (double)itemsOnPage);
            ViewBag.CurrentPage = pg;

            return View("Index", listPhotographsPerPage);
        }
    }
}