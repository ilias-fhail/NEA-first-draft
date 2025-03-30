using APIcalls;
using BlazorApp1.Components;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using StockProSim.Data;
using RiskCalculations;
using Infragistics;
using IgniteUI.Blazor.Controls;
using static APIcalls.AlphaVantage;


namespace BlazorApp1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<MyServerDb>();
            builder.Services.AddScoped(provider =>new APICalls());
            builder.Services.AddScoped<RiskCalculator>();
            builder.Services.AddSingleton<AuthenticationService>();
            builder.Services.AddBlazorBootstrap();
            builder.Services.AddScoped<StockNewsService>();
            builder.Services.AddScoped<NavigationService>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
               .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
