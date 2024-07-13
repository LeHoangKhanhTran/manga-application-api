using MangaApplication.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
string connectionString = "";
string cloudinaryURL = "";
string jwtKey = "";
string[] allowedOrigins = new string[]{ env.IsDevelopment() ? "http://localhost:5173" : "https://manga-application-frontend.vercel.app"};
if (env.IsDevelopment()) 
{
    connectionString = builder.Configuration["Database:ConnectionString"];
    cloudinaryURL = builder.Configuration["Cloudinary:CloudinaryURL"];
    jwtKey = builder.Configuration["Authentication:JwtKey"];
}
else 
{
    connectionString = Environment.GetEnvironmentVariable("ConnectionString");
    cloudinaryURL = Environment.GetEnvironmentVariable("CloudinaryURL");
    jwtKey = Environment.GetEnvironmentVariable("JwtKey");
}
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => 
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

        // Include JWT token in Swagger requests
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter JWT token",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("Allow All", builder => 
    {
        builder.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IImageUploader, CloudinaryImageUploader>(serviceProvider => {
    Cloudinary cloudinary = new (cloudinaryURL);
    cloudinary.Api.Secure = true;
    return new(cloudinary);
});
builder.Services.AddSingleton<IMongoClient>(serviceProvider => 
{
    return new MongoClient(connectionString);
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.HttpOnly = true;
    options.LoginPath = "/user/authenticate";
    options.Cookie.Name = "access_token";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, bearer =>
{
    bearer.RequireHttpsMetadata = false;
    bearer.SaveToken = true;
    bearer.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});




builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IAuthorizationHandler, CustomAuthorizationHandler>();
builder.Services.AddAuthorization(options => 
{
    options.AddPolicy("AdministratorPolicy", policy => 
    {
        // policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new HttpOnlyCookieJwtRequirement());
    });
    
});
BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
BsonSerializer.RegisterSerializer(typeof(decimal?), new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseRouting();

app.UseCors("Allow All");

app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints => 
{
    endpoints.MapControllers();
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://*:{port}");

// app.Run();

