using Microsoft.EntityFrameworkCore;
using Petroleum_Materials_Transport_Office_System.Data;
using Petroleum_Materials_Transport_Office_System.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONNECT DATABASE ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
// ---------------------------

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddSingleton<ActionLogger>();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/Login");
        return;
    }
    await next();
});

app.MapRazorPages();

app.Run();