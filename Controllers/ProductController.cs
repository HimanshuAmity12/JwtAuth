using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProducts()
        {
            var products = new[]
            {
                new { Id = 1, Name = "Laptop", Price = 50000 },
                new { Id = 2, Name = "Mouse", Price = 800 }
            };

            return Ok(products);
        }
    }
}
