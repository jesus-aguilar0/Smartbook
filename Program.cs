using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Smartbook.LogicaDeNegocio.Extensions;
using Smartbook.LogicaDeNegocio.Mapping;
using Smartbook.LogicaDeNegocio.Services;
using Smartbook.Middleware;
using Smartbook.Persistencia.Data;
using Smartbook.Persistencia.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/smartbook-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar formato de fecha flexible
        options.JsonSerializerOptions.Converters.Add(new Smartbook.Converters.DateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new Smartbook.Converters.DateTimeNullableConverter());
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        // Permitir fechas en formato ISO 8601 y otros formatos comunes
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Personalizar respuestas de validación
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => new
                {
                    Field = x.Key,
                    Message = e.ErrorMessage
                }))
                .ToList();

            return new BadRequestObjectResult(new
            {
                message = "Error de validación en los datos enviados.",
                errors = errors
            });
        };
    });

// Configure Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartBook API",
        Version = "v1",
        Description = @"
## Sistema de Gestión de Inventario - Centro de Idiomas CECAR

### 🔐 Autenticación con JWT Token

**Paso 1: Obtener Token**
1. Usa el endpoint `POST /api/usuarios/login` con tus credenciales
2. Copia el `token` de la respuesta

**Paso 2: Usar el Token**
1. Haz clic en el botón **'Authorize'** 🔓 (arriba a la derecha)
2. Pega el token en el campo (sin la palabra 'Bearer')
3. Haz clic en **'Authorize'** y luego en **'Close'**
4. Ahora todos los endpoints protegidos usarán automáticamente tu token

**Credenciales por defecto:**
- Email: `admin@cecar.edu.co`
- Contraseña: `AdminCDI123!`

### 📋 Notas Importantes
- El token expira en 1 hora
- Los endpoints marcados con 🔒 requieren autenticación
- Los endpoints marcados con 👑 requieren rol Admin
- Los endpoints sin marca son públicos (solo login y confirmación de email)
        ",
        Contact = new OpenApiContact
        {
            Name = "CDI CECAR",
            Email = "cdi@cecar.edu.co"
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // JWT Security Definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"
**Autenticación JWT Token**

1. Primero, obtén tu token usando el endpoint `/api/usuarios/login`
2. Copia el token de la respuesta
3. Haz clic en el botón 'Authorize' 🔓 arriba
4. Pega el token (sin escribir 'Bearer')
5. Haz clic en 'Authorize' y 'Close'

**Formato:** `Bearer {tu_token_aqui}`

**Ejemplo:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

El token expira en 1 hora. Deberás volver a hacer login para obtener uno nuevo.
        ",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    // JWT Security Requirement - Apply to all endpoints by default
    // Endpoints with [AllowAnonymous] will not require authentication
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
            new string[] { }
        }
    });
    
    // Ensure all controllers are included
    c.CustomSchemaIds(type => type.FullName);

});

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SmartBook";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SmartBook";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
    
    // Mejorar el manejo de errores de autenticación
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Log.Warning("Error de autenticación JWT: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var userId = context.Principal?.FindFirst("UserId")?.Value;
            var role = context.Principal?.FindFirst(ClaimTypes.Role)?.Value;
            Log.Information("Token JWT validado - UserId: {UserId}, Rol: {Rol}", userId, role);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Log.Warning("Challenge de autenticación: {Error}", context.Error);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Add Persistence
builder.Services.AddPersistence(builder.Configuration);

// Configure Mapping
MappingConfig.ConfigureMappings();

// Register Application Services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ILibroService, LibroService>();
builder.Services.AddScoped<IIngresoService, IngresoService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
// Swagger siempre disponible para facilitar el desarrollo y pruebas
// Swagger configuration - must be before UseRouting/UseEndpoints
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartBook API v1");
    c.RoutePrefix = "swagger"; // This means Swagger will be at /swagger
    c.DisplayRequestDuration();
    c.EnableTryItOutByDefault();
    c.EnableDeepLinking();
    c.EnableFilter();
    c.ShowExtensions();
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
});

app.UseCors("AllowAll");

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created (for development)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SmartbookDbContext>();
        
        // Asegurar que la base de datos esté creada primero
        // Nota: La base de datos ya está creada con la estructura correcta según el script SQL
        // EnsureCreated() solo creará las tablas si no existen, pero no modificará la estructura existente
        context.Database.EnsureCreated();
        
        // Seed default admin user if it doesn't exist
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();
        var passwordService = services.GetRequiredService<IPasswordService>();
        
        var adminExists = await unitOfWork.Usuarios.GetByEmailAsync("admin@cecar.edu.co");
        if (adminExists == null)
        {
            var admin = new Smartbook.Entidades.Usuario
            {
                Identificacion = "1234567890",
                Nombres = "Admin CDI",
                Email = "admin@cecar.edu.co",
                ContrasenaHash = passwordService.HashPassword("AdminCDI123!"),
                Rol = Smartbook.Entidades.Enums.Rol.Admin,
                EmailConfirmado = true,
                Activo = true
            };
            await unitOfWork.Usuarios.AddAsync(admin);
            await unitOfWork.SaveChangesAsync();
            Log.Information("Default admin user created");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while seeding the database");
    }
}

// Log Swagger URL for easy access
Log.Information("Swagger UI available at: http://localhost:5235/swagger");
Log.Information("Swagger JSON available at: http://localhost:5235/swagger/v1/swagger.json");

app.Run();
