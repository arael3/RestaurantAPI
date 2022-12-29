using RestaurantAPI.Entities;
using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models
{
    public class CreateDishDto
    {
        [Required(ErrorMessage = "Pole wymagane")]
        public string? Name { get; set; }
        
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }
}