using AspBlog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace AspBlog.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public AccountController
        (
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager
        )   
        {   
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

       

        //pomocná metoda pro přesněmrování uživatele
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        //akce "Login()" s formulářem pro přihláš.uživatele
        public IActionResult Login(string? returnUrl = null)
        {   
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //přetížená asynchr.metoda(s návrat.hodnotou typu "Task") "Login"
        [HttpPost]  
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {   
                Microsoft.AspNetCore.Identity.SignInResult result =
                    await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                    return RedirectToLocal(returnUrl);
                ModelState.AddModelError("Login error", "Neplatné přihlašovací údaje.");
                return View(model);
            }

            
            return View(model);
        }

        //akce s registrací uživatele
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //přetíž.akce s registrací uživatele po odeslání formuláře
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {   
                IdentityUser user = new IdentityUser { UserName = model.Email, Email = model.Email };
                IdentityResult result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {   
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToLocal(returnUrl);
                }
                
                foreach (IdentityError error in result.Errors)
                {   
                    ModelState.AddModelError(error.Code, error.Description);
                }
            }

            return View(model);
        }

        //akce odhlášení uživatele
        public async Task<IActionResult> Logout()
        {   
            await signInManager.SignOutAsync();
            return RedirectToLocal(null);
        }

        

    }
}
