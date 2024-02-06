using AspBlog;
using AspBlog.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)); //(AddScope() type)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
//vymazání složky Areas (byl tam výchozí pohled pro vykresl.identify pohledù)
//úprava stávající metody "AddDefaultIdentity()"
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})  
    .AddEntityFrameworkStores<ApplicationDbContext>();
/*
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();*/
builder.Services.AddControllersWithViews();
//pøidání metody "ConfigureApplicationCookie()" pro nastavení návrat.adresy
builder.Services.ConfigureApplicationCookie(options =>
{   //adresa pro pøesmìrování,pøi pokusu vytv./edit admin akcí(Vytvoøit,Editovat)
    options.LoginPath = "/account/login";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
//deaktivace "MapRazorPages()"-tuto metodu využívají výchozí pohledy autentizace
//app.MapRazorPages();

//vytvoøení vlastního rámce(scope) (pro administrátora)
using (IServiceScope scope = app.Services.CreateScope())//"CreateScope()"-vytvoø.rámce
{   //admin=IdentityRole; user=IdentityUser
    RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    UserManager<IdentityUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    IdentityUser? defaultAdminUser = await userManager.FindByEmailAsync("admin@admin.cz");//"michalh18@seznam.cz"//vyhled.usera podle emailu,kterému chceme pøiø.roli admina

    if (!await roleManager.RoleExistsAsync(UserRoles.Admin))//jestli admin neexistuje,vytvoøí ho
        await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
    //zkusí pøiøadit roli admina,pokud ji ještì nemá
    if (defaultAdminUser is not null && !await userManager.IsInRoleAsync(defaultAdminUser, UserRoles.Admin))
        await userManager.AddToRoleAsync(defaultAdminUser, UserRoles.Admin);
}
//musí se znovu updatovat databáze pøes Package Manager Console(pøíkaz: Update-Database)


app.Run();
