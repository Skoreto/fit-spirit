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
    public class ActivityType : IEntity
    {
        public virtual int Id { get; set; }

        [Required(ErrorMessage = "Zadání názvu aktivity je vyžadováno")]
        public virtual string Name { get; set; }

        [Required(ErrorMessage = "Zadání ceny lekce je vyžadováno")]
        [Range(0, 1500, ErrorMessage = "Cena jedné lekce nemůže být vyšší než 1500 Kč")]
        public virtual int Price { get; set; }

        [AllowHtml]
        public virtual string ShortDescription { get; set; }

        [AllowHtml]
        public virtual string Description { get; set; }

        public virtual string IllustrationImageName { get; set; }

        public virtual string IllustrationThumbImageName { get; set; }
    }
}