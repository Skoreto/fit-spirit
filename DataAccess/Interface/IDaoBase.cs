using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    /*
     * Interface pro DaoBase, kvůli rozšiřitelnosti.
     */
    interface IDaoBase<T>
    {
        // Podle toho, jaká třída tam přijde, tak se bude vracet typ T. Pokud bude třída odvozena z DaoBase<Book>, tak to bude IList<Book>
        IList<T> GetAll();

        object Create(T entity);

        void Update(T entity);

        void Delete(T entity);

        T GetById(int id);        // bude navracet T
    }
}
