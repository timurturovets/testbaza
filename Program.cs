global using System;
global using System.Threading.Tasks;
global using System.Collections.Generic;

global using TestBaza.Models.RegularModels;

using Microsoft.EntityFrameworkCore;

using TestBaza.Data;
using TestBaza.Factories;
using TestBaza.Extensions;
using TestBaza.Repositories.Rates;
using TestBaza.Repositories.Tests;
using TestBaza.Repositories.Questions;
using TestBaza.Repositories.CheckInfos;
using TestBaza.Repositories.PassingInfos;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
    services.AddDatabaseDeveloperPageExceptionFilter();

    services.AddDefaultIdentity<User>(options => {
        // Валидация всего этого дела происходит непосредственно через ModelState.IsValid и атрибуты валидации в модели
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
    services.AddTransient<IPassingInfoRepository, PassingInfoRepository>();
    services.AddTransient<IChecksInfoRepository, ChecksInfoRepository>();

    services.AddTransient<IUserFactory, UserFactory>();
    services.AddTransient<ITestFactory, TestFactory>();
    services.AddTransient<IQuestionFactory, QuestionFactory>();
    services.AddTransient<IPassingInfoFactory, PassingInfoFactory>();

    services.AddSession();
}

var app = builder.Build();
{
    if (app.Environment.IsDevelopment()) app.UseMigrationsEndPoint();
    else app.UseHsts();
    
    app.UseSession()
       .UseDatabaseUpdateRequestsHandler()
       .UseErrorStatusCodesHandler()


       .UseHttpsRedirection()
       .UseStaticFiles()
       .UseRouting()
    
       .UseAuthentication()
       .UseAuthorization()

       .UseUserPresenceHandler()

       .ToEndpointRouteBuilder().MapControllerRoute(
        "default", 
            "{controller=auth}/{action=reg}");
}
app.Run();