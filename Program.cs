using System.Data;
using Npgsql;
using Microsoft.AspNetCore.Authentication.Cookies;

// Zapne automatické mapování snake_case (category_id) na PascalCase (CategoryId)
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

// Přidá podporu pro antiforgery tokeny v hlavičkách (potřebné pro HTMX/AJAX)
builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddScoped(_ => new Supabase.Client(
    builder.Configuration["Supabase:Url"]!,
    builder.Configuration["Supabase:Key"]!,
    new Supabase.SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = true
    }
));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();