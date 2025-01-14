using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastPMS.Models.Domain
{
    public class Developer
    {
        [Key]
        public int DeveloperID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name="E-mail")]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; }

        [Phone]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [StringLength(50)]
        public string Department { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name="Data of joining")]
        public DateTime DateOfJoining { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Display(Name ="Profile Photo")] 
        public byte[]? image { get; set; }

        [Display(Name ="Upload Photo")]
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}
