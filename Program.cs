global using System;
global using System.Threading.Tasks;
global using System.Collections.Generic;

global using TestBaza.Models;
global using static TestBaza.Constants;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using TestBaza.Data;
using TestBaza.Factories;
using TestBaza.Extensions;
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
        options.AccessDeniedPath = "/home/forbidpage";
        options.LogoutPath = "/auth/login";
    });

    services.AddTransient<ITestsRepository, TestsRepository>();
    services.AddTransient<IQuestionsRepository, QuestionsRepository>();
    services.AddTransient<IRatesRepository, RatesRepository>();

    services.AddTransient<IUserFactory, UserFactory>();
    services.AddTransient<ITestFactory, TestFactory>();
    services.AddTransient<IQuestionFactory, QuestionFactory>();
    services.AddTransient<IResponseFactory, ResponseFactory>();

    services.AddSession();
}

WebApplication app = builder.Build();
{
    if (app.Environment.IsDevelopment()) app.UseMigrationsEndPoint();
    else app.UseHsts();

    app.UseSession();

    app.ClearSession();
    app.UseErrorStatusCodesHandler();
    app.UseApiKeysHandler();

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