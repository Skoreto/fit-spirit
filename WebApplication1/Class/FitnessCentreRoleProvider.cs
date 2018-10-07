using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using DataAccess.Dao;
using DataAccess.Model;

namespace WebApplication1.Class
{
    public class FitnessCentreRoleProvider : RoleProvider
    {
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        /*
         * Jestliže user nebude null sáhnu do Role na identifikátor.
         * Mechanismus je obecně připravený, že uživatel může mít více rolí, proto vrací pole stringů.
         */
        public override string[] GetRolesForUser(string username)
        {
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            FitnessCentreUser user = fitnessCentreUserDao.GetByLogin(username);

            if (user == null)
            {
                return new string[] { };
            }

            return new string[] { user.Role.Identificator };
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        /*
         * Pokud roleName se bude shodovat s identifikátorem na Role vrátí true
         */
        public override bool IsUserInRole(string username, string roleName)
        {
            FitnessCentreUserDao fitnessCentreUserDao = new FitnessCentreUserDao();
            FitnessCentreUser user = fitnessCentreUserDao.GetByLogin(username);

            if (user == null)
                return false;

            return user.Role.Identificator == roleName;
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}