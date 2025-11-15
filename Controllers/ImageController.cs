using System.Net;
using FastPMS.ImageRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FastPMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageRepo imageRepo;

        public ImageController(IImageRepo imageRepo)
        {
            this.imageRepo = imageRepo;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("this is GET img Api call");
        }

        [HttpPost]
        public async Task<IActionResult>UploadAsync(IFormFile file)
        {
            var ImageUrl=await imageRepo.UploadAsync(file);
            if (string.IsNullOrEmpty(ImageUrl))
            {
                return Problem("Image upload failed", null, (int)HttpStatusCode.InternalServerError);
            }

            return new JsonResult(new { link = ImageUrl });

        }
    }
}
