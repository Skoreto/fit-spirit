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
    public class ActivityTypeDao : DaoBase<ActivityType>
    {
        /*
         * Pro jistotu vytvoříme konstruktor, který dědí z konstruktoru předka, tak abychom měli jistotu, 
         * že proběhne konstrukce podle třídy DaoBase.
         */
        public ActivityTypeDao() : base()
        {
        }

        /// <summary>Metoda stránkování (úprava na datové vrstvě).</summary>
        /// <param name="count">kolik aktivit vracím</param>
        /// <param name="page">kterou stránku chci zobrazit</param>
        /// <param name="totalActivityTypes">(out výstupní parametr) kolik aktivit celkem existuje</param>
        public IList<ActivityType> GetActivityTypesPaged(int count, int page, out int totalActivityTypes)
        {
            totalActivityTypes = session.CreateCriteria<ActivityType>().SetProjection(Projections.RowCount()).UniqueResult<int>();
            return session.CreateCriteria<ActivityType>().SetFirstResult((page - 1) * count).SetMaxResults(count).List<ActivityType>();
        }
    }
}