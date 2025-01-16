using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knihy.Models
{
    public class ShoppingCart
    {
        
        public int Id { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public int ProductId { get; set; }
        public Product Product { get; set; }
        [Range(1,1000,ErrorMessage ="Please enter number between 1 and 1000 ")]
        public int Count { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public string ApplicationUserId { get; set; }
        [NotMapped]
        public double Price { get; set; }
    }
}
