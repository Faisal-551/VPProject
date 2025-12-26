using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VPProject.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ImageUrl { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public Category Category { get; set; }
    }
}