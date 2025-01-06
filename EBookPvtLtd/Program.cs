using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EBookPvtLtd.Data;
using EBookPvtLtd.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Configuration;
using Microsoft.Extensions.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("EBookPvtLtdContextConnection") ?? throw new InvalidOperationException("Connection string 'EBookPvtLtdContextConnection' not found.");

builder.Services.AddDbContext<EBookPvtLtdContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<Users>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<EBookPvtLtdContext>();

builder.Services.AddTransient<IEmailSender, EmailSender>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
