using Microsoft.AspNetCore.Authorization;
using RestaurantAPI.Exceptions;
using System.Security.Claims;

namespace RestaurantAPI.Authorization
{
    public class MinimumAgeRequirementHandler : AuthorizationHandler<MinimumAgeRequirement>
    {
        private readonly ILogger<MinimumAgeRequirementHandler> _logger;

        public MinimumAgeRequirementHandler(ILogger<MinimumAgeRequirementHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
        {
            if (context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier) is null)
                throw new ForbidException();

            var dateOfBith = DateTime.Parse(context.User.FindFirst(c => c.Type == "DateOfBirth").Value);

            var userId = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value;

            _logger.LogInformation($"User: {userId} with date of birth: {dateOfBith}");

            if(dateOfBith.AddYears(requirement.MinimumAge) <= DateTime.Today)
            {
                _logger.LogInformation("Authorization succeded");
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogInformation("Authorization failed");
            }
            return Task.CompletedTask;
        }
    }
}
