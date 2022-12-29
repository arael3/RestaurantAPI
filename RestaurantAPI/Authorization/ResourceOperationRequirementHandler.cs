using Microsoft.AspNetCore.Authorization;
using RestaurantAPI.Entities;
using System.Security.Claims;

namespace RestaurantAPI.Authorization
{
    public class ResourceOperationRequirementHandler : AuthorizationHandler<ResourceOperationRequirement, Restaurant>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourceOperationRequirement requirement, Restaurant restaurant)
        {
            // Zezwalaj na wykonywanie operacji typu Read oraz Create
            if (requirement.ResourceOperation == ResourceOperation.Read || requirement.ResourceOperation == ResourceOperation.Create)
            {
                context.Succeed(requirement);
            }

            // Zezwalaj Adminom na wykonywanie pozostałych operacji, czyli Update, Delete
            var userRole = context.User.FindFirst(c => c.Type == ClaimTypes.Role).Value;

            if (userRole == "Admin")
            {
                context.Succeed(requirement);
            }

            // Zezwalaj pozostałym użytkownikom (np. Manager) na wykonywanie pozostałych operacji (czyli Update, Delete) tylko jeśli utworzyli obiekt (w tym przypadku restaurację), na którym ma zostać wykonana akcja
            var userID = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (int.Parse(userID) == restaurant.CreatedById)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
