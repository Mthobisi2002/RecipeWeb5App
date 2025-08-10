using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipeApp.Models;
using RecipeApp.Models.ViewModels;

namespace RecipeApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Auth/Login
        public IActionResult Login() => View();

        // POST: /Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                {
                    return RedirectToRoute(new { controller = "Admin", action = "Index" });
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }


        // GET: /Auth/Register
        public IActionResult Register() => View();

        // POST: /Auth/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Optional: assign default role (e.g., Customer)
                await _userManager.AddToRoleAsync(user, "Customer");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // POST: /Auth/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
