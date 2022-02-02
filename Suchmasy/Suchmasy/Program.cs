using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Suchmasy;
using Suchmasy.Data;
using Suchmasy.Repos;
using Suchmasy.Repos.Contracts;

var builder = WebApplication.CreateBuilder(args);

// docker command to up sql server container: 
// docker run -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=ASDasd123' -p 1401:1433 -d mcr.microsoft.com/mssql/server

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("ContainerConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredUniqueChars = 1;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
}).AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddTransient<IRequestRepository, RequestRepository>();

const string SUPPLIER_RELATIONSHIP = "supplier_relationship";
const string MANAGING_REQUESTS = "managing_requests";
const string CREATE_REQUESTS = "create_requests";

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(SUPPLIER_RELATIONSHIP, policy =>
                                        policy.RequireAuthenticatedUser()
                                              .RequireRole("admin", "buyer"));

    options.AddPolicy(MANAGING_REQUESTS, policy =>
                                      policy.RequireAuthenticatedUser()
                                            .RequireRole("admin", "requester", "buyer"));

    options.AddPolicy(CREATE_REQUESTS, policy =>
                                      policy.RequireAuthenticatedUser()
                                            .RequireRole("admin", "requester"));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Suppliers", SUPPLIER_RELATIONSHIP);
    options.Conventions.AuthorizePage("/Requests", MANAGING_REQUESTS);
    options.Conventions.AuthorizePage("/CreateRequest", MANAGING_REQUESTS);
    options.Conventions.AuthorizePage("/Orders", MANAGING_REQUESTS);
    options.Conventions.AuthorizePage("/CreateOrder", SUPPLIER_RELATIONSHIP);
    //options.Conventions.AuthorizePage("/Contact");
    //options.Conventions.AllowAnonymousToPage("/Private/PublicPage");
    //options.Conventions.AllowAnonymousToFolder("/Private/PublicPages");
});

var app = builder.Build();

// SEED initial data
new InitialData(app.Services).Seed();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
