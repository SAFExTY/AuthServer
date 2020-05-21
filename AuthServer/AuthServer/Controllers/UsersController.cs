using AuthServer.Entities;
using Microsoft.AspNetCore.Mvc;
using AuthServer.Models;
using AuthServer.Services;
using Microsoft.AspNetCore.Authorization;

namespace AuthServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.Username, model.Password);
            if (user == null)
                return BadRequest();

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult CreateUser([FromBody] CreateModel model)
        {
            // Sign up with existing account, so log in
            if (_userService.Exist(model.Username, model.Email))
                return BadRequest(new {message = "This user is already existing"});
            var user = _userService.Create(new InternalUser
            {
                Email = model.Email,
                Username = model.Username,
                Password = model.Password,
                FirstName = model.FirstName,
                LastName = model.LastName
            });
            return Ok(user);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }
    }
}