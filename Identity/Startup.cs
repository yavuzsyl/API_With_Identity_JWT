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
                opts.User.AllowedUserNameCharacters = "1234567890*-qwertyu�op��asdfghjkl�i,<zxcvbnm��.QWERTYUIOP��ASDFGHJKL��,<ZXCVBNM��.";
                opts.Password.RequiredLength = 4;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = false;

            }).AddEntityFrameworkStores<AppIdentityContext>();

            //service container�na tokenoptions eklendi b�ylece applicationda her yerde ctor injection ile bu de�erlere ula��labilecek
            services.Configure<AppTokenOptions>(Configuration.GetSection("TokenOptions"));
            var tokenOptions = Configuration.GetSection("TokenOptions").Get<AppTokenOptions>();

            #region JWT
            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //identity ile token sisteminin e�le�mesi i�in bu arkada� �nemli tam olarak bakaca��m ne i�e yarad���na
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    //bu bilgiler token ile gelecek her requestte kontrol edilecek
                    ValidateAudience = true, //dinleyiciyi do�rula token� g�nderen do�rumu ayarlardaki ile
                    ValidateIssuer = true, //token i�inde gelen issuer bilgisini valide et ayarlardaki ile ayn�m�
                    ValidateLifetime = true,//token expiration� kontrol et
                    ValidateIssuerSigningKey = true,
                    ValidAudience = tokenOptions.Audience,//ge�erli dinleyici
                    ValidIssuer = tokenOptions.Issuer,//ge�erli issuer
                    IssuerSigningKey = SignHandler.GetSecurityKey(tokenOptions.SecurityKey),//verify signature gerekli tipe d�n��t�r�lerek token optionsa de�eri atan�r ver validasyon bu keye g�re yap�l�r
                    ClockSkew = TimeSpan.Zero//farkl� saat dilimlerinde token �mr�n� uzatmak i�in kullan�l�r
                };
            });

            ///Default olarak bearer eklendikten sonra farkl� tipl kullan�c�lar i�in farkl� �emadaki tokenlar olu�turulup kullan�labilir
            //services.AddAuthentication(opts =>
            //{
            //    opts.DefaultAuthenticateScheme = "CustomSchema";
            //    opts.DefaultChallengeScheme = "CustomSchema";
            //    //identity ile token sisteminin e�le�mesi i�in tam olarak bakaca��m ne i�e yarad���na
            //}).AddJwtBearer("CustomSchema", jwtBearerOptions =>
            //{
            //    jwtBearerOptions.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
            //    {
            //        //bu bilgiler token ile gelecek her requestte kontrol edilecek
            //        ValidateAudience = true, //dinleyiciyi do�rula token� g�nderen do�rumu ayarlardaki ile
            //        ValidateIssuer = true, //token i�inde gelen issuer bilgisini valide et ayarlardaki ile ayn�m�
            //        ValidateLifetime = true,//token expiration� kontrol et
            //        ValidateIssuerSigningKey = true,
            //        ValidAudience = tokenOptions.Audience,//ge�erli dinleyici
            //        ValidIssuer = tokenOptions.Issuer,//ge�erli issuer
            //        IssuerSigningKey = SignHandler.GetSecurityKey(tokenOptions.SecurityKey),//verify signature gerekli tipe d�n��t�r�lerek token optionsa de�eri atan�r ver validasyon bu keye g�re yap�l�r
            //        ClockSkew = TimeSpan.Zero//farkl� saat dilimlerinde token �mr�n� uzatmak i�in kullan�l�r
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
