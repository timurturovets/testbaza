global using System;
global using System.Threading.Tasks;
global using System.Collections.Generic;

global using TestBaza.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using TestBaza.Data;
using TestBaza.Repositories;

var builder = WebApplication.CreateBuilder(args);

IServiceCollection services = builder.Services;
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
    services.AddDatabaseDeveloperPageExceptionFilter();

    services.AddDefaultIdentity<User>(options => {
        //Валидация всего этого дела происходит непосредственно через ModelState.IsValid и атрибуты валидации в модели
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 0;
        options.Password.RequiredUniqueChars = 0;
        options.User.AllowedUserNameCharacters += "абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮИЯ";
        options.SignIn.RequireConfirmedAccount = false;
    })
        .AddEntityFrameworkStores<AppDbContext>();

    services.AddControllersWithViews();

    services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/auth/login";
    });

    services.AddTransient<ITestsRepository, TestsRepository>();
}
WebApplication app = builder.Build();

{
    if (app.Environment.IsDevelopment()) app.UseMigrationsEndPoint();
    else app.UseHsts();
    

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=auth}/{action=reg}");
}

app.Run();
