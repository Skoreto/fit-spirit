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
    public class RoomDao : DaoBase<Room>
    {
        /*
         * Pro jistotu vytvoříme konstruktor, který dědí z konstruktoru předka, tak abychom měli jistotu, 
         * že proběhne konstrukce podle třídy DaoBase.
         */
        public RoomDao() : base()
        {
        }

        /// <summary>Metoda stránkování (úprava na datové vrstvě).</summary>
        /// <param name="count">kolik místnostní vracím</param>
        /// <param name="page">kterou stránku chci zobrazit</param>
        /// <param name="totalRooms">(out výstupní parametr) kolik místností celkem existuje</param>
        public IList<Room> GetRoomsPaged(int count, int page, out int totalRooms)
        {
            totalRooms = session.CreateCriteria<Room>().SetProjection(Projections.RowCount()).UniqueResult<int>();
            return session.CreateCriteria<Room>().SetFirstResult((page - 1) * count).SetMaxResults(count).List<Room>();
        }
    }
}
