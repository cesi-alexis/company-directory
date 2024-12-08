using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Worker
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The FirstName field is required.")]
        [MaxLength(50, ErrorMessage = "The FirstName cannot exceed 50 characters.")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "The LastName field is required.")]
        [MaxLength(50, ErrorMessage = "The LastName cannot exceed 50 characters.")]
        public string LastName { get; set; } = null!;

        [Required]
        [Phone]
        [MaxLength(15)]
        public string PhoneFixed { get; set; } = null!;

        [Required]
        [Phone]
        [MaxLength(15)]
        public string PhoneMobile { get; set; } = null!;

        [Required]
        [EmailAddress(ErrorMessage = "The Email field is not a valid email address.")]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public Service? Service { get; set; } = null!;

        [Required]
        public int LocationId { get; set; }

        [ForeignKey("LocationId")]
        public Location? Location { get; set; } = null!;
    }
}