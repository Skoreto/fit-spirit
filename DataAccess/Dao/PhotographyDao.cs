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
    public class PhotographyDao : DaoBase<Photography>
    {
        public PhotographyDao() : base()
        {
        }

        /// <summary>Metoda stránkování (úprava na datové vrstvě).</summary>
        /// <param name="count">kolik fotografií vracím</param>
        /// <param name="page">kterou stránku chci zobrazit</param>
        /// <param name="totalPhotographs">(out výstupní parametr) kolik fotografií celkem existuje</param>
        public IList<Photography> GetPhotographsPaged(int count, int page, out int totalPhotographs)
        {
            totalPhotographs = session.CreateCriteria<Photography>().SetProjection(Projections.RowCount()).UniqueResult<int>();
            return session.CreateCriteria<Photography>().SetFirstResult((page - 1) * count).SetMaxResults(count).List<Photography>();
        }
    }
}
