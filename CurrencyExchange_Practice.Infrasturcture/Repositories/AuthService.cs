using Azure.Core;
using CurrencyExchange_Practice.Application.Services;
using CurrencyExchange_Practice.Core;
using CurrencyExchange_Practice.Core.AuthDtos;
using CurrencyExchange_Practice.Core.Entities;
using CurrencyExchange_Practice.Core.Interfaces;
using CurrencyExchange_Practice.Core.OtherObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyExchange_Practice.Infrasturcture.Repositories
{
    public class AuthService : IAuthService
    {
        readonly UserManager<User> _userManager;
        readonly RoleManager<IdentityRole> _roleManager;
        readonly IConfiguration _configuration;
        readonly IService<RefreshToken> _service;

        public AuthService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IService<RefreshToken> service, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _service = service;
            _configuration = configuration;
        }

        public async Task<AuthServiceResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user is null || user.Email != loginDto.Email)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Invalid Cedentials"
                };
            }

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!isPasswordCorrect)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Invalid Cedentials"
                };
            }

            string accessToken = await GenerateNewAccessToken(user);
            string refreshToken = await GenerateNewRefreshToken(user);

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "Login successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthServiceResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var isExistsUser = await _userManager.FindByEmailAsync(registerDto.Email);

            if (isExistsUser is not null)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Email is already taken."
                };
            }

            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Passwords do not match"
                };
            }

            User newUser = new()
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var createUser = await _userManager.CreateAsync(newUser, registerDto.Password);

            if (!createUser.Succeeded)
            {
                string errorMessage = "User Creation Failed Because: ";

                foreach (var error in createUser.Errors)
                {
                    errorMessage += $"# {error.Description}";
                }

                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = errorMessage
                };
            }

            await _userManager.AddToRoleAsync(newUser, StaticUserRoles.USER);

            string accessToken = await GenerateNewAccessToken(newUser);
            string refreshToken = await GenerateNewRefreshToken(newUser);

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "Login successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private async Task<string> GenerateNewAccessToken(User user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("JWTID", Guid.NewGuid().ToString()),
            };

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var tokenOjbect = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddMinutes(5),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256));

            string token = new JwtSecurityTokenHandler().WriteToken(tokenOjbect);

            return token;
        }

        private async Task<string> GenerateNewRefreshToken(User user)
        {
            var randomNumber = new byte[64];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }

            string token = Convert.ToBase64String(randomNumber);

            RefreshToken refreshToken = new()
            {
                Token = token,
                UserId = user.Id,
                ExpDate = DateTime.Now.AddDays(7),
                User = user
            };

            await _service.AddAsync(refreshToken);

            return token;
        }

        public async Task<AuthServiceResponseDto> MakeAdminAsynce(UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (user is null)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Invalid User Name"
                };
            }

            await _userManager.AddToRoleAsync(user, StaticUserRoles.ADMIN);

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "User is now an Admin"
            };
        }

        public async Task<AuthServiceResponseDto> MakeOwnerAsynce(UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (user is null)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Invalid User Name"
                };
            }

            await _userManager.AddToRoleAsync(user, StaticUserRoles.OWNER);

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "User is now an Owner"
            };
        }

        public async Task<AuthServiceResponseDto> SeedRolesAsync()
        {
            bool isOwnerExists = await _roleManager.RoleExistsAsync(StaticUserRoles.OWNER),
             isAdminExists = await _roleManager.RoleExistsAsync(StaticUserRoles.ADMIN),
             isUserExists = await _roleManager.RoleExistsAsync(StaticUserRoles.USER);

            if (isOwnerExists && isAdminExists && isUserExists)
            {
                return new AuthServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Roles Seeding is already Done"
                };
            }

            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.OWNER));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.ADMIN));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.USER));

            return new AuthServiceResponseDto()
            {
                IsSucceed = true,
                Message = "Role Seeding Done Successfully"
            };
        }
    }
}
