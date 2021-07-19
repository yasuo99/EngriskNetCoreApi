using System;
using System.Text;
using Engrisk.Data;
using Domain.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Application.Helper;
using Application.Services;
using Engrisk.Services;
using Engrisk.Middleware;
using System.Threading.Tasks;
using Application.Hubs;
using MediatR;
using Application.Mediator.Accounts;

namespace Engrisk
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // public void ConfigureDevelopmentServices(IServiceCollection services)
        // {
        //     services.AddDbContextPool<ApplicationDbContext>(options => options.UseLazyLoadingProxies().UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        //     ConfigureServices(services);
        // }
        // public void ConfigureProductionServices(IServiceCollection services)
        // {
        //     services.AddDbContextPool<ApplicationDbContext>(options => options.UseLazyLoadingProxies().UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        //     ConfigureServices(services);
        // }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddEntityFrameworkSqlServer();
            IdentityBuilder identityBuider = services.AddIdentityCore<Account>(opts =>
            {
                opts.Password.RequireDigit = false;
                opts.Password.RequiredLength = 8;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireUppercase = false;
            });
            identityBuider = new IdentityBuilder(identityBuider.UserType, typeof(Role), identityBuider.Services);
            identityBuider.AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
            identityBuider.AddRoleValidator<RoleValidator<Role>>();
            identityBuider.AddRoleManager<RoleManager<Role>>();
            identityBuider.AddSignInManager<SignInManager<Account>>();
            // services.AddIdentity<Account,Role>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:TokenSecret").Value)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
                options.SaveToken = true;
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && ((path.StartsWithSegments("/notification")) || path.StartsWithSegments("/message") || path.StartsWithSegments("/tournament")))
                        {
                            context.Token = accessToken;
                            System.Console.WriteLine(accessToken);
                        }
                        return Task.CompletedTask;
                    }
                };
            }
            );
            // services.AddAuthorization(opts => {
            //     opts.
            //     opts.AddPolicy("RequireSuperadminRole", policy => policy.RequireRole("SUPERADMIN"));
            //     opts.AddPolicy("RequireManagerRole", policy => policy.RequireRole("MANAGER"));
            //     opts.AddPolicy("RequireForumRole", policy => policy.RequireRole("forumadmin","forummod"));
            // });
            services.AddDbContextPool<ApplicationDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")).EnableSensitiveDataLogging();
                options.UseInternalServiceProvider(serviceProvider);
            });
            services.AddControllers(opts =>
            {
                // var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                // opts.Filters.Add(new AuthorizeFilter(policy));
            })
            .AddNewtonsoftJson(options =>
            {
                options.UseCamelCasing(false);
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
            services.ConfigurationServices();
            services.AddScoped<IAuthRepo, AuthRepo>();
            services.AddScoped<ICRUDRepo, CRUDRepo>();
            services.AddScoped<IAuthService, GoogleAuthService>();
            services.AddHttpClient<IAuthService, FacebookAuthService>();
            services.AddScoped<IAuthService, FacebookAuthService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IUserService, UserService>();
            
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));
            services.Configure<GoogleAuthConfig>(Configuration.GetSection("Google"));
            services.Configure<FacebookAuthConfig>(Configuration.GetSection("Facebook"));
            services.Configure<DropBoxSettings>(Configuration.GetSection("Dropbox"));
            services.Configure<AnonymousSettings>(Configuration.GetSection("AnonymousSettings"));
            services.AddTransient<GoogleAuthService>();
            services.AddTransient<FacebookAuthService>();
            services.AddTransient<Func<ServiceEnum, IAuthService>>(serviceProvider => key =>
            {
                switch (key)
                {
                    case ServiceEnum.Google:
                        return serviceProvider.GetService<GoogleAuthService>();
                    case ServiceEnum.Facebook:
                        return serviceProvider.GetService<FacebookAuthService>();
                    default:
                        return null;
                }
            });
            services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
            services.AddCors();
            services.AddSignalR();
            services.AddMediatR(typeof(List.Handler).Assembly);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseMiddleware<ExceptionMiddleware>();
            }
            // app.UseHttpsRedirection();
            app.UseCors(options =>
            {
                options.WithOrigins("http://localhost:3000").AllowCredentials().AllowAnyMethod().AllowAnyHeader();
            });
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<NotificationHub>("/notification");
                endpoints.MapHub<MessageHub>("/notification/signalr");
                endpoints.MapHub<ExamHub>("/tournament");
                endpoints.MapControllers();
            });
        }
    }
}
