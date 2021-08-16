using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky_Models
{
    public class Product
    {

        public Product()
        {
            TempSqFt = 1;
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Наименование товара не должно быть пустым")]
        public string Name { get; set; }

        public string ShortDesc { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Цена не должна быть пустой")]
        [Range(1,int.MaxValue,ErrorMessage = "Неверная цена")]
        public double Price { get; set; }

        public string Image { get; set; }

        [Display(Name = "Категория")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [Display(Name = "Тип приложения")]
        public int ApplicationTypeId { get; set; }

        [ForeignKey("ApplicationTypeId")]
        public virtual ApplicationType ApplicationType { get; set; }

        [NotMapped]
        [Range(1,10000)]
        public int TempSqFt { get; set; }

    }
}
