namespace RestaurantAPI.Models
{
    public class LoginDto
    {
        // W celu praktyki można zdefiniować walidację w Models >> Validators np LoginDtoValidator.cs

        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}