using GayDetectorBot.WebApi.Models;
using GayDetectorBot.WebApi.Models.Users;
using GayDetectorBot.WebApi.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GayDetectorBot.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [Authorize]
    [HttpGet("[action]")]
    [ProducesResponseType(typeof(IEnumerable<User>), 200)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return BadRequest(new ErrorResponse { Error = e.Message });
        }
    }

    [Authorize]
    [HttpGet("[action]")]
    [ProducesResponseType(typeof(User), 200)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([FromQuery] int userId)
    {
        try
        {
            var user = await _userService.GetByIdAsync(userId);
            return Ok(user);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return BadRequest(new ErrorResponse { Error = e.Message });
        }
    }

    [Authorize]
    [HttpGet("[action]")]
    [ProducesResponseType(typeof(User), 200)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByUsername([FromQuery] string username)
    {
        try
        {
            var user = await _userService.GetByUsernameAsync(username);
            return Ok(user);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return BadRequest(new ErrorResponse { Error = e.Message });
        }
    }
}