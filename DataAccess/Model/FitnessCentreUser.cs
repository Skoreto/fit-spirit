using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using DataAccess.Interface;

namespace DataAccess.Model
{
    public class FitnessCentreUser : MembershipUser, IEntity
    {
        public virtual int Id { get; set; }

        [Required(ErrorMessage = "Křestní jméno je vyžadováno")]
        [RegularExpression(@"^[a-zá-žA-ZÁ-Ž]*$", ErrorMessage = "Lze použít pouze malá a velká písmena abecedy.")]
        public virtual string FirstName { get; set; }

        [Required(ErrorMessage = "Příjmení je vyžadováno")]
        [RegularExpression(@"^[a-zá-žA-ZÁ-Ž]*$", ErrorMessage = "Lze použít pouze malá a velká písmena abecedy.")]
        public virtual string LastName { get; set; }

        public virtual string Street { get; set; }

        public virtual string City { get; set; }

        public virtual string PostCode { get; set; }

        [Required(ErrorMessage = "Zadání e-mailu je vyžadováno")]
        [RegularExpression(@"[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}", ErrorMessage = "Správný formát jmeno@domena.cz")]
        public virtual string Mail { get; set; }
       
        [RegularExpression(@"^(\+420|\+421) ?[0-9]{3} ?[0-9]{3} ?[0-9]{3}$", ErrorMessage = "Správný formát +420 xxx xxx xxx")]
        public virtual string Telephone { get; set; }

        [Required(ErrorMessage = "Zadání kreditu pro připsání je vyžadováno")]
        [Range(0, 8000, ErrorMessage = "Nemůže být připsán kredit více než 8000 Kč")]
        public virtual int Credit { get; set; }

        /*
         * TinyMce vkládá do stránky HTML kód, což je vyhodnoceno jako potenciální nebezpečí cross site skriptingu.
         * Jedná-li se o funkci v rámci systému či pro administrátora, můžu si dovolit HTML znaky povolit.
         */
        [AllowHtml]
        public virtual string Description { get; set; }

        public virtual string ProfilePhotoName { get; set; }

        public virtual string Login { get; set; }

        [Required(ErrorMessage = "Zadání hesla je vyžadováno")]
        public virtual string Password { get; set; }

        public virtual FitnessCentreRole Role { get; set; }

        public virtual bool IsActive { get; set; }

        // pomocný atribut pro vypsání ID - křest. jm. a příjmení návštěvníka do select listu
        public virtual string NameWithId
        {
            get { return string.Format("{2} {1} (ID {0})", Id, FirstName, LastName); }
        }

        /// <summary> Pomocná vlastnost na označení instruktorů, které již nelze smazat z důvodu vypsaných lekcí. </summary>
        public virtual bool IsDeletable { get; set; }
    }
}
