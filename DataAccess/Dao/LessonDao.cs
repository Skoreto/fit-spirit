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
    public class LessonDao : DaoBase<Lesson>
    {
        /*
         * Pro jistotu vytvoříme konstruktor, který dědí z konstruktoru předka, tak abychom měli jistotu, 
         * že proběhne konstrukce podle třídy DaoBase.
         */
        public LessonDao() : base()
        {
        }

        /// <summary>Metoda na vrácení seznamu aktivních/uplynulých lekcí.</summary>
        /// <param name="isActive">uplynulé lekce nebo aktivní</param>
        public IList<Lesson> GetRestrictedLessons(bool isActive)
        {         
            return session.CreateCriteria<Lesson>().Add(Restrictions.Eq("IsActive", isActive)).AddOrder(Order.Asc("StartTime")).List<Lesson>();
        }

        /// <summary>Metoda stránkování (úprava na datové vrstvě) vrátí veškeré lekce v databázi.</summary>
        /// <param name="count">kolik lekcí vracím</param>
        /// <param name="page">kterou stránku chci zobrazit</param>
        /// <param name="totalLessons">(out výstupní parametr) kolik lekcí celkem existuje</param>
        /*
         * Výstupní parametr - vždy musí být přiřazen v těle metody.
         * SetFirstResult - metoda to přeloží do SQL dotazu a do něj zakomponuje, který výsledek má být braný jako první.
         * První výsledek spočítáme jako "(page - 1) * count". Chci zobrazit 3. stránku (page=3) a 20 lekcí na stránce (count=20), první výsledek bude mít index (3-1)*20=40 tj. lekce 40-59.
         */
        public IList<Lesson> GetLessonsPaged(int count, int page, out int totalLessons)
        {
            totalLessons = session.CreateCriteria<Lesson>().SetProjection(Projections.RowCount()).UniqueResult<int>();
            return session.CreateCriteria<Lesson>().AddOrder(Order.Asc("StartTime")).SetFirstResult((page - 1) * count).SetMaxResults(count).List<Lesson>();
        }

        /// <summary>Metoda stránkování (úprava na datové vrstvě).</summary>
        /// <param name="isActive">uplynulé lekce nebo aktivní</param>
        /// <param name="count">kolik lekcí vracím</param>
        /// <param name="page">kterou stránku chci zobrazit</param>
        /// <param name="totalLessons">(out výstupní parametr) kolik lekcí celkem existuje</param>
        public IList<Lesson> GetRestrictedLessonsPaged(bool? isActive, int count, int page, out int totalLessons)
        {
            totalLessons = session.CreateCriteria<Lesson>().Add(Restrictions.Eq("IsActive", isActive)).SetProjection(Projections.RowCount()).UniqueResult<int>();
            return session.CreateCriteria<Lesson>().Add(Restrictions.Eq("IsActive", isActive)).AddOrder(Order.Asc("StartTime")).SetFirstResult((page - 1) * count).SetMaxResults(count).List<Lesson>();
        }    

        /// <summary> Metoda pro vrácení seznamu lekcí dané aktivity </summary>
        /// <param name="id"> Id aktivity hledaných lekcí </param>
        /// <returns> Seznam lekcí dané aktivity. </returns>
        public IList<Lesson> GetLessonsByActivityTypeId(int id)
        {
            // Abych se mohl dostávat do objektů do hloubky (na Id aktivity dané lekce) vytvořím Alias. 
            // Díky přejmenování ActivityType na "at" můžu použít funkci restrikcí. at.Id se musí rovnat příchozímu parametru id.
            return session.CreateCriteria<Lesson>().CreateAlias("ActivityType", "at").Add(Restrictions.Eq("at.Id", id)).List<Lesson>();
        }

        /// <summary>Metoda stránkování (úprava na datové vrstvě) pro lekce filtrované dle vybrané aktivity.</summary>
        /// <param name="activityTypeId">Id aktivity, podle které filtruji</param>
        /// <param name="count">kolik lekcí vracím</param>
        /// <param name="page">kterou stránku chci zobrazit</param>
        /// <param name="totalLessonsByActivityType">(out výstupní parametr) kolik lekcí celkem existuje</param>
        public IList<Lesson> GetLessonsByActivityTypeIdPaged(int? activityTypeId, int count, int page, out int totalLessonsByActivityType)
        {
            totalLessonsByActivityType = session.CreateCriteria<Lesson>().CreateAlias("ActivityType", "at").Add(Restrictions.Eq("at.Id", activityTypeId)).SetProjection(Projections.RowCount()).UniqueResult<int>();
            return session.CreateCriteria<Lesson>().CreateAlias("ActivityType", "at").Add(Restrictions.Eq("at.Id", activityTypeId)).AddOrder(Order.Asc("StartTime")).SetFirstResult((page - 1) * count).SetMaxResults(count).List<Lesson>();
        }

        /// <summary>Metoda stránkování (úprava na datové vrstvě) pro lekce filtrované dle vybrané aktivity a aktivace lekce</summary>
        /// <param name="activityTypeId">Id aktivity, podle které filtruji</param>
        /// <param name="isActive">uplynulé lekce nebo aktivní</param>
        /// <param name="count">kolik lekcí vracím</param>
        /// <param name="page">kterou stránku chci zobrazit</param>
        /// <param name="totalLessonsByActivityType">(out výstupní parametr) kolik lekcí celkem existuje</param>
        public IList<Lesson> GetRestrictedLessonsByActivityTypeIdPaged(int? activityTypeId, bool? isActive, int count, int page, out int totalLessonsByActivityType)
        {
            totalLessonsByActivityType = session.CreateCriteria<Lesson>().CreateAlias("ActivityType", "at").Add(Restrictions.Eq("IsActive", isActive)).Add(Restrictions.Eq("at.Id", activityTypeId)).SetProjection(Projections.RowCount()).UniqueResult<int>();
            return session.CreateCriteria<Lesson>().CreateAlias("ActivityType", "at").Add(Restrictions.Eq("IsActive", isActive)).Add(Restrictions.Eq("at.Id", activityTypeId)).AddOrder(Order.Asc("StartTime")).SetFirstResult((page - 1) * count).SetMaxResults(count).List<Lesson>();
        }

        /// <summary> Metoda pro nastavení uskutečněných lekcí jako neaktivní </summary>
        public void SetExpiredLessons()
        {
            IList<Lesson> listLessons = GetAll();

            foreach (Lesson lesson in listLessons)
            {
                if (lesson.StartTime < DateTime.Now)
                {
                    lesson.IsActive = false;
                    Update(lesson);
                }
            }
        }

    }
}
