using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DataAccess.Dao;
using DataAccess.Model;

namespace WebApplication1.Areas.Admin.Controllers
{
    public class LoginController : Controller
    {
        // GET: /Admin/Login/
        public ActionResult Index()
        {
            return View();
        }

        // Přidáme atribut HttpPost, protože víme, že je to akce, která by se měla volat jedině postem
        [HttpPost]
        public ActionResult SignIn(string login, string password)
        {
            if (Membership.ValidateUser(login, password))
            {
                FitnessCentreUser user = new FitnessCentreUserDao().GetByLogin(login);
                if (user.IsActive)
                {
                    // vytvoří se uživatelská cookie, která je vrácena klientovi a díky ní se bude uživatel autentifikovat při každém dalším dotazu
                    FormsAuthentication.SetAuthCookie(login, false);
                    return RedirectToAction("Index", "Home"); 
                }
                else
                {
                    TempData["login-error"] = "Váš účet není aktivní. Prosím, vyčkejte na aktivaci obsluhou fitness centra.";
                    return RedirectToAction("Index", "Login");
                }

            }

            TempData["login-error"] = "Login nebo heslo není správné";
            return RedirectToAction("Index", "Login");  // TomSko
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();    // Zavoláme Session.Clear() aby náhodou v session nezůstaly přebytečný data

            return RedirectToAction("Index", "Home", new {area = ""});
        }
	}
}