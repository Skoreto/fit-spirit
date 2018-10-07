using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Interface;

namespace DataAccess.Model
{
    public class Reservation : IEntity
    {
        public virtual int Id { get; set; }

        public virtual DateTime ReservationTime { get; set; }

        public virtual Lesson Lesson { get; set; }

        public virtual FitnessCentreUser Client { get; set; }

        /// <summary> Pomocná vlastnost na označení rezervací, které již nelze zrušit. </summary>
        public virtual bool IsCancellable { get; set; }
    }
}
