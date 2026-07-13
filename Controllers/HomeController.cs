using Microsoft.AspNetCore.Mvc;

namespace ApiAuth.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/home/status/[controller]")]

public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Bem-vindo à API Auth!");
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            status = "online",
            versao = "1.0.0"
        });
    }
}