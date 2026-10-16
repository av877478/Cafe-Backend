using System.Text;
using Cafe.Data;
using Cafe.Repository.Abstarct;
using Cafe.Repository.Concreate;
using Cafe.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// 1. Controllers
// --------------------------------------------------
builder.Services.AddControllers();

// --------------------------------------------------
// 2. Database Context (PostgreSQL / Supabase)
// --------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// --------------------------------------------------
// 3. Dependency Injection (Pattern: Controller -> IRepository -> Repository -> Service)
// --------------------------------------------------
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();

builder.Services.AddScoped<TableService>();
builder.Services.AddScoped<ITableRepository, TableRepository>();

builder.Services.AddScoped<QrCodeService>();
builder.Services.AddScoped<IQrCodeRepository, QrCodeRepository>();

builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddScoped<KitchenService>();
builder.Services.AddScoped<IKitchenRepository, KitchenRepository>();

builder.Services.AddScoped<StaffService>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();

builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();

builder.Services.AddScoped<SettingService>();
builder.Services.AddScoped<ISettingRepository, SettingRepository>();

builder.Services.AddScoped<CustomerHomeService>();
builder.Services.AddScoped<ICustomerHomeRepository, CustomerHomeRepository>();

builder.Services.AddScoped<CustomerMenuService>();
builder.Services.AddScoped<ICustomerMenuRepository, CustomerMenuRepository>();

builder.Services.AddScoped<CustomerCartService>();
builder.Services.AddScoped<ICustomerCartRepository, CustomerCartRepository>();

// --------------------------------------------------
// 4. CORS Policy (For Angular Admin Frontend)
// --------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "https://cafe-ui-coral.vercel.app",
                "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// --------------------------------------------------
// 5. JWT Authentication & Authorization
// --------------------------------------------------
var jwtKey = builder.Configuration["Jwt:Key"] ?? "zzK45SVMtqn9zNjnuD7YXVdshbdvrtvrxoO6zE2IEm";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Cafe",

        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "Cafe Api",

        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddAuthorization();

// --------------------------------------------------
// 6. Swagger API Documentation & Testing Configuration
// --------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Little Talk Cafe - API Testing Interface",
        Version = "v1",
        Description = "Interactive Swagger UI for Little Talk Cafe Admin Registration, OTP Verification, and JWT Authentication."
    });

    // Add JWT Bearer Security Definition for Swagger UI "Authorize" Button
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token only (no need to write 'Bearer '). Example: eyJhbGciOi..."
    });

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
            Array.Empty<string>()
        }
    });
});

// --------------------------------------------------
// 7. Build Application Pipeline
// --------------------------------------------------
var app = builder.Build();

// Swagger Middleware for API Testing
app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cafe API v1");
//    c.RoutePrefix = "swagger"; // http://localhost:5050/swagger
//    c.DisplayRequestDuration();
//    c.EnableTryItOutByDefault();
//    c.DocumentTitle = "Cafe Management API - Swagger UI";
//});

app.UseCors("AllowFrontend");

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
