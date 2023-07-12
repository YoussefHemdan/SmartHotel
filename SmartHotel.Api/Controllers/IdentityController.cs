using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartHotel.core.Contracts;
using SmartHotel.core.Dtos.User;
using System.Data;

namespace SmartHotel.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly IAuthManager _authManager;
        public IdentityController(IAuthManager authManager)
        {
            _authManager = authManager;
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register([FromBody] RegisterUserDto apiUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authManager.RegisterAsync(apiUserDto);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> RefreshToken([FromBody] LoginUserDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authManager.GetTokenAsync(request);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost("addrole")]
        [Authorize(Roles ="Administrator")]
        public async Task<IActionResult> AddRoleAsync([FromBody] AddToRoleDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authManager.AddRoleAsync(model);

            if (!string.IsNullOrEmpty(result))
                return BadRequest(result);

            return Ok(model);
        }


    }
}
