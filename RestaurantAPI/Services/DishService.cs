using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;
using RestaurantAPI.Exceptions;
using RestaurantAPI.Models;
using RestaurantAPI.Services;

namespace RestaurantAPI.Services
{
    public interface IDishService
    {
        int Create(int restaurantId, CreateDishDto dto);
        DishDto GetById(int restaurantId, int dishId);
        List<DishDto> GetAll(int restaurantId);
        void Delete(int restaurantId, int dishId);
    }

    public class DishService : IDishService
    {
        private readonly RestaurantDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<RestaurantService> _logger;

        public DishService(RestaurantDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public DishDto GetById(int restaurantId, int dishId)
        {
            var restaurant = GetRestaurant(restaurantId);

            var dish = _dbContext
                            .Dishes
                            .FirstOrDefault(d => d.Id == dishId);

            if (dish is null || dish.RestaurantId != restaurantId)
                throw new NotFoundException("Dish not found");

            var dishDto = _mapper.Map<DishDto>(dish);

            return dishDto;
        }

        public List<DishDto> GetAll(int restaurantId)
        {
            var restaurant = GetRestaurant(restaurantId);

            var dishes = restaurant.Dishes;

            var dishesDtos = _mapper.Map<List<DishDto>>(dishes);

            return dishesDtos;
        }

        public int Create(int restaurantId, CreateDishDto dto)
        {
            GetRestaurant(restaurantId);

            var dish = _mapper.Map<Dish>(dto);
            dish.RestaurantId = restaurantId;

            _dbContext.Dishes.Add(dish);
            _dbContext.SaveChanges();

            return dish.Id;
        }

        public void Delete(int restaurantId, int dishId)
        {
            var restaurant = GetRestaurant(restaurantId);

            var dish = restaurant.Dishes.FirstOrDefault(d => d.Id == dishId);

            if (dish == null)
                throw new NotFoundException("Dish not found");

            _dbContext.Dishes.Remove(dish);
            _dbContext.SaveChanges();
        }

        private Restaurant GetRestaurant(int restaurantId)
        {
            var restaurant = _dbContext
                .Restaurants
                .Include(r => r.Dishes)
                .FirstOrDefault(r => r.Id == restaurantId);

            if (restaurant == null)
                throw new NotFoundException("Restaurant not found");

            return restaurant;
        }
    }
}
