using System.ComponentModel.DataAnnotations.Schema;
using VPProject.Models;

namespace VPProject.Models
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }

        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }

        public Order Order { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public Product Product { get; set; }
    }
}
