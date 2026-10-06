using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
    private IUserRepository userRepository;
    public UsersController(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    [HttpGet("{userId}")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] object user)
    {
        throw new NotImplementedException();
    }

    [HttpPut("{userId}")]
    public IActionResult UpdateUser([FromRoute] Guid userId, [FromBody] UserUpdateDto userDTO)
    {
        if (!ModelState.IsValid)
            return UnprocessableEntity();
        var newUser = new UserEntity(userId)
        {
            FirstName = userDTO.firstName,
            LastName = userDTO.lastName,
            Login = userDTO.Login
        };
        var currentUser = userRepository.FindById(userId);
        if (currentUser is not null)
        {
            newUser.CurrentGameId = currentUser.CurrentGameId;
            newUser.GamesPlayed = currentUser.GamesPlayed;
        }
        userRepository.UpdateOrInsert(newUser, out var inserted);
        return Ok();
    }
}