using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    /* 
     * Interface, od kterého budou jednotlivé DAO třídy dědit.
     */
    public interface IEntity
    {
        int Id { get; set; }
    }
}
