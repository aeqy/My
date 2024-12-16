using Microsoft.AspNetCore.Mvc;

namespace MyCc.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{

    private readonly Random _random;
    public AccountController()
    {
        _random = new Random();
    }
    // GET: api/RandomNumber
    [HttpGet]
    public IActionResult GetRandomNumber()
    {
        int randomNumber = _random.Next(1, 101); // 生成1到100之间的随机数
        return Ok(new { Number = randomNumber });
    }
}