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
//vymaz�n� slo�ky Areas (byl tam v�choz� pohled pro vykresl.identify pohled�)
//�prava st�vaj�c� metody "AddDefaultIdentity()"
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
//p�id�n� metody "ConfigureApplicationCookie()" pro nastaven� n�vrat.adresy
builder.Services.ConfigureApplicationCookie(options =>
{   //adresa pro p�esm�rov�n�,p�i pokusu vytv./edit admin akc�(Vytvo�it,Editovat)
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
//deaktivace "MapRazorPages()"-tuto metodu vyu��vaj� v�choz� pohledy autentizace
//app.MapRazorPages();

//vytvo�en� vlastn�ho r�mce(scope) (pro administr�tora)
using (IServiceScope scope = app.Services.CreateScope())//"CreateScope()"-vytvo�.r�mce
{   //admin=IdentityRole; user=IdentityUser
    RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    UserManager<IdentityUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    IdentityUser? defaultAdminUser = await userManager.FindByEmailAsync("admin@admin.cz");//"michalh18@seznam.cz"//vyhled.usera podle emailu,kter�mu chceme p�i�.roli admina

    if (!await roleManager.RoleExistsAsync(UserRoles.Admin))//jestli admin neexistuje,vytvo�� ho
        await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
    //zkus� p�i�adit roli admina,pokud ji je�t� nem�
    if (defaultAdminUser is not null && !await userManager.IsInRoleAsync(defaultAdminUser, UserRoles.Admin))
        await userManager.AddToRoleAsync(defaultAdminUser, UserRoles.Admin);
}
//mus� se znovu updatovat datab�ze p�es Package Manager Console(p��kaz: Update-Database)


app.Run();
