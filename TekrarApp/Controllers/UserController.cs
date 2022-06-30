using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TekrarApp.Model;

namespace TekrarApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly JwtDbContext context;
        private readonly IConfiguration configuration;

        public UserController(JwtDbContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        [HttpPost("[action]")]
        public async Task<bool> Create([FromForm] User user)
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return true;
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<Token>> Login( UserLogin userLogin)
        {
            User user = await context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(x=>x.Email == userLogin.Email && x.Password == userLogin.Password);
            
            if(user != null)
            {
                TokenHandler tokenHandler = new TokenHandler(configuration);
                Token token = tokenHandler.CreateAccessToken(user);
                user.RefreshToken = token.RefreshToken;
                user.RefrestTokenEndDate = token.Expiration.AddMinutes(3);
                await context.SaveChangesAsync();

                //var claims = new List<Claim>()
                //{
                //    new Claim("AccessToken",token.AccessToken),
                //    new Claim("RefreshToken",token.RefreshToken)
                //};
                //var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                //ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);
                //await HttpContext.SignInAsync(principal);


                var cookieOptions = new CookieOptions
                {
                    //Expires = DateTime.UtcNow.AddHours(8),
                    //Path = "/login",
                };
                Response.Cookies.Append("RefreshToken", token.RefreshToken, cookieOptions);
   
                //Response.Cookies.Append(key:"jwt",value:"jwy",new CookieOptions { HttpOnly=true});
                //HttpContext.Response.Cookies.Append("AccessToken", token.AccessToken, cookieOptions);

                //var claims = new List<Claim>
                //    {
                //       new Claim("AccessToken",token.AccessToken),
                //       new Claim("RefreshToken",token.RefreshToken)
                //    };
                //var claimIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                //var authenticationProperty = new AuthenticationProperties
                //{
                //    RedirectUri = @"/public/login"
                //};


                //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme
                //                        , new ClaimsPrincipal(claimIdentity)
                //                        , authenticationProperty);



                //        var claims = new List<Claim>
                //{
                //    new Claim(ClaimTypes.Name, user.Email),
                //    new Claim("Token", user.RefreshToken),
                //    new Claim(ClaimTypes.Role, "Administrator"),
                //};

                //        var claimsIdentity = new ClaimsIdentity(
                //            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                //        var authProperties = new AuthenticationProperties
                //        {
                //            //AllowRefresh = <bool>,
                //            // Refreshing the authentication session should be allowed.

                //            //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                //            // The time at which the authentication ticket expires. A 
                //            // value set here overrides the ExpireTimeSpan option of 
                //            // CookieAuthenticationOptions set with AddCookie.

                //            IsPersistent = true,
                //            // Whether the authentication session is persisted across 
                //            // multiple requests. When used with cookies, controls
                //            // whether the cookie's lifetime is absolute (matching the
                //            // lifetime of the authentication ticket) or session-based.

                //            IssuedUtc = DateTime.Now,
                //            // The time at which the authentication ticket was issued.

                //            RedirectUri = "http://localhost:3000/login"
                //            // The full path or absolute URI to be used as an http 
                //            // redirect response value.
                //        };

                //        await HttpContext.SignInAsync(
                //            CookieAuthenticationDefaults.AuthenticationScheme,
                //            new ClaimsPrincipal(claimsIdentity),
                //            authProperties);


                //SHA1 sha = new SHA1CryptoServiceProvider();



                //string HashAccessToken = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(token.AccessToken)));
                //string HashRefreshToken = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(token.RefreshToken)));
                //token.AccessToken = HashAccessToken;
                //token.RefreshToken = HashRefreshToken;


                return Ok(token);
            }

            return null;
        }

        [HttpGet("[action]")]
        public async Task<Token> RefreshTokenLogin([FromForm] string refreshToken)
        {
            User user = await context.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
            if (user != null && user?.RefrestTokenEndDate > DateTime.Now)
            {
                TokenHandler tokenHandler = new TokenHandler(configuration);
                Token token = tokenHandler.CreateAccessToken(user);

                user.RefreshToken = token.RefreshToken;
                user.RefrestTokenEndDate = token.Expiration.AddMinutes(3);
                await context.SaveChangesAsync();

                return token;
            }
            return null;
        }
    }
}
