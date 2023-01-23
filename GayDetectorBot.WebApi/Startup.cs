using GayDetectorBot.WebApi.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GayDetectorBot.WebApi;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        //var dbConnectionString = Configuration.GetConnectionString("DefaultConnection");
        //services.AddDbContext<QuaderMainContext>(builder => builder.UseNpgsql(dbConnectionString));

        //var appSettingsSection = Configuration.GetSection("AppSettings");
        //services.Configure<AppSettings>(appSettingsSection);

        //var key = Encoding.ASCII.GetBytes(appSettingsSection.GetValue<string>("Secret"));

        /*services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = AppVersionInfo.AppName + " " + AppVersionInfo.BuildVersion,
                    ValidAudience = "Quader"
                };

                opt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;
                        // Allow SignalR access
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ws/testHub"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });*/

        services.AddLogging(builder =>
        {
            var loggingSection = Configuration.GetSection("Logging");
            builder.AddFile(loggingSection, options =>
            {
                options.FormatLogFileName = fileName =>
                {
                    return string.Format(fileName, DateTime.UtcNow);
                };
            });
        });

        //services.AddControllers().AddNewtonsoftJson();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();

        services.AddCors(options =>
        {
            options.AddPolicy("GayDetector Policy", builder =>
            {
                builder.WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        services.AddSwaggerGen();

        services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
            options.HttpOnly = HttpOnlyPolicy.Always;
        });

        /*services.AddSpaStaticFiles(options =>
        {
            options.RootPath = "../../Swagger.Web.Frontend/dist/quader.web.frontend";
        });*/

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        /*services.AddScoped<ITokenGeneratorService, TokenGeneratorService>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();*/
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseCors("GayDetector Policy");

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Add("X-Xss-Protection", "1"); // XSS Protection
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff"); // No Sniff Protection
            context.Response.Headers.Add("X-Frame-Options", "DENY"); // Deny Frame Protection
            await next();
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapControllerRoute("default", "{controller}/{action=Index}/{id?}");
        });

        /*app.UseSpa(builder =>
        {
            builder.Options.SourcePath = "../../Quader.Web.Frontend";

            builder.UseProxyToSpaDevelopmentServer("http://localhost:4200");
        });*/
    }
}