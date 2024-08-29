using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.IO.Abstractions;
using iLabPlus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// Autenticación
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.AccessDeniedPath = new PathString("/Account/Forbidden");
        options.LoginPath = new PathString("/Account/Login");
    });

// Entity Framework Contexts
builder.Services.AddDbContext<DbContextiLabPlus>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("iLabPlusDB")));

//builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
//builder.Services.AddScoped<IUrlHelper>(x => {
//    var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;

//    var factory = x.GetRequiredService<IUrlHelperFactory>();
//    return factory.GetUrlHelper(actionContext);

//});

// Session y demás servicios
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<FunctionsBBDD>();
builder.Services.AddScoped<FunctionsiLabPlus>();
builder.Services.AddScoped<FunctionsMails>();
builder.Services.AddScoped<FunctionsCrypto>();
builder.Services.AddScoped<FunctionsLeyAntiFraude>();
builder.Services.AddSingleton<ThreatOpenAIHanna>();
builder.Services.AddSingleton<IFileSystem, FileSystem>();
builder.Services.AddTransient<Notifications>();



// Configuración de compresión de respuesta
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});


builder.Services.AddHttpClient();

// Configuración de JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });


var app = builder.Build();

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



app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // Llamar a UseAuthentication() antes de UseAuthorization().
app.UseAuthorization();

app.UseSession();               // Habilitar el middleware de sesión
app.UseResponseCompression();   // Habilitar la compresión de respuesta

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
