using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Interface;

namespace DataAccess.Model
{
    public class FitnessCentreRole : IEntity
    {
        public virtual int Id { get; set; }

        // Popisek role pro zobrazení v aplikaci
        public virtual string RoleDescription { get; set; }

        public virtual string Identificator { get; set; }
    }
}
