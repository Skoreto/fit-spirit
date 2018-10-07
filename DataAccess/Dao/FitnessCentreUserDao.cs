using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Model;
using NHibernate.Criterion;

namespace DataAccess.Dao
{
    /*
    * Dědí z DaoBase<T>.
    */
    public class FitnessCentreUserDao : DaoBase<FitnessCentreUser>
    {
        /*
         * Na základě kritérií vrátíme daného uživatele.
         * Omezení, aby vlastnost Login byla stejná jako parametr login a zároveň vlastnost Password stejná jako parametr password.
         * Pro pokročílejší práci s podmínkami - objekty Conjunction (platí a zároveň) 
         * a Disjunction (celý výraz platný pokud je platná jedna z podmínek).
         */
        public FitnessCentreUser GetByLoginAndPassword(string login, string password)
        {
            return session.CreateCriteria<FitnessCentreUser>()
                .Add(Restrictions.Eq("Login", login))
                .Add(Restrictions.Eq("Password", password))
                .UniqueResult<FitnessCentreUser>();
        }

        /*
         * Získání instance uživatele pouze na základě jeho uživatelského jména.
         */
        public FitnessCentreUser GetByLogin(string login)
        {
            return session.CreateCriteria<FitnessCentreUser>()
                .Add(Restrictions.Eq("Login", login))
                .UniqueResult<FitnessCentreUser>();
        }

        /// <summary> TomSko Metoda pro vrácení seznamu pouze uživatelů v zadané roli. </summary>
        /// <param name="roleIdentificator"> parametr identifikátor dané role </param>
        /// <returns> Vrací seznam uživatelů v zadané roli. </returns>
        public IList<FitnessCentreUser> GetUsersByRole(string roleIdentificator)
        {
            IList<FitnessCentreUser> listAllUsers = GetAll();
            IList<FitnessCentreUser> listUsersByRole = new List<FitnessCentreUser>();

            foreach (FitnessCentreUser user in listAllUsers)
            {
                if (user.Role.Identificator.Equals(roleIdentificator))
                {
                    listUsersByRole.Add(user);
                }
            }
            return listUsersByRole;
        }

        /// <summary> Metoda pro vrácení seznamu registrovaných klientů na vybranou lekci. </summary>
        /// <param name="id">Id vybrané lekce</param>
        /// <returns>Vrací seznam klientů registrovaných na danou lekci.</returns>
        public IList<FitnessCentreUser> GetReservedClients(int id)
        {
            ReservationDao reservationDao = new ReservationDao();
            IList<Reservation> listAllReservations = reservationDao.GetAll();
            IList<FitnessCentreUser> listReservedClients = new List<FitnessCentreUser>();

            foreach (Reservation reservation in listAllReservations)
            {
                if (reservation.Lesson.Id == id )
                {
                    listReservedClients.Add(reservation.Client);
                }
            }

            return listReservedClients;
        }

        /// <summary> Metoda pro vyhledávání uživatelů, jejichž jméno obsahuje zadanou frázi. </summary>
        /// <param name="phrase"> vyhledávaná fráze </param>
        /// <returns> Vrací kolekci uživatelů, v jejichž jméně se objevila hledaná fráze. </returns>
        public IList<FitnessCentreUser> SearchUsers(string phrase)
        {
            // Procenta jsou pro SQL jako zástupný znak toho, že před a za tou frází může být libovolný počet znaků.
            return session.CreateCriteria<FitnessCentreUser>().Add(Restrictions.Like("LastName", string.Format("%{0}%", phrase))).List<FitnessCentreUser>();
        }

        /// <summary> Metoda pro vyhledávání klientů, jejichž jméno obsahuje zadanou frázi. </summary>
        /// <param name="phrase"> vyhledávaná fráze </param>
        /// <returns> Vrací kolekci klientů, v jejichž jméně se objevila hledaná fráze. </returns>
        public IList<FitnessCentreUser> SearchClients(string phrase)
        {
            return session.CreateCriteria<FitnessCentreUser>().CreateAlias("Role", "rl").Add(Restrictions.Eq("rl.Id", 3)).Add(Restrictions.Like("LastName", string.Format("%{0}%", phrase))).List<FitnessCentreUser>();
        }

        /// <summary>Metoda stránkování seznamu klientů (úprava na datové vrstvě).</summary>
        /// <param name="count">kolik klientů vracím</param>
        /// <param name="page">kterou stránku chci zobrazit</param>
        /// <param name="totalClients">(out výstupní parametr) kolik klientů celkem existuje</param>
        public IList<FitnessCentreUser> GetClientsPaged(int count, int page, out int totalClients)
        {
            totalClients = session.CreateCriteria<FitnessCentreUser>().CreateAlias("Role", "rl").Add(Restrictions.Eq("rl.Id", 3)).SetProjection(Projections.RowCount()).UniqueResult<int>();
            return session.CreateCriteria<FitnessCentreUser>().CreateAlias("Role", "rl").Add(Restrictions.Eq("rl.Id", 3)).SetFirstResult((page - 1) * count).SetMaxResults(count).List<FitnessCentreUser>();
        }

        /// <summary> Metoda pro stránkování vyhledávaných klientů, jejichž jméno obsahuje zadanou frázi. </summary>
        /// <param name="phrase"> vyhledávaná fráze </param>
        /// <param name="count">kolik klientů vracím</param>
        /// <param name="page">kterou stránku chci zobrazit</param>
        /// <param name="totalClientsFound">(out výstupní parametr) kolik klientů celkem existuje</param>
        /// <returns> Vrací kolekci klientů, v jejichž jméně se objevila hledaná fráze na dané stránce. </returns>
        public IList<FitnessCentreUser> SearchClientsPaged(string phrase, int count, int page, out int totalClientsFound)
        {
            totalClientsFound = session.CreateCriteria<FitnessCentreUser>().CreateAlias("Role", "rl").Add(Restrictions.Eq("rl.Id", 3)).Add(Restrictions.Like("LastName", string.Format("%{0}%", phrase))).SetProjection(Projections.RowCount()).UniqueResult<int>();
            return session.CreateCriteria<FitnessCentreUser>().CreateAlias("Role", "rl").Add(Restrictions.Eq("rl.Id", 3)).Add(Restrictions.Like("LastName", string.Format("%{0}%", phrase))).SetFirstResult((page - 1) * count).SetMaxResults(count).List<FitnessCentreUser>();
        }
    }
}