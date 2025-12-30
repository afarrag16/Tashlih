using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Tashlih.Api.BackgroundServices;
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
            builder.Services.AddScoped<IReviewsService, ReviewsService>();
            builder.Services.AddScoped<IFavoritesService, FavoritesService>();
            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
            builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
            builder.Services.AddScoped<SubscriptionNotificationService>();
            builder.Services.AddHostedService<SubscriptionExpirationJob>();
            builder.Services.AddScoped<AdminSupplierService>();
            builder.Services.AddScoped<AdminCustomerService>();
            builder.Services.AddScoped<AdminDashboardService>();

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
           
            #region Rate Limiting
            // Rate Limiting
            builder.Services.AddRateLimiter(options =>
            {
                // 1️⃣ الحد العام - 100 طلب/دقيقة
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // 2️⃣ حد تسجيل الدخول - 5 محاولات / 15 دقيقة
                options.AddFixedWindowLimiter("login", opt =>
                {
                    opt.PermitLimit = 5;
                    opt.Window = TimeSpan.FromMinutes(15);
                    opt.AutoReplenishment = true;
                });

                // 3️⃣ حد إرسال OTP - 3 مرات / 10 دقائق
                options.AddFixedWindowLimiter("otp", opt =>
                {
                    opt.PermitLimit = 3;
                    opt.Window = TimeSpan.FromMinutes(10);
                    opt.AutoReplenishment = true;
                });

                // رسالة عند تجاوز الحد
                options.RejectionStatusCode = 429;
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsync(
                        "{\"success\":false,\"message\":\"Too many requests\",\"messageAr\":\"طلبات كثيرة، حاول لاحقاً\"}", token);
                };
            });
            #endregion

          

            var app = builder.Build();

            #region  Security Headers
            // Security Headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
                await next();
            });

            #endregion

            app.UseCors("AllowAll");  // CORS 

            app.UseSwagger();
            app.UseSwaggerUI();

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseRateLimiter();
            app.UseAuthorization();
            

            app.MapControllers();

            // SignalR Hub
            app.MapHub<ChatHub>("/chathub");

            app.Run();
        }
    }
}