using InvoiceManagementSystem.DAL.Repositories;
using InvoiceManagementSystem.BLL.Interfaces;
using InvoiceManagementSystem.BLL.Services;
using InvoiceManagementSystem.DAL.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InvoiceManagementSystem.BLL.CQRS.Commands;
using InvoiceManagementSystem.BLL.CQRS.Queries;
using FluentValidation;
using FluentValidation.AspNetCore;
using InvoiceManagementSystem.API.Validators;
using InvoiceManagementSystem.DAL.Entities;
using InvoiceManagementSystem.API.Security;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

// ✅ Add Controllers
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateInvoiceValidator>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });


// ✅ ✅ ADD CORS HERE
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// ✅ Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer <token>'"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ✅ Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Dependency Injection (BLL)
builder.Services.AddScoped<IInvoiceLineItemRepository, InvoiceLineItemRepository>();
builder.Services.AddScoped<IInvoiceLineItemService, InvoiceLineItemService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<GetInvoiceByIdHandler>();
builder.Services.AddScoped<CreateInvoiceHandler>();
builder.Services.AddScoped<CreatePaymentHandler>();
builder.Services.AddScoped<InvoiceAnalyticsService>();

// ✅ Authentication
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
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// ✅ Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Configuration"] ?? "localhost:6379";
    options.InstanceName = builder.Configuration["Redis:InstanceName"] ?? "InvoiceApp_";
});

// ✅ Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var maxAttempts = 24;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            dbContext.Database.Migrate();

            var demoUsers = new[]
            {
                new User { Username = "admin", Password = "1234", Role = "Admin" },
                new User { Username = "manager", Password = "1234", Role = "FinanceManager" },
                new User { Username = "financeuser", Password = "1234", Role = "FinanceUser" }
            };

            foreach (var demoUser in demoUsers)
            {
                var existingUser = dbContext.Users
                    .FirstOrDefault(user => user.Username == demoUser.Username);

                if (existingUser == null)
                {
                    demoUser.Password = PasswordHasher.HashPassword(demoUser.Password);
                    dbContext.Users.Add(demoUser);
                    continue;
                }

                var changed = false;

                if (!PasswordHasher.VerifyPassword(demoUser.Password, existingUser.Password))
                {
                    existingUser.Password = PasswordHasher.HashPassword(demoUser.Password);
                    changed = true;
                }

                if (existingUser.Role != demoUser.Role)
                {
                    existingUser.Role = demoUser.Role;
                    changed = true;
                }

                if (changed)
                {
                    dbContext.Users.Update(existingUser);
                }
            }

            if (!dbContext.Customers.Any())
            {
                dbContext.Customers.AddRange(
                    new Customer
                    {
                        Name = "Acme Corp",
                        Email = "accounts@acme.test",
                        Phone = "9999999991",
                        Address = "Mumbai"
                    },
                    new Customer
                    {
                        Name = "Globex Pvt Ltd",
                        Email = "billing@globex.test",
                        Phone = "9999999992",
                        Address = "Bengaluru"
                    },
                    new Customer
                    {
                        Name = "Initech Solutions",
                        Email = "finance@initech.test",
                        Phone = "9999999993",
                        Address = "Delhi"
                    });
            }

            dbContext.SaveChanges();
            break;
        }
        catch (Exception ex) when (
            ex is SqlException ||
            ex.InnerException is SqlException)
        {
            if (attempt == maxAttempts)
            {
                throw;
            }

            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }
}

app.UseHttpsRedirection();

// ✅ ✅ ADD CORS HERE (VERY IMPORTANT POSITION)
app.UseCors("AllowFrontend");

app.UseAuthentication();

app.UseMiddleware<InvoiceManagementSystem.API.Middleware.ExceptionMiddleware>();

app.UseAuthorization();

// ✅ Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Map Controllers
app.MapControllers();

app.Run();
