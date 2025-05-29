using CurrencyExchange_Practice.Core.AuthDtos;
using CurrencyExchange_Practice.Core;
using CurrencyExchange_Practice.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.AspNetCore.Identity;
using CurrencyExchange_Practice.Core.Entities;
using System.Data;

namespace CurrencyExchange_Practice.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase  
    {
        readonly IAuthService _authService;
        readonly UserManager<User> _userManager;
        readonly RoleManager<IdentityRole> _roleManager;
        readonly IService<RefreshToken> _refreshService;

        public AuthController(IAuthService authService, RoleManager<IdentityRole> roleManager, UserManager<User> userManager, IService<RefreshToken> refreshService)
        {
            _authService = authService;
            _userManager = userManager;
            _roleManager = roleManager;
            _refreshService = refreshService;
        }

        [HttpPost]
        [Route("Adding_Roles")]
        public async Task<ActionResult> SeedRoles()
        {
            var seedRole = await _authService.SeedRolesAsync();

            return Ok(seedRole);
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var register = await _authService.RegisterAsync(registerDto);

            if (!register.IsSucceed) { return BadRequest(); }

            var user = await _userManager.FindByEmailAsync(registerDto.Email);
            var roles = await _userManager.GetRolesAsync(user);

            var accessTokenCookie = new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(5)
            };

            var refreshTokenCookie = new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("accessToken", register.AccessToken, accessTokenCookie);
            Response.Cookies.Append("refreshToken", register.RefreshToken, refreshTokenCookie);

            return Ok(new
            {
                userName = user.UserName,
                userId = user.Id,
                userRole = roles
            });
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var login = await _authService.LoginAsync(loginDto);

            if (!login.IsSucceed) { return Unauthorized(); }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            var roles = await _userManager.GetRolesAsync(user);

            var accessTokenCookie = new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            var refreshTokenCookie = new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("accessToken", login.AccessToken, accessTokenCookie);
            Response.Cookies.Append("refreshToken", login.RefreshToken, refreshTokenCookie);

            return Ok(new
            {
                userName = user.UserName,
                userId = user.Id,
                userRole = roles
            });
        }


        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (refreshToken is null) { return Unauthorized("invalid refresh token!"); }

            var result = await _authService.ValidateRefreshToken(refreshToken);

            if (!result.IsSucceed ) { return Unauthorized("invalid refresh token!"); }

            var accessTokenCookie = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(15),
                SameSite = SameSiteMode.None,
                Secure = true
            };

            var refreshTokenCookie = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                SameSite = SameSiteMode.None,
                Secure = true
            };

            Response.Cookies.Append("accessToken", result.AccessToken, accessTokenCookie);
            Response.Cookies.Append("refreshToken", result.RefreshToken, refreshTokenCookie);

            return Ok(new { message = "Tokens refreshed" });
        }


        [HttpPost]
        [Route("make-admin")]
        public async Task<IActionResult> MakeAdmin([FromBody] UpdatePermissionDto updatePermissionDto)
        {
            var makeAdmin = await _authService.MakeAdminAsynce(updatePermissionDto);

            return Ok(makeAdmin);
        }


        [HttpPost]
        [Route("make-owner")]
        public async Task<IActionResult> MakeOwner([FromBody] UpdatePermissionDto updatePermissionDto)
        {
            var makeOwner = await _authService.MakeOwnerAsynce(updatePermissionDto);

            return Ok(makeOwner);
        }


        [HttpPost]
        [Route("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("accessToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });

            return Ok(new { message = "Logged out successfully" });
        }


        [HttpGet]
        [Route("get-profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var cookieRefreshToken = Request.Cookies["refreshToken"];
            var userRefreshToken = (await _refreshService.GetAsync(token => token.Token == cookieRefreshToken, includes: token => token.User)).FirstOrDefault();

            if (userRefreshToken is null || userRefreshToken.ExpDate < DateTime.UtcNow) { return Unauthorized(); }

            var user = userRefreshToken.User;
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                userName = user.UserName,
                userId = user.Id,
                userRole = roles
            });
        }
    }
}
