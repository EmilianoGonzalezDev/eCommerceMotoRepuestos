using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using eCommerceMotoRepuestos.Context;
using eCommerceMotoRepuestos.Repositories;
using eCommerceMotoRepuestos.Services;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddLocalization();

var supportedCulture = new CultureInfo("es-AR");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(supportedCulture);
    options.DefaultRequestCulture.Culture.NumberFormat.CurrencyDecimalDigits = 0;
    options.SupportedCultures = [supportedCulture];
    options.SupportedUICultures = [supportedCulture];
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlString"));    
});

builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<CartRepository>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<UserOrderService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(30); });

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(
    options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

        options.AccessDeniedPath = "/Home/Error";
    }
    );

var app = builder.Build();

app.UseSession();
app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
