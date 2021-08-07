using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Rocky.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }


        [DisplayName("Наименование")]
        [Required(ErrorMessage = "Заполнение наименование категории является обязательным")]
        public string Name { get; set; }


        [DisplayName("Порядок вывода")]
        [Required(ErrorMessage = "Заполнение Порядка вывода является обязательным")]
        [Range(1,int.MaxValue,ErrorMessage = "Порядок отображения категории должен быть больше нуля")]
        public int DisplayOrder { get; set; }        

    }
}
