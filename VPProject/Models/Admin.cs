using System.ComponentModel.DataAnnotations;

namespace VPProject.Models
{
    public class Admin
    {
        public int AdminId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Role { get; set; } = "Manager";
    }
}