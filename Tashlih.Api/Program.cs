using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;
using System.Text.Json.Serialization;
using Tashlih.Api.Hubs;
using Tashlih.Api.Services;
using Tashlih.Application.Interfaces;
using Tashlih.Infrastructure.Models;
using Tashlih.Infrastructure.Services;

namespace Tashlih.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //builder.WebHost.ConfigureKestrel(options =>
            //{
            //    options.ListenAnyIP(7281, listenOptions =>
            //    {
            //        listenOptions.UseHttps();
            //    });
            //});

            builder.Services.AddControllers()
                             .AddJsonOptions(options =>
                             {
                                 options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                             });

            builder.Services.AddEndpointsApiExplorer();

            #region Swagger With JWT
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Tashlih API",
                    Version = "v1",
                    Description = "API for Tashlih Auto Parts Platform"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your token:"
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
                        Array.Empty<string>()
                    }
                });
            });
            #endregion

            #region JWT Authentication
            var jwtSecret = builder.Configuration["Jwt:Secret"]!;
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];

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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ClockSkew = TimeSpan.Zero
                };

                // ✅ أضف ده للـ SignalR
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();
            #endregion

            #region Database & Services
            builder.Services.AddDbContext<TashlihContext>(options =>
                       options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
            //var connectionString = "Server=localhost;Database=Tashlih;User Id=sa;Password=Rekj@10170;TrustServerCertificate=True;";

            //builder.Services.AddDbContext<TashlihContext>(options =>
            //    options.UseSqlServer(connectionString));
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IPartsService, PartsService>();
            builder.Services.AddScoped<ILookupsService, LookupsService>();
            builder.Services.AddScoped<ICustomerProfileService, CustomerProfileService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<ISuppliersService, SuppliersService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ISupplierProfileService, SupplierProfileService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IOtpService, OtpService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IFirebasePushService, FirebasePushService>();


            #endregion

            #region SignalR Services
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .SetIsOriginAllowed(_ => true)  // يسمح لأي domain
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();  // ✅ مهم للـ SignalR
                });
            });

           

           

           
            #endregion
            // SignalR
            builder.Services.AddSignalR();
            builder.Services.AddScoped<IChatHubService, ChatHubService>();
            builder.Services.AddScoped<IOrderHubService, OrderHubService>();
            var app = builder.Build();
            app.UseCors("AllowAll");  // CORS 

            app.UseSwagger();
            app.UseSwaggerUI();

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // SignalR Hub
            app.MapHub<ChatHub>("/chathub");

            app.Run();
        }
    }
}