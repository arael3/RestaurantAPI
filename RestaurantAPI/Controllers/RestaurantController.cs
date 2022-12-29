using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;
using RestaurantAPI.Models;
using RestaurantAPI.Services;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Xml.Linq;

namespace RestaurantAPI.Controllers
{

    [Route("api/restaurant")]  // atrybut odpowiadający za ściezkę dla akcji zdefiniowanych w tym kontrolerze
    [ApiController]  // ten atrybut sprawia, że jeśli do naszego API przyjdzie jakiekolwiek zapytanie, dla którego istnieje walidacja modelu (działa metoda ModelState.IsValid), to poniższy kawałek kodu wywoływany jest automatycznie
    //if (!ModelState.IsValid)
    //    return BadRequest(ModelState);
    // innymi słowy atrybut ApiController zapewnia automatyczną walidację i zwracania odpowiedniego komunikatu (np. BadRequest())
    // automatyczna walidacja bierze także pod uwagę customowe walidatory (np. Validators\RestaurantQueryValidator), które dziedziczą po klasie AbstractValidator i są zarejstrowane w Program.cs np. builder.Services.AddScoped<IValidator<RestaurantQuery>, RestaurantQueryValidator>();
    [Authorize]  // Oznacza, że wszystkie akcje wykonywane w tym kontrolerze muszą być autoryzowane
    public class RestaurantController : ControllerBase
    {
        private readonly IRestaurantService _restaurantService;

        public RestaurantController(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        [HttpPut("{restaurantId}")]
        [Authorize(Roles = "Admin, Manager")]
        public ActionResult<IEnumerable<RestaurantDto>> UpdateRestaurant([FromRoute] int restaurantId, [FromBody] UpdateRestaurantDto dto) 
        {
            // Poniższy kod jest niepotrzebny, ponieważ jego funkcjonalność zapewnia atrybut [ApiController] 
            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);

            // Sposób na przeprowadzenie autoryzacji z wykorzystaniem ClaimsPrincipal.User
            // Poniższy opis może być niewłaściwy, po prostu tak to rozumiem na dany moment 
            // Parametr User ustala swoją wartość poprzez HttpContext (można także zapisać HttpContext.User)
            // Natomiast HttpContext ustala swoje wartości na podstawie tokenu tzn.
            // 1. Użytkownik loguje się
            // 2. Generowany jest token
            // 3. Token przekazywany jest razem z przesyłanym żądaniem (np. UpdateRestaurant)
            // 4. Serwer otrzymuje token i przy pomocy klucza prywatnego odszyfrowuje z niego dane, m.in. claims
            // 5. Jednym z claims jest "NameIdentifier" czyli Id zalogowanego użytkownika
            // 6. Posiadając Id zalogowanego użytkownika, HttpContext jest w stanie ustalić ClaimsPrincipal i wykorzystać je w danym algorytmie autoryzacji
            //_restaurantService.Update(restaurantId, dto, User);


            // Inny sposób (który jest tu wykorzystywany) opiera się o własną (customową) klasę UserContextService, która jest odpowiedzialna za udostępnianie informacji o danym użytkowniku na podstawie kontekstu Http. Stworzona klasa korzysta z IHttpContextAccessor, który umożliwia dostęp do kontekstu Http nawet poza kontrolerem. User z użyciem klasy UserContextService ustalany jest w klasie RestaurantService.
            _restaurantService.Update(restaurantId, dto);

            return Ok();
        }

        [HttpDelete("{restaurantId}")]
        [Authorize(Roles = "Admin, Manager")]
        public ActionResult DeleteRestaurant([FromRoute] int restaurantId)
        {
            _restaurantService.Delete(restaurantId);
            return NoContent();
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Manager")]  // autoryzacja w oparciu o role użytkownika. Mechanizm autoryzacji jest w stanie dokonać autoryzacji w oparciu o rolę użytkownika, ponieważ jest ona przekazywana w tokenie. Token zawiera tę informację, ponieważ została ona zdefiniowana w jego żądaniach (claims)
        public ActionResult CreateRestaurant([FromBody] CreateRestaurantDto dto)
        {
            // Poniżej inny sposób na autoryzację w oparciu o rolę użytkownika
            //if (!HttpContext.User.IsInRole("Admin") || !HttpContext.User.IsInRole("Manager"))
            //{
            //    return Forbid();
            //}

            // Poniższy kod jest niepotrzebny, ponieważ jego funkcjonalność zapewnia atrybut [ApiController] 
            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);

            // Sposób na przeprowadzenie autoryzacji z wykorzystaniem ClaimsPrincipal.User
            // Inny sposób (który jest tu wykorzystywany) opiera się o własną (customową) klasę UserContextService, która jest odpowiedzialna za udostępnianie informacji o danym użytkowniku na podstawie kontekstu Http. Stworzona klasa korzysta z IHttpContextAccessor, który umożliwia dostęp do kontekstu Http nawet poza kontrolerem. UserId z użyciem klasy UserContextService ustalany jest w klasie RestaurantService.
            //var userId = int.Parse(User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value);

            int restaurantId = _restaurantService.Create(dto);

            return Created($"/api/restaurant/{restaurantId}", null);
        }

        // ####################  GETALL  ####################
        [HttpGet]
        [AllowAnonymous]
        //[Authorize(Roles = "Admin, Manager, User", Policy = "HasNationality")] // autoryzacja w oparciu o role użytkownika oraz w oparciu o własną (customową) politykę o nazwie "HasNationality" zdefiniowaną w Program.cs
        //[Authorize(Policy = "IsCreator")]
        public ActionResult<PageResult<RestaurantDto>> GetAll([FromQuery] RestaurantQuery query)
        {
            return Ok(_restaurantService.GetAll(query));
        }

        [HttpGet("{restaurantId}")]
        [Authorize(Policy = "IsAdult")]  // autoryzacja w oparciu o własną (customową) politykę o nazwie "IsAdult" zdefiniowaną w Program.cs. Jest to polityka ze zdefiniowanymi własnymi (customowymi) wymaganiami
        //[AllowAnonymous] // umożliwia wykonanie akcji bez autoryzacji
        public ActionResult<RestaurantDto> Get([FromRoute] int restaurantId)
        {
            var restaurantDto = _restaurantService.GetById(restaurantId);
            return Ok(restaurantDto);
        }
    }
}
