using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rocky_DataAccess.Data;
using Rocky_DataAccess.Initializer;
using Rocky_DataAccess.Repository;
using Rocky_Utility;
using Rocky_Utility.BrainTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky
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
            
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                Configuration.GetConnectionString("DefaultConnection")
                ));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddDefaultUI()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddTransient<IEmailSender, EmailSender>();

            services.AddDistributedMemoryCache();
            services.AddHttpContextAccessor();
            services.AddSession(Options =>
            {
                Options.IdleTimeout = TimeSpan.FromMinutes(10);
                Options.Cookie.HttpOnly = true;
                Options.Cookie.IsEssential = true;
            }
            );

            services.Configure<BrainTreeSettings>(Configuration.GetSection("BrainTree"));
            services.AddSingleton<IBrainTreeGate, BrainTreeGate>();

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IApplicationTypeRepository, ApplicationTypeRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();

            services.AddScoped<IInquiryHeaderRepository, InquiryHeaderRepository>();
            services.AddScoped<IInquiryDetailRepository, InquiryDetailRepository>();
            services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();

            services.AddScoped<IOrderHeaderRepository, OrderHeaderRepository>();
            services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();

            services.AddScoped<IDbInitializer, DbInitializer>();

            services.AddAuthentication().AddFacebook(Options =>
            {
                Options.AppId = "572356303938130";
                Options.AppSecret = "0a3949688c5dadbeb81989a3ab30a664";
            });

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbInitializer dbInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            dbInitializer.Initialize();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
