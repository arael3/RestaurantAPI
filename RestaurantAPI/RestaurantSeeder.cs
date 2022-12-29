using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;

namespace RestaurantAPI
{
    public class RestaurantSeeder
    {
        private readonly RestaurantDbContext _dbContext;

        public RestaurantSeeder(RestaurantDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Seed()
        {
            if (_dbContext.Database.CanConnect())
            {
                var pendingMigrations = _dbContext.Database.GetPendingMigrations(); // Sprawdzenie czy istnieją jakieś niezaaplikowane migracji na aktualnie podłaczonej bazie danych

                if (pendingMigrations != null && pendingMigrations.Any())
                {
                    _dbContext.Database.Migrate(); // Wykonanie niezaaplikowanych migracji na aktualnie podłaczonej bazie danych
                }

                if (!_dbContext.Roles.Any())
                {
                    var roles = GetRoles();
                    _dbContext.Roles.AddRange(roles);
                    _dbContext.SaveChanges();
                }

                if (!_dbContext.Restaurants.Any())
                {
                    var restaurants = GetRestaurants();
                    _dbContext.Restaurants.AddRange(restaurants);
                    _dbContext.SaveChanges();
                }
            }
        }

        private IEnumerable<Role> GetRoles()
        {
            var roles = new List<Role>()
            {
                new Role()
                {
                    Name = "User"
                },
                new Role()
                {
                    Name = "Manager"
                },
                new Role()
                {
                    Name = "Admin"
                }
            };

            return roles;
        }

        private IEnumerable<Restaurant> GetRestaurants()
        {
            var restaurants = new List<Restaurant>()
            {
                new Restaurant()
                {
                    Name = "KFC",
                    Category = "FastFood",
                    Description = "Description of restaurant...",
                    ContactEmail = "contact@kfc.com",
                    HasDelivery = true,
                    Dishes = new List<Dish> 
                    { 
                        new Dish()
                        {
                            Name = "Nashville Hot Chicken",
                            Price = 10.30M
                        },
                        new Dish()
                        {
                            Name = "Chicken Nuggets",
                            Price = 5.30M
                        }
                    },
                    Address = new Address()
                    {
                        City = "Kraków",
                        Street = "Długa 5",
                        PostalCode = "30-001"
                    }
                },
                new Restaurant()
                {
                    Name = "McDonald",
                    Category = "FastFood",
                    Description = "Description of restaurant...",
                    ContactEmail = "contact@mac.com",
                    HasDelivery = true,
                    Dishes = new List<Dish>
                    {
                        new Dish()
                        {
                            Name = "Cheeseburger",
                            Price = 6.30M
                        },
                        new Dish()
                        {
                            Name = "Hamburger",
                            Price = 7.30M
                        }
                    },
                    Address = new Address()
                    {
                        City = "Warszawa",
                        Street = "Potockiego 15",
                        PostalCode = "01-002"
                    }
                }
            };

            return restaurants;
        }
    }
}
