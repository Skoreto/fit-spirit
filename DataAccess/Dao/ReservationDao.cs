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
    public class ReservationDao : DaoBase<Reservation>
    {
        /*
         * Pro jistotu vytvoříme konstruktor, který dědí z konstruktoru předka, tak abychom měli jistotu, 
         * že proběhne konstrukce podle třídy DaoBase.
         */
        public ReservationDao() : base()
        {
        }

        // TODO Asi přeprogramovat pomocí NHibernate 
        /// <summary> TomSko Metoda pro vrácení seznamu všech rezervací přihlášeného klienta. </summary>
        /// <param name="idClient"> Id klienta </param>
        /// <returns></returns>
        public IList<Reservation> GetClientsReservations(int idClient)
        {
            IList<Reservation> listAllReservations = GetAll();
            IList<Reservation> listClientsReservations = new List<Reservation>();

            foreach (Reservation reservation in listAllReservations)
            {
                if (reservation.Client.Id == idClient)
                {
                    listClientsReservations.Add(reservation);
                }
            }

            return listClientsReservations;
        }

        /// <summary>Metoda stránkování (úprava na datové vrstvě).</summary>
        /// <param name="idClient">Id klienta</param>
        /// <param name="count">kolik rezervací vracím</param>
        /// <param name="page">kterou stránku chci zobrazit</param>
        /// <param name="totalClientsReservations">(out výstupní parametr) kolik rezervací daného klienta(!) celkem existuje</param>
        public IList<Reservation> GetClientsReservationsPaged(int idClient, int count, int page, out int totalClientsReservations)
        {
            totalClientsReservations = session.CreateCriteria<Reservation>().CreateAlias("Client", "cl").Add(Restrictions.Eq("cl.Id", idClient)).SetProjection(Projections.RowCount()).UniqueResult<int>();
            return session.CreateCriteria<Reservation>().CreateAlias("Lesson", "ls").CreateAlias("Client", "cl").Add(Restrictions.Eq("cl.Id", idClient)).AddOrder(Order.Asc("ls.StartTime")).SetFirstResult((page - 1) * count).SetMaxResults(count).List<Reservation>();
        }

        /// <summary>Metoda stránkování (úprava na datové vrstvě).</summary>
        /// <param name="isActive">pouze uplynulé/aktivní rezervace</param>
        /// <param name="idClient">Id klienta</param>
        /// <param name="count">kolik rezervací vracím</param>
        /// <param name="page">kterou stránku chci zobrazit</param>
        /// <param name="totalClientsReservations">(out výstupní parametr) kolik rezervací daného klienta(!) celkem existuje</param>
        public IList<Reservation> GetRestrictedClientsReservationsPaged(bool? isActive, int idClient, int count, int page, out int totalClientsReservations)
        {
            totalClientsReservations = session.CreateCriteria<Reservation>().CreateAlias("Lesson", "ls").Add(Restrictions.Eq("ls.IsActive", isActive)).CreateAlias("Client", "cl").Add(Restrictions.Eq("cl.Id", idClient)).SetProjection(Projections.RowCount()).UniqueResult<int>();
            return session.CreateCriteria<Reservation>().CreateAlias("Lesson", "ls").Add(Restrictions.Eq("ls.IsActive", isActive)).CreateAlias("Client", "cl").Add(Restrictions.Eq("cl.Id", idClient)).AddOrder(Order.Asc("ls.StartTime")).SetFirstResult((page - 1) * count).SetMaxResults(count).List<Reservation>();
        }
    }
}
