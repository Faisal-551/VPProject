using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using VPProject.Models;

namespace VPProject.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public DateTime OrderDate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        public Customer Customer { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
