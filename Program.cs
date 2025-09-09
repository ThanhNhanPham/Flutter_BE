using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FoodOdering_BE.Hubs; // Namespace cho NotificationHub
using FoodOdering_BE.Model;
using FoodOdering_BE.Repository;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Register Repositories
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IQRCodeRepository, QRCodeRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();

// Add SignalR
builder.Services.AddSignalR(); // Thêm SignalR vào container dịch vụ

// Configure Controllers
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyAllowOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:5500", "http://192.168.1.228")
        //policy.WithOrigins("http://172.20.10.2")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // SignalR cần cho phép credentials
    });
});


// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JWTKey");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["ValidIssuer"],
        ValidAudience = jwtSettings["ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]))
    };
});

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
    options.AddPolicy("AdminOrUser", policy => policy.RequireRole("Admin", "User"));
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5239); // 5239 là port bạn đang dùng, giữ nguyên hoặc thay nếu khác
});


var app = builder.Build();

// Seed Roles at Startup
//using (var scope = app.Services.CreateScope())
//{
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//    var roles = new[] { "Admin", "User" };
//    foreach (var role in roles)
//    {
//        if (!await roleManager.RoleExistsAsync(role))
//        {
//            await roleManager.CreateAsync(new IdentityRole(role));
//        }
//    }
//}

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error"); // Custom error handling endpoint
    app.UseHsts(); // Enforce HTTPS in production
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("MyAllowOrigins"); // Apply CORS Policy
app.UseHttpsRedirection();
app.UseAuthentication(); // Authentication Middleware
app.UseAuthorization(); // Authorization Middleware

// Map SignalR Hubs
app.MapHub<NotificationHub>("/notificationHub"); // Định tuyến cho NotificationHub

app.MapControllers();


app.Run();

