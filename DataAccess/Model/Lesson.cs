using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using DataAccess.Interface;

namespace DataAccess.Model
{
    public class Lesson : IEntity
    {
        public virtual int Id { get; set; }

        // TODO Problém s přenastavením vnitřní validace DateTime. Přes HTMLHelpery se snaží naparsovat zadané české datum na anglický formát MM.dd.yyyy.
        // TODO Nicméně při zapnutém JavaScriptu DatePicker znemožní zadat jiný než český formát datumu.
        //[Required(ErrorMessage = "Čas zahájení je vyžadován")]
        //[RegularExpression(@"^[0-3]?[0-9]\.[01]?[0-9]\.[0-9]{2,4} [0-2]?[0-9]:[0-5]?[0-9]?$", ErrorMessage = "Správný formát dd.MM.rrrr hh:mm")]
        //[DataType(DataType.DateTime)]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public virtual DateTime StartTime { get; set; }

        //[Required(ErrorMessage = "Čas ukončení je vyžadován")]
        //[RegularExpression(@"^[0-3]?[0-9].[01]?[0-9].[0-9]{2,4} [0-2]?[0-9]:[0-5]?[0-9]?$", ErrorMessage = "Správný formát dd.MM.rrrr hh:mm")]
        public virtual DateTime EndTime { get; set; }

        public virtual ActivityType ActivityType { get; set; }

        public virtual Room Room { get; set; }       

        [Required(ErrorMessage = "Kapacita je vyžadována")]
        [Range(0, 1000, ErrorMessage = "Kapacita nemůže být záporná.")]
        public virtual int OriginalCapacity { get; set; }

        public virtual int ActualCapacity { get; set; }

        public virtual FitnessCentreUser Instructor { get; set; }

        /*
         * TinyMce vkládá do stránky HTML kód, což je vyhodnoceno jako potenciální nebezpečí cross site skriptingu.
         * Jedná-li se o funkci v rámci systému či pro administrátora, můžu si dovolit HTML znaky povolit.
         */
        [AllowHtml]
        public virtual string DescriptionLesson { get; set; }

        public virtual bool IsActive { get; set; }

        /// <summary> Pomocná vlastnost pro zamezení klientovi ve vícenásobné rezervaci tytéž lekce. Není zaznamenána v databázi. </summary>
        public virtual bool IsReserved { get; set; }
    }
}
