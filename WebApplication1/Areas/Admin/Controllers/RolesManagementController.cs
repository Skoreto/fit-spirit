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
    public class RolesManagementController : Controller
    {
        // GET: /Admin/RolesManagement/
        [Authorize(Roles = "staff")]
        public ActionResult Index()
        {
            FitnessCentreRoleDao fitnessCentreRoleDao = new FitnessCentreRoleDao();
            IList<FitnessCentreRole> listRoles = fitnessCentreRoleDao.GetAll();

            return View(listRoles);
        }

        [Authorize(Roles = "staff")]
        public ActionResult Create()
        {
            return View();
        }

        /*
         * Svazování formuláře s modelem.
         * Nastavení ID druhu aktivity dle pořadí v seznamu druhů aktivit.
         * Přidání nového druhu aktivity do seznamu.
         * Zjištění, jestli je model validní. Controller má na sobě objekt ModelState a ten má na sobě vlastnost isValid.
         * Pokud není validní, uživatel dostane předvyplněný formulář zpátky a je varován.
         * Nenavracím pohled akce Add, ale cizí pohled Create, protože ho uživateli znovu vrátím i s daty, které už vyplnil "activityType".
         */
        public ActionResult Add(FitnessCentreRole role)
        {
            if (ModelState.IsValid)
            {
                FitnessCentreRoleDao fitnessCentreRoleDao = new FitnessCentreRoleDao();
                fitnessCentreRoleDao.Create(role);

                TempData["message-success"] = "Role " + role.RoleDescription + " byla úspěšně přidána";
            }
            else
            {
                TempData["message-error"] = "Role nebyla přidána";
                return View("Create", role);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            FitnessCentreRoleDao fitnessCentreRoleDao = new FitnessCentreRoleDao();
            FitnessCentreRole role = fitnessCentreRoleDao.GetById(id);

            return View(role);
        }

        public ActionResult Update(FitnessCentreRole role)
        {
            try
            {
                FitnessCentreRoleDao fitnessCentreRoleDao = new FitnessCentreRoleDao();
                fitnessCentreRoleDao.Update(role);

                TempData["message-success"] = "Role " + role.RoleDescription + " byla úspěšně upravena.";
            }
            catch (Exception)
            {

                throw;
            }

            return RedirectToAction("Index", "RolesManagement");
        }

        public ActionResult Delete(int id)
        {
            try
            {
                FitnessCentreRoleDao fitnessCentreRoleDao = new FitnessCentreRoleDao();
                FitnessCentreRole role = fitnessCentreRoleDao.GetById(id);
                fitnessCentreRoleDao.Delete(role);

                TempData["message-success"] = "Role " + role.RoleDescription + " byla úspěšně smazána.";
            }
            catch (Exception)
            {
                // mechanismus zachytávání výjimek doporučuje dobře si nastudovat
                throw;
            }

            return RedirectToAction("Index");
        }
	}
}