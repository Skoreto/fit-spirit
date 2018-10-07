using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DataAccess.Interface;

namespace DataAccess.Model
{
    public class Room : IEntity
    {
        public virtual int Id { get; set; }

        [Required(ErrorMessage = "Zadání názvu místnosti je vyžadováno")]
        public virtual string Name { get; set; }

        [AllowHtml]
        public virtual string ShortDescription { get; set; }

        public virtual string IllustrationImageName { get; set; }

        public virtual string IllustrationThumbImageName { get; set; }
    }
}
