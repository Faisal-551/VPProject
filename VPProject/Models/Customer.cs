using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VPProject.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required]
        public string Phone { get; set; }

        public string Address { get; set; }

        public ICollection<Order>? Orders { get; set; }
    }
}
