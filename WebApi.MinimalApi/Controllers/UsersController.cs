using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
    private readonly IUserRepository userRepository;
    private readonly IMapper mapper;

    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        this.userRepository = userRepository;
        this.mapper = mapper;
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] CreateUserRequest user)
    {
        if (user is null)
            return BadRequest();

        if (!string.IsNullOrEmpty(user.Login) && !user.Login.All(char.IsLetterOrDigit))
            ModelState.AddModelError(nameof(user.Login), "Login should contain only letters or digits");

        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        var userEntity = mapper.Map<UserEntity>(user);
        var createdUserEntity = userRepository.Insert(userEntity);

        return CreatedAtRoute(
            nameof(GetUserById),
            new { userId = createdUserEntity.Id },
            createdUserEntity.Id);
    }

    [HttpDelete("{userId}")]
    [Produces("application/json", "application/xml")]
    public IActionResult DeleteUser([FromRoute] Guid userId)
    {
        var user = userRepository.FindById(userId);
        if (user is null)
            return NotFound();

        userRepository.Delete(userId);
        return NoContent();
    }

    [HttpGet("{userId}", Name = nameof(GetUserById))]
    [HttpHead("{userId}")]
    [Produces("application/json", "application/xml")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var entity = userRepository.FindById(userId);
        if (entity == null) return NotFound();

        var dto = mapper.Map<UserDto>(entity);

        if (HttpMethods.IsHead(Request.Method))
        {
            Response.ContentType = "application/json; charset=utf-8";
            return Ok();
        }

        return Ok(dto);         
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