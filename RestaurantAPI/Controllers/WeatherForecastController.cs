using Microsoft.AspNetCore.Mvc;
using RestaurantAPI.Models;
using RestaurantAPI.Services;

namespace RestaurantAPI.Controllers
{
    [ApiController]
    [Route("[controller]")] // oznacza to, ¿e metody GET, POST itd. wywowy³ane w tym kontrolerze bêd¹ dostêpne pod adresem zgodnym z nazw¹ kontrolera (a dok³adnie t¹ czêœci¹ nazwy kotrolera przed s³ówkiem "Controller", pisan¹ ma³ymi literami.
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

        [HttpGet("get2/{max}")] // dodanie w nawiasie ("get2") oznacza, ¿e metoda Get2() bêdzie dostêpna pod adresem /weatherforecast/get2
        // /{max} odnosi siê do [FromRoute]int max
        // poni¿ej drugi sposób na ustalenie œcie¿ki dla metody, czyli zamiast dodawania ("get2") do HttpGet, poni¿ej [HttpGet] wprowadzamy [Route("adres")] 
        // [Route("get2")] 
        public IEnumerable<WeatherForecast> Get2([FromQuery]int take, [FromRoute]int max)
        {
            return _service.Get();
        }
        //aby przekazaæ wartoœæ dla parametru [FromRoute]int max, do adresu dopisujemy odpowiedni¹ wartoœæ np. /weatherforecast/get2/123
        //aby przekazaæ wartoœæ dla parametru [FromQuery]int take, do adresu dopisujemy odpowiednie zapytanie np. /weatherforecast/get2/123?take=50

        [HttpPost]
        public ActionResult<string> Hello([FromBody]string name)
        {
            // Pierwszy sposób
            return StatusCode(200, $"Hello {name}");

            // Drugi sposób
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