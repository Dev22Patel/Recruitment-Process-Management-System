using Hangfire;
using Hangfire.Dashboard;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using Recruitment_Process_Management_System.Data;
using Recruitment_Process_Management_System.Extensions;
using Recruitment_Process_Management_System.Services.Consumers;
using Recruitment_Process_Management_System.Services.RabbitMq;
using System.Text;
using Hangfire.Dashboard;


var builder = WebApplication.CreateBuilder(args);

var rabbitMqConfig = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqConfig>();


builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    return new ConnectionFactory
    {
        HostName = rabbitMqConfig.Host,
        Port = rabbitMqConfig.Port,
        UserName = rabbitMqConfig.Username,
        Password = rabbitMqConfig.Password,
        VirtualHost = "/"
    };
});

builder.Services.AddMassTransit(x =>
{
    // Register the email consumer
    x.AddConsumer<SendEmailConsumer>();

    // Configure RabbitMQ transport
    x.UsingRabbitMq((context, cfg) =>
    {
        // Use URI-based host configuration with port
        cfg.Host($"rabbitmq://{rabbitMqConfig.Host}:{rabbitMqConfig.Port}", h =>
        {
            h.Username(rabbitMqConfig.Username);
            h.Password(rabbitMqConfig.Password);
        });

        // Configure endpoint for consuming emails
        cfg.ReceiveEndpoint("email-queue", e =>
        {
            e.ConfigureConsumer<SendEmailConsumer>(context);

            // Retry policy: retry 3 times with incremental method
            e.UseMessageRetry(r =>
            {
                r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(8));
            });

            // Concurrency limit
            e.ConcurrentMessageLimit = 10;
        });
    });
});


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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();

}

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});



app.UseHttpsRedirection();

// Use CORS before Authentication
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();



public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // In production, add kari devanu proper authorization
        // haman mate , allow access only in Development
        return true; // Change this to check for Admin role in production
    }
}