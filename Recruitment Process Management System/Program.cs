using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Extensions;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Recruitment API", Version = "v1" });

        // Custom Schema ID generation to avoid conflicts
        c.CustomSchemaIds(type =>
        {
            if (type.Namespace != null && type.Namespace.Contains("DTOs"))
                return type.Name + "Dto";
            if (type.Namespace != null && type.Namespace.Contains("Entities"))
                return type.Name + "Entity";
            return type.Name;
        });
    });

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new ConnectionFactory
    {
        HostName = configuration["RabbitMQ:HostName"],
        UserName = configuration["RabbitMQ:UserName"],
        Password = configuration["RabbitMQ:Password"],
        Port = int.Parse(configuration["RabbitMQ:Port"]),
        VirtualHost = configuration["RabbitMQ:VirtualHost"],
        AutomaticRecoveryEnabled = true,
        NetworkRecoveryInterval = TimeSpan.FromSeconds(int.Parse(configuration["RabbitMQ:NetworkRecoveryIntervalSeconds"])),
        RequestedConnectionTimeout = TimeSpan.FromSeconds(int.Parse(configuration["RabbitMQ:RequestedConnectionTimeoutSeconds"])),
        SocketReadTimeout = TimeSpan.FromSeconds(int.Parse(configuration["RabbitMQ:SocketReadTimeoutSeconds"])),
        SocketWriteTimeout = TimeSpan.FromSeconds(int.Parse(configuration["RabbitMQ:SocketWriteTimeoutSeconds"])),
        ContinuationTimeout = TimeSpan.FromSeconds(int.Parse(configuration["RabbitMQ:ContinuationTimeoutSeconds"])),
        TopologyRecoveryEnabled = true,
        ClientProvidedName = "RecruitmentProcessManagementSystem"
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();

}



app.UseHttpsRedirection();

// Use CORS before Authentication
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();