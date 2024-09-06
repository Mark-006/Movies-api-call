using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MvcMovie.Data;
using MvcMovie.Services;

var builder = WebApplication.CreateBuilder(args);


// Force the environment to Development
builder.Environment.EnvironmentName = "Development";

// Get the IWebHostEnvironment instance
var environment = builder.Environment;

// Register IWebHostEnvironment as a singleton
builder.Services.AddSingleton(environment);

// Register HttpClient
builder.Services.AddHttpClient();


// Register MoviesDbContext with SQL Server configuration
builder.Services.AddDbContext<MoviesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MoviesDatabase")));

// Register MovieService for dependency injection
builder.Services.AddScoped<MovieService>();

// Add authorization services
builder.Services.AddAuthorization();

// Add controllers with views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Use authorization middleware
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
