using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;
using RestaurantAPI.Exceptions;
using System.Security.Claims;

internal class MinimumCreatedRestaurantsRequirementHandler : AuthorizationHandler<MinimumCreatedRestaurantsRequirement>
{
    private readonly RestaurantDbContext _dbContext;

    public MinimumCreatedRestaurantsRequirementHandler(RestaurantDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumCreatedRestaurantsRequirement requirement)
    {
        // Sprawdzenie czy w zapytaniu został zawarty token
        // Jeżeli NameIdentifier is null oznacza, że w zapytaniu nie przekazano tokenu. Wniosek ten wynika z tego, że każdy token musi posiadać NameIdentifier, skoro NameIdentifier is null oznacza, że w ogólnie nie ma tokenu
        Claim? userIdClaim = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier);
        // Jeżeli nie ma tokenu, zwracamy ForbidException() czyli code 403 - forbidden
        if (userIdClaim is null)
        {
            throw new ForbidException();
        }

        int userId = int.Parse(userIdClaim.Value);

        int amountOfCreatedRestaurants = _dbContext.Restaurants.Count(r => r.CreatedById == userId);

        if (amountOfCreatedRestaurants >= requirement.MinimumCreatedRestaurants)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}