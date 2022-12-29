using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;
using RestaurantAPI.Exceptions;
using RestaurantAPI.Models;
using RestaurantAPI.Services;

namespace RestaurantAPI.Controllers
{
    [Route("api/restaurant/{restaurantId}/dish")]
    [ApiController]
    public class DishController : ControllerBase
    {
        private readonly IDishService _dishService;

        public DishController(IDishService dishService)
        {
            _dishService = dishService;
        }

        [HttpPost]
        public ActionResult CreateDish([FromRoute] int restaurantId, [FromBody] CreateDishDto dto)
        {
            int dishId = _dishService.Create(restaurantId, dto);
            return Created($"/api/restaurant/{restaurantId}/dish/{dishId}", null);
        }

        [HttpGet("{dishId}")]
        public ActionResult<DishDto> Get([FromRoute] int restaurantId, [FromRoute] int dishId)
        {
            var dishDto = _dishService.GetById(restaurantId, dishId);
            return Ok(dishDto);
        }

        [HttpGet]
        public ActionResult<List<DishDto>> Get([FromRoute] int restaurantId)
        {
            return Ok(_dishService.GetAll(restaurantId));
        }

        [HttpDelete("{dishId}")]
        public ActionResult DeleteDish([FromRoute] int restaurantId, [FromRoute] int dishId)
        {
            _dishService.Delete(restaurantId, dishId);
            return NoContent();
        }

    }
}
