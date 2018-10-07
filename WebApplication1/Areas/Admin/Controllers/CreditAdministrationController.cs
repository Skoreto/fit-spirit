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
    public class CreditAdministrationController : Controller
    {
        // GET: /Admin/CreditAdministration/
        public ActionResult Index(int? clientId)
        {
            int cId = clientId.HasValue ? clientId.Value : -1;   // cId musí být vyplněno

            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();

            if (cId != -1)
            {
                FitnessCentreUser client = fitnessCentreUserDao.GetById(cId);
                return View("IndexChosenClient", client);
            }
            else
            {
                IList<FitnessCentreUser> listClients = fitnessCentreUserDao.GetUsersByRole("client");
                ViewBag.ListClients = listClients;
                return View("IndexAllClients");
            }                     
        }

        /*
         * Metoda pro připsání kreditu klientovi.
         */
        public ActionResult UpdateCredit(int clientId, int addedCredit)
        {
            try
            {
                FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
                FitnessCentreUser editedClient = fitnessCentreUserDao.GetById(clientId);
                editedClient.Credit = editedClient.Credit + addedCredit;
                fitnessCentreUserDao.Update(editedClient);

                TempData["message-success"] = "Klientu " + editedClient.FirstName + " " + editedClient.LastName + " byl úspěšně připsán kredit " + addedCredit + " Kč";
            }
            catch (Exception)
            {
                throw;
            }

            return RedirectToAction("Index", "Clients");
        }
	}
}