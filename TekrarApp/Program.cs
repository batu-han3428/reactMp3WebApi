using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using TekrarApp.Model;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager Configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<JwtDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("JwtConStr")));

builder.Services.AddCors(options => options.AddDefaultPolicy(policy => {
    policy.WithOrigins(new[] { "http://localhost:3000", "http://localhost:8080", "http://localhost:4200", "http://localhost:7024" })
            .AllowAnyHeader()
            .AllowAnyMethod()
                .AllowCredentials();                   
}));


var key = Encoding.ASCII.GetBytes(Configuration["Token:SecurityKey"]);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Configuration["Token:Issuer"],
        ValidAudience = Configuration["Token:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Token:SecurityKey"])),
        ClockSkew = TimeSpan.Zero
    };
});

//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(x =>
//{
//    x.LoginPath = "/login#";
//    x.LogoutPath = "/public/login";
//    x.AccessDeniedPath = "/public/login";
//    x.Cookie.HttpOnly = false;
//    x.Cookie.Name = "TokenUserCookie";
//    x.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
//    x.ExpireTimeSpan = TimeSpan.FromMinutes(5);
//    x.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;

//});
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
//        options.SlidingExpiration = true;
//        options.AccessDeniedPath = "/Forbidden/";
//        options.LoginPath = "/login";
//        options.LogoutPath = "/index.html";
//        options.Cookie.HttpOnly = false;
//        options.Cookie.Name = "TokenUserCookie";
//        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
//        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
//        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
//    });



//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
//{
//    options.LoginPath = @"/http://localhost:3000/login#";
//});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseCors();

app.MapControllers();

//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(
//        Path.Combine(builder.Environment.ContentRootPath, "MyStaticFiles")),
//    RequestPath = "C:/Users/bfindik/Desktop"
//});

app.Run();
