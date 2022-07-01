using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TekrarApp.Context;
using TekrarApp.Helpers;
using TekrarApp.Model;

namespace TekrarApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly JwtDbContext context;
        private readonly IConfiguration configuration;
        private readonly Sha1Hash _sha; 

        public UserController(JwtDbContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
            _sha = new Sha1Hash();
        }

        [HttpPost("[action]")]
        public async Task<HttpStatusCode> Register(User user)
        {

            if (await context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return HttpStatusCode.Forbidden;
            }

            user.Password = _sha.Encrypt(user.Password);
            context.Users.Add(user);
            int result = await context.SaveChangesAsync();

            if (result == 0)
                return HttpStatusCode.BadRequest;
            else
            {
                return HttpStatusCode.OK;
            }
        }

        [HttpPost("[action]")]
        public async Task<HttpStatusCode> Login(UserLogin userLogin)
        {
            User user = await context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(x=>x.Email == userLogin.Email && x.Password == userLogin.Password);
            
            if(user != null)
            {
                TokenHandler tokenHandler = new TokenHandler(configuration);
                Token token = tokenHandler.CreateAccessToken(user);
                user.RefreshToken = _sha.Encrypt(token.RefreshToken);
                user.RefrestTokenEndDate = token.Expiration.AddMinutes(3);
                await context.SaveChangesAsync();

                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddHours(100000)
                };

                Response.Cookies.Append("AccessToken", _sha.Encrypt(token.AccessToken), cookieOptions);
                Response.Cookies.Append("RefreshToken", user.RefreshToken, cookieOptions);

                return HttpStatusCode.OK;
            }

            return HttpStatusCode.Forbidden;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> RefreshTokenLogin([FromForm] string refreshToken)
        {
            User user = await context.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
            if (user != null && user?.RefrestTokenEndDate > DateTime.Now)
            {
                TokenHandler tokenHandler = new TokenHandler(configuration);
                Token token = tokenHandler.CreateAccessToken(user);

                user.RefreshToken = _sha.Encrypt(token.RefreshToken);
                user.RefrestTokenEndDate = token.Expiration.AddMinutes(3);
                await context.SaveChangesAsync();


                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddHours(100000)
                };

                Response.Cookies.Delete("RefreshToken");
                Response.Cookies.Append("RefreshToken", user.RefreshToken, cookieOptions);


                return Ok("success");
            }
            return Problem("error");
        }
    }
}
