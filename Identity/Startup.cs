using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.Data;
using Identity.Domain.Services;
using Identity.Model;
using Identity.Security.Token;
using Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Identity
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<AppIdentityContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("IdentityCon"));
            });

            services.AddIdentity<AppUser, AppRole>(opts =>

            {
                opts.User.RequireUniqueEmail = true;
                opts.User.AllowedUserNameCharacters = "1234567890*-qwertyuýopðüasdfghjklþi,<zxcvbnmöç.QWERTYUIOPÐÜASDFGHJKLÞÝ,<ZXCVBNMÖÇ.";
                opts.Password.RequiredLength = 4;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = false;

            }).AddEntityFrameworkStores<AppIdentityContext>();

            //service containerýna tokenoptions eklendi böylece applicationda her yerde ctor injection ile bu deðerlere ulaþýlabilecek
            services.Configure<AppTokenOptions>(Configuration.GetSection("TokenOptions"));
            var tokenOptions = Configuration.GetSection("TokenOptions").Get<AppTokenOptions>();

            #region JWT
            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //identity ile token sisteminin eþleþmesi için bu arkadaþ önemli tam olarak bakacaðým ne iþe yaradýðýna
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    //bu bilgiler token ile gelecek her requestte kontrol edilecek
                    ValidateAudience = true, //dinleyiciyi doðrula tokený gönderen doðrumu ayarlardaki ile
                    ValidateIssuer = true, //token içinde gelen issuer bilgisini valide et ayarlardaki ile aynýmý
                    ValidateLifetime = true,//token expirationý kontrol et
                    ValidateIssuerSigningKey = true,
                    ValidAudience = tokenOptions.Audience,//geçerli dinleyici
                    ValidIssuer = tokenOptions.Issuer,//geçerli issuer
                    IssuerSigningKey = SignHandler.GetSecurityKey(tokenOptions.SecurityKey),//verify signature gerekli tipe dönüþtürülerek token optionsa deðeri atanýr ver validasyon bu keye göre yapýlýr
                    ClockSkew = TimeSpan.Zero//farklý saat dilimlerinde token ömrünü uzatmak için kullanýlýr
                };
            });

            ///Default olarak bearer eklendikten sonra farklý tipl kullanýcýlar için farklý þemadaki tokenlar oluþturulup kullanýlabilir
            //services.AddAuthentication(opts =>
            //{
            //    opts.DefaultAuthenticateScheme = "CustomSchema";
            //    opts.DefaultChallengeScheme = "CustomSchema";
            //    //identity ile token sisteminin eþleþmesi için tam olarak bakacaðým ne iþe yaradýðýna
            //}).AddJwtBearer("CustomSchema", jwtBearerOptions =>
            //{
            //    jwtBearerOptions.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
            //    {
            //        //bu bilgiler token ile gelecek her requestte kontrol edilecek
            //        ValidateAudience = true, //dinleyiciyi doðrula tokený gönderen doðrumu ayarlardaki ile
            //        ValidateIssuer = true, //token içinde gelen issuer bilgisini valide et ayarlardaki ile aynýmý
            //        ValidateLifetime = true,//token expirationý kontrol et
            //        ValidateIssuerSigningKey = true,
            //        ValidAudience = tokenOptions.Audience,//geçerli dinleyici
            //        ValidIssuer = tokenOptions.Issuer,//geçerli issuer
            //        IssuerSigningKey = SignHandler.GetSecurityKey(tokenOptions.SecurityKey),//verify signature gerekli tipe dönüþtürülerek token optionsa deðeri atanýr ver validasyon bu keye göre yapýlýr
            //        ClockSkew = TimeSpan.Zero//farklý saat dilimlerinde token ömrünü uzatmak için kullanýlýr
            //    };
            //});
            #endregion
            #region Cors
            services.AddCors(opts =>
            {
                opts.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });
            #endregion

            services.AddScoped<ITokenHandler, TokenHandler>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserService, UserSerivce>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
