using System.Text;
using lfg;
using lfg.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson(
    options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
string azureCS = Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection");
builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(azureCS is null ? builder.Configuration.GetConnectionString("DefaultConnection") : azureCS));
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<GameRepository>();
builder.Services.AddScoped<RequestRepository>();
string token = Environment.GetEnvironmentVariable("Token");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(token is null ? builder.Configuration.GetSection("AppSettings:Token").Value : token)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

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

app.MapControllers();

app.Run();
