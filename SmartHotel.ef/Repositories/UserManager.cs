using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartHotel.core.Contracts;
using SmartHotel.core.Dtos.User;
using SmartHotel.core.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartHotel.ef.Repositories
{
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApiUser> _userManager;

        private readonly IConfiguration _config;


        private readonly RoleManager<IdentityRole> _roleManager;

        //constants  ===> must be deleted
        private const string _loginProvider = "HotelListingApi";
        private const string _refreshToken = "RefreshToken";
        public AuthManager(IMapper mapper, UserManager<ApiUser> userManager, IConfiguration config, RoleManager<IdentityRole> roleManager) 
        {
            _mapper = mapper;
            _userManager = userManager;
            _config = config;
            _roleManager = roleManager;
        }

        public async Task<AuthModel> RegisterAsync(RegisterUserDto userDto)
        {
            if (await _userManager.FindByEmailAsync(userDto.Email) is not null)
            {
                return new AuthModel { Message = "Email is already registered!" };
            }

            if (await _userManager.FindByNameAsync(userDto.Username) is not null)
            {
                return new AuthModel { Message = "Username is already registered!" };
            }
            var user = new ApiUser
            {
                UserName = userDto.Username,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName
            };

            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Empty;

                foreach (var error in result.Errors)
                    errors += $"{error.Description},";

                return new AuthModel { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, "User");

            var jwtSecurityToken = await CreateJwtToken(user);

            return new AuthModel
            {
                Email = user.Email,
                ExpiresOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Roles = new List<string> { "User" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Username = user.UserName
            };

        }


        public async Task<AuthModel> GetTokenAsync(LoginUserDto model)
        {
            var authModel = new AuthModel();

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authModel.Message = "Email or Password is incorrect!";
                return authModel;
            }

            var jwtSecurityToken = await CreateJwtToken(user);
            var rolesList = await _userManager.GetRolesAsync(user);

            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.Email = user.Email;
            authModel.Username = user.UserName;
            authModel.ExpiresOn = jwtSecurityToken.ValidTo;
            authModel.Roles = rolesList.ToList();

            return authModel;
        }

        public async Task<string> AddRoleAsync(AddToRoleDto model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user is null || !await _roleManager.RoleExistsAsync(model.Role))
                return "Invalid user ID or Role";

            if (await _userManager.IsInRoleAsync(user, model.Role))
                return "User already assigned to this role";

            var result = await _userManager.AddToRoleAsync(user, model.Role);

            return result.Succeeded ? string.Empty : "Sonething went wrong";
        }


        private async Task<JwtSecurityToken> CreateJwtToken(ApiUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();


            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName), //contain user identifier or username
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email), //email claims
                new Claim("uid", user.Id)
            }
            .Union(userClaims)
            .Union(roleClaims);

            //convert claims from a string to bytes using UTF-8 encoding
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);


            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(Convert.ToInt32(_config["JWT:DurationInDays"])),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;

        }

        
    }
}
