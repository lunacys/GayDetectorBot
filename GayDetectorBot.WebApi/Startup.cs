﻿using GayDetectorBot.WebApi.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GayDetectorBot.WebApi.Configuration;
using GayDetectorBot.WebApi.Controllers;
using GayDetectorBot.WebApi.Data;
using GayDetectorBot.WebApi.Services.Auth;
using GayDetectorBot.WebApi.Services.UserManagement;

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
        var dbConnectionString = Configuration.GetConnectionString("DefaultConnection");
        var sqliteConnectionString = Configuration.GetConnectionString("SqliteConnection");

        if (string.IsNullOrEmpty(dbConnectionString))
        {
            throw new ArgumentException("No DB connection string provided", nameof(dbConnectionString));
        }

        /*if (!string.IsNullOrEmpty(sqliteConnectionString))
        {
            services.AddDbContext<GayDetectorBotContext>(builder => builder.UseSqlite(sqliteConnectionString));
        }
        else*/
        {
            services.AddNpgsqlDataSource(dbConnectionString);
            services.AddDbContext<GayDetectorBotContext>(builder =>
            {
                //builder.UseNpgsql(dbConnectionString);
                builder.UseNpgsql();
            }, ServiceLifetime.Singleton);
        }

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var appSettingsSection = Configuration.GetSection("AppSettings");
        services.Configure<AppSettings>(appSettingsSection);

        var tgSection = Configuration.GetSection("Telegram");
        services.Configure<TelegramOptions>(tgSection);

        var key = Encoding.ASCII.GetBytes(appSettingsSection.GetValue<string>("Secret")!);

        services.AddAuthentication(options =>
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
                    ValidAudience = "GayDetectorBot"
                };
            });

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

        services.AddHealthChecks().AddNpgSql();

        services.AddControllers().AddNewtonsoftJson();
        services.AddAuthorization();

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

        services.ConfigureGayDetector(tgSection.Get<TelegramOptions>()!);
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

        //app.UseHttpsRedirection();

        app.UseRouting();
        app.UseCors("GayDetector Policy");

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Xss-Protection", "1"); // XSS Protection
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff"); // No Sniff Protection
            context.Response.Headers.Append("X-Frame-Options", "DENY"); // Deny Frame Protection
            await next();
        });

        app.UseAuthentication();
        app.UseAuthorization();

        var cfg = Configuration.GetSection("Telegram").Get<TelegramOptions>();

        app.UseEndpoints(endpoints =>
        {
            if (cfg!.UseWebhooks)
                endpoints.MapBotWebhookRoute<BotController>(cfg!.Route);
            else
                endpoints.MapControllerRoute("default", "{controller}/{action=Index}/{id?}");
            endpoints.MapControllers();
        });

        /*app.UseSpa(builder =>
        {
            builder.Options.SourcePath = "../../Quader.Web.Frontend";

            builder.UseProxyToSpaDevelopmentServer("http://localhost:4200");
        });*/
    }
}