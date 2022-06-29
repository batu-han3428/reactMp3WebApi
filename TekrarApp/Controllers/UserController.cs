using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<ActionResult<Token>> Login([FromForm] UserLogin userLogin)
        {
            User user = await context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(x=>x.Email == userLogin.Email && x.Password == userLogin.Password);
            
            if(user != null)
            {
                TokenHandler tokenHandler = new TokenHandler(configuration);
                Token token = tokenHandler.CreateAccessToken(user);
                user.RefreshToken = token.RefreshToken;
                user.RefrestTokenEndDate = token.Expiration.AddMinutes(3);
                await context.SaveChangesAsync();
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
