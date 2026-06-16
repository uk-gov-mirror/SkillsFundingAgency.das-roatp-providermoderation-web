using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Roatp.ProviderModeration.Application.Providers.Queries.GetProvider;
using SFA.DAS.Roatp.ProviderModeration.Web.AppStart;
using SFA.DAS.Roatp.ProviderModeration.Web.Validators;

namespace SFA.DAS.Roatp.ProviderModeration.Web;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddConfigFromAzureTableStorage();

        builder.Services.Configure<RouteOptions>(o => o.LowercaseUrls = true);

        builder.Services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

        builder.Services.RegisterConfigurations(builder.Configuration);

        builder.Services
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(GetProviderQuery).Assembly))
            .AddAuthentication(builder.Configuration)
            .AddServiceRegistrations(builder.Configuration);

        builder.Services.AddOpenTelemetryRegistration(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]!);

        builder.Services.AddValidatorsFromAssembly(typeof(ProviderDescriptionReviewModelValidator).Assembly);

        builder.Services.AddHealthChecks();
        builder.Services.AddDataProtection(builder.Configuration, builder.Environment);

        //Add services above
        var app = builder.Build();
        //Add pipeline below

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.Use(async (context, next) =>
        {
            if (context.Response.Headers.ContainsKey("X-Frame-Options"))
            {
                context.Response.Headers.Remove("X-Frame-Options");
            }

            context.Response.Headers!.Append("X-Frame-Options", "SAMEORIGIN");

            await next();

            if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
            {
                //Re-execute the request so the user gets the error page
                var originalPath = context.Request.Path.Value;
                context.Items["originalPath"] = originalPath;
                context.Request.Path = "/error/404";
                await next();
            }
        });


        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseCookiePolicy();


        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHealthChecks();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
