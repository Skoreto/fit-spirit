using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Interface;
using NHibernate;
using NHibernate.Criterion;

namespace DataAccess.Dao
{
    /*
    * Prapředek všech DAO tříd. Třída definuje základní operace.
    * Musíme mu říct, že typ T je třída, implementiuje IEntity a zároveň tříde Daobase<T>, že má implementovat rozhraní IDaoBase<T>
    */
    public class DaoBase<T> : IDaoBase<T> where T : class, IEntity
    {
        // Nejprve se potřebujeme dostat k objektu Session z NHibernateHelperu. Ze třídy budeme dědit, proto protected.
        protected ISession session;

        /*
         * Inicializuju sessionu v konstruktoru.
         */
        protected DaoBase()
        {
            session = NHibernateHelper.Session;
        }

        public IList<T> GetAll()
        {
            return session.QueryOver<T>().List<T>();
        }

        public object Create(T entity)
        {
            object o;

            // vypomůžeme si vytvoření objektu transakce. Je tu, aby bylo možné vykonat několik perzistetním operací za sebou aniž by byla narušena intergita datového uložiště.
            // A máme zajištěno, že session se vyprázdní a nic nezůstane v cachi.
            using (ITransaction transaction = session.BeginTransaction())
            {
                o = session.Save(entity);    // metoda Save vrací object
                transaction.Commit();
            }

            return o;
        }

        public void Update(T entity)
        {
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Update(entity);
                transaction.Commit();
            }
        }

        public void Delete(T entity)
        {
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Delete(entity);
                transaction.Commit();
            }
        }

        public T GetById(int id)
        {
            // CreateCriteria vytvoří objekt, do kterého se dají vkládat kritéria. Filtrují se podmínky, podle kterých se objekty vybírají.
            // Vlastnost Id se musí rovnat hodnotě id v parametru.
            return session.CreateCriteria<T>().Add(Restrictions.Eq("Id", id)).UniqueResult<T>();
        }
    }  
}
