// Program.cs
using Grind.Data;
using Grind.Core.Interfaces;
using Grind.Service.Services;
using Grind.Service.MappingProfiles;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Grind.Core.DTOs; // ודא שזה נחוץ
using System.Text.Json.Serialization;
using System.Text.Json;

// ייבוא עבור JWT
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// ייבוא עבור OpenApi (Swagger)
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http; // ודא שזה קיים עבור IHttpContextAccessor

var builder = WebApplication.CreateBuilder(args);

// הוספת שירותים למאגר ה-DI (Dependency Injection)

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();

// ********************************************************************************
// הגדרת SwaggerGen עם תמיכה ב-JWT Bearer Token
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Grind API", Version = "v1" });

    // הגדרת סכימת האבטחה עבור JWT Bearer
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\n\nExample: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
    });

    // הוספת דרישת האבטחה עבור כל ה-endpoints שמוגדרים כמאובטחים
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {} // כאן ניתן לציין Scope-ים אם יש, אך עבור JWT זה בדרך כלל ריק
        }
    });
});
// ********************************************************************************

// ********************************************************************************
// הגדרת CORS Policy - חובה שתהיה לפני הוספת שירותי DB/Authentication אם משתמשים בהם
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:5173") // 👈 ודא שזה הפורט הנכון של ה-React Vite שלך!
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials()); // מאפשר העברת קוקיז (Refresh Tokens) ו-Authorization Headers
});
// ********************************************************************************

// הגדרת בסיס נתונים (Entity Framework Core)
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// הוספת AutoMapper
builder.Services.AddAutoMapper(typeof(MapperProfile).Assembly);

// רישום השירותים (Service Layer)
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<ITrainerService, TrainerService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// הוסף את השורה הזו כדי ש-IHttpContextAccessor יהיה זמין להזרקה (Dependency Injection)
builder.Services.AddHttpContextAccessor();

// *** התחלת הגדרת JWT Authentication ***
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // ב-production, כדאי שיהיה true
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true, // ולידציה של זמן תפוגה
        ClockSkew = TimeSpan.Zero // אין סטייה של זמן
    };
});

builder.Services.AddAuthorization();
// *** סוף הגדרת JWT Authentication ***


var app = builder.Build();

// הגדרת ה-HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ********************************************************************************
// סדר המידלוויר חשוב מאוד!
// UseRouting צריך להיות לפני UseCors, UseAuthentication, UseAuthorization
app.UseRouting();

// ** UseCors חייב להיות אחרי UseRouting ולפני UseAuthentication/UseAuthorization **
app.UseCors("AllowSpecificOrigin");

// מידלוויר אימות והרשאות
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// ********************************************************************************


// הפעלת מיגרציות ו-seeding נתונים (אופציונלי, לפיתוח)
// וודא/י שה-DataContext מוגדר כראוי ושהמיגרציות קיימות
using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    dataContext.Database.Migrate();
    // אם את רוצה להפעיל seeding, ודא שהקובץ DataSeeder.cs קיים ומוגדר כראוי
    // DataSeeder.Seed(dataContext);
}

app.Run();