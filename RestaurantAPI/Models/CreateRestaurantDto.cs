using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models
{
    public class CreateRestaurantDto
    {
        [Required(ErrorMessage = "Pole wymagane")]
        [MaxLength(25, ErrorMessage = "Maksymalna ilość znaków wynosi 25.")]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool HasDelivery { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactNumber { get; set; }

        [Required(ErrorMessage = "Pole wymagane")]
        [MaxLength(50)]
        public string? City { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "Maksymalna ilość znaków wynosi 50.")]
        public string? Street { get; set; }
        public string? PostalCode { get; set; }
    }
}
