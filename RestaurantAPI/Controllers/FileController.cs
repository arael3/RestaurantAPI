using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace RestaurantAPI.Controllers
{
    [Route("file")]
    //[Authorize]
    public class FileController : ControllerBase
    {
        [HttpGet]
        [ResponseCache(Duration = 1200, VaryByQueryKeys = new[] {"fileName"})] // keszowanie pliku
        public ActionResult GetFile([FromQuery] string fileName)
        {
            var rootPath = Directory.GetCurrentDirectory();
            var filePath = $"{rootPath}/PrivateFiles/{fileName}";

            var fileExist = System.IO.File.Exists(filePath);

            if (!fileExist)
            {
                return NotFound();
            }

            var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
            fileExtensionContentTypeProvider.TryGetContentType(filePath, out string contentType);

            var fileContent = System.IO.File.ReadAllBytes(filePath);

            return File(fileContent, contentType, fileName);
        }

        [HttpPost]
        public ActionResult Upload([FromForm] IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var rootPath = Directory.GetCurrentDirectory();
                var fileName = file.FileName;
                var fullPath = $"{rootPath}/PrivateFiles/{fileName}";
                using(var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                return Ok();
            }

            return BadRequest();
        }
    }
}
