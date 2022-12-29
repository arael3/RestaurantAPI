using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.Models;
using RestaurantAPI.Services;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("[controller]")] // oznacza to, �e metody GET, POST itd. wywowy�ane w tym kontrolerze b�d� dost�pne pod adresem zgodnym z nazw� kontrolera (a dok�adnie t� cz�ci� nazwy kotrolera przed s��wkiem "Controller", pisan� ma�ymi literami.
    // np. dla WeatherForecastController adres to: /weatherforecast
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IWeatherForecastService _service;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherForecastService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return _service.Get();
        }

        [HttpGet("get2/{max}")] // dodanie w nawiasie ("get2") oznacza, �e metoda Get2() b�dzie dost�pna pod adresem /weatherforecast/get2
        // /{max} odnosi si� do [FromRoute]int max
        // poni�ej drugi spos�b na ustalenie �cie�ki dla metody, czyli zamiast dodawania ("get2") do HttpGet, poni�ej [HttpGet] wprowadzamy [Route("adres")] 
        // [Route("get2")] 
        public IEnumerable<WeatherForecast> Get2([FromQuery]int take, [FromRoute]int max)
        {
            return _service.Get();
        }
        //aby przekaza� warto�� dla parametru [FromRoute]int max, do adresu dopisujemy odpowiedni� warto�� np. /weatherforecast/get2/123
        //aby przekaza� warto�� dla parametru [FromQuery]int take, do adresu dopisujemy odpowiednie zapytanie np. /weatherforecast/get2/123?take=50

        [HttpPost]
        public ActionResult<string> Hello([FromBody]string name)
        {
            // Pierwszy spos�b
            return StatusCode(200, $"Hello {name}");

            // Drugi spos�b
            //return Ok($"Hello {name}"); // lub
            //return NotFound($"Hello {name}"); // lub
            //return BadRequest($"Hello {name}");
        }

        //12. Zadanie praktyczne: Akcje
        [HttpGet("get3")]
        public IEnumerable<WeatherForecast> Get3([FromQuery]int numberOfResults, [FromQuery]int minTempC, [FromQuery] int maxTempC)
        {
            return _service.Get(numberOfResults, minTempC, maxTempC);
        }

        [HttpPost("generate")]
        public ActionResult<IEnumerable<WeatherForecast>> Generate([FromQuery]int numberOfResults, [FromBody] MinMaxTempC minMaxTempC)
        {
            if (numberOfResults < 0)
            {
                return BadRequest("numberOfResults must be grater than 0");
            }
            else if (minMaxTempC.MaxTempC < minMaxTempC.MinTempC)
            {
                return BadRequest("MaxTempC must be grater than MinTempC");
            }
            
            var result = _service.Get(numberOfResults, minMaxTempC.MinTempC, minMaxTempC.MaxTempC);
            return StatusCode(200, result);
        }
    }
}