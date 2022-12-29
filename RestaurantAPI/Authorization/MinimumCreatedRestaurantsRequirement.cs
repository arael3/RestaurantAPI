using Microsoft.AspNetCore.Authorization;

internal class MinimumCreatedRestaurantsRequirement : IAuthorizationRequirement
{
    public int MinimumCreatedRestaurants { get; }

    public MinimumCreatedRestaurantsRequirement(int minimumCreatedRestaurants)
    {
        MinimumCreatedRestaurants = minimumCreatedRestaurants;
    }
}