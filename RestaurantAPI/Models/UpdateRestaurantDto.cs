using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models
{
    public class UpdateRestaurantDto
    {
        [Required(ErrorMessage = "Pole wymagane")]
        [MaxLength(25, ErrorMessage = "Maksymalna ilość znaków wynosi 25.")]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool HasDelivery { get; set; }
    }
}
