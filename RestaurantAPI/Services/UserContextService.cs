using System.Security.Claims;

namespace RestaurantAPI.Services
{
    public interface IUserContextService
    {
        ClaimsPrincipal User { get; }
        int? GetUserId { get; }
    }

    // Klasa UserContextService jest odpowiedzialna za udostępnianie informacji o danym użytkowniku na podstawie kontekstu Http
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // IHttpContextAccessor umożliwia dostęp do kontekstu Http nawet poza kontrolerem
        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User;
        public int? GetUserId => User is null ? null : (int?)int.Parse(User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value);
    }
}
