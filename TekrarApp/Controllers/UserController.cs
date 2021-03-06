using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
            if (!ModelState.IsValid)
            {
                return HttpStatusCode.BadRequest;
            }

            Regex regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$");

            if(!regex.IsMatch(user.Password)){
                return HttpStatusCode.BadRequest;
            }

            if (await context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return HttpStatusCode.Forbidden;
            }

            TokenHandler tokenHandler = new TokenHandler(configuration);
            user.ConfirmEmailToken = tokenHandler.CreateEmailConfirmToken().ConfirmToken;
            user.Password = _sha.Encrypt(user.Password);
            context.Users.Add(user);
            UserRole userRole = new UserRole();
            userRole.User = user;
            userRole.Role = context.Roles.Find(1002);
            context.UserRoles.Add(userRole);
            int result = await context.SaveChangesAsync();

            if (result == 0)
                return HttpStatusCode.BadRequest;
            else
            {
                //var code = _sha.Encrypt(user.Email);

               

                StringBuilder mailbuilder = new StringBuilder();
                mailbuilder.Append("<html>");
                mailbuilder.Append("<head>");
                mailbuilder.Append("<meta charset= utf-8 />");
                mailbuilder.Append("<title>Email Onaylama</title>");
                mailbuilder.Append("</head>");
                mailbuilder.Append("<body>");
                mailbuilder.Append($"<p>Merhaba {user.Name}</p><br/>");
                mailbuilder.Append($"Mail adresinizi onaylamak için aşağıda ki bağlantı adresien tıklayınız.<br/>");
                mailbuilder.Append($"<a onclick='window.close()' href='https://localhost:7024/api/User/ConfirmEmail/?uid={user.Id}&code={user.ConfirmEmailToken}'>Email adresinizi onaylayın.");
                mailbuilder.Append("</body>");
                mailbuilder.Append("</html>");

                EmailHelper emailHelper = new EmailHelper();
                bool isSend = emailHelper.SendEmail(user.Email, mailbuilder.ToString(), Emails.bticaret01Email, Emails.bticaret01Password, "Üyelik Onaylama");

                if (isSend)
                    return HttpStatusCode.OK;
                else
                    return HttpStatusCode.BadRequest;
            }
        }

        [HttpPost("[action]")]
        public async Task<HttpStatusCode> Login(UserLogin userLogin)
        {
            if (!await context.Users.AnyAsync(x => x.Email == userLogin.Email && x.Password == _sha.Encrypt(userLogin.Password)))
                return HttpStatusCode.Forbidden;
            else if (await context.Users.AnyAsync(x => x.Email == userLogin.Email && x.Password == _sha.Encrypt(userLogin.Password) && x.IsConfirmEmail == false))
                return HttpStatusCode.Unauthorized;
            else
            {
                User user = await context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(x => x.Email == userLogin.Email && x.Password == _sha.Encrypt(userLogin.Password) && x.IsConfirmEmail == true);

                if (user != null)
                {
                    TokenHandler tokenHandler = new TokenHandler(configuration);
                    Token token = tokenHandler.CreateAccessToken(user);
                    user.RefreshToken = token.RefreshToken;
                    user.RefrestTokenEndDate = token.Expiration.AddMinutes(3);
                    await context.SaveChangesAsync();

                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddHours(24)
                    };

                    Response.Cookies.Append("AccessToken", token.AccessToken, cookieOptions);
                    //Response.Cookies.Append("RefreshToken", user.RefreshToken, cookieOptions);

                    return HttpStatusCode.OK;
                }
                return HttpStatusCode.BadRequest;
            } 
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> RefreshTokenLogin([FromForm] string refreshToken)
        {
            User user = await context.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
            if (user != null && user?.RefrestTokenEndDate > DateTime.Now)
            {
                TokenHandler tokenHandler = new TokenHandler(configuration);
                Token token = tokenHandler.CreateAccessToken(user);

                user.RefreshToken = token.RefreshToken;
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

        [HttpGet("[action]")]
        public async Task<IActionResult> ConfirmEmail(string uid, string code)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddMinutes(1)
            };
            Response.Cookies.Append("ConfirmToken", code, cookieOptions);
            if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(code))
            {
                var user = await context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(uid));
         
                if (code == user.ConfirmEmailToken && user.IsConfirmEmail == false)
                {
                    user.IsConfirmEmail = true;
                    context.Users.Update(user);
                    context.SaveChangesAsync();

                    return Redirect("https://localhost:3001/ConfirmEmail/:true");
                }
            }

            return Redirect("https://localhost:3001/ConfirmEmail/:false");
        }

        [HttpPost("[action]")]
        public HttpStatusCode userContact(UserContact user)
        {
            StringBuilder mailbuilder = new StringBuilder();
            mailbuilder.Append("<html>");
            mailbuilder.Append("<head>");
            mailbuilder.Append("<meta charset= utf-8 />");
            mailbuilder.Append("<title>Kullanıcı Mesajı</title>");
            mailbuilder.Append("</head>");
            mailbuilder.Append("<body>");
            mailbuilder.Append($"<h1>Ad/Soyad: {user.name} {user.surname}</h1><br/>");
            mailbuilder.Append($"<h3>Mail: {user.mail}</h3><br/>");
            mailbuilder.Append($"<b>Mesaj:</b> <p>{user.message}</p>");
            mailbuilder.Append("</body>");
            mailbuilder.Append("</html>");

            EmailHelper emailHelper = new EmailHelper();
            bool isSend = emailHelper.SendEmail(Emails.bticaret01Email, mailbuilder.ToString(), Emails.usercontactEmail, Emails.usercontactPassword, user.subject);

            if (isSend)
                return HttpStatusCode.OK;
            else
                return HttpStatusCode.BadRequest;
        }
    }
}
