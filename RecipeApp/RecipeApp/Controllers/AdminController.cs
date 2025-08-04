using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeApp.Data;
using RecipeApp.Models;
using RecipeApp.Models.ViewModels;
using System.Text.RegularExpressions;

namespace RecipeApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
            _userManager = userManager;
        }

        // GET: /Recipes
        public async Task<IActionResult> Index(int skip = 0)
        {
            int pageSize = _configuration.GetValue<int>("RecipeSettings:PageSize");

            var recipes = await _context.Recipes
                .OrderByDescending(r => r.DateAdded)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalCount = await _context.Recipes.CountAsync();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_RecipeCardsPartial", recipes);
            }

            return View(recipes);
        }

        public IActionResult GetTotalRecipeCount()
        {
            return Json(_context.Recipes.Count());
        }


        // GET: /Recipes/Create
        public IActionResult Create() => View();

        // POST: /Recipes/Create
        [HttpPost]
        public async Task<IActionResult> Create(RecipeViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.GetUserAsync(User);

            var recipe = new Recipe
            {
                Name = vm.Name,
                ImageUrl = vm.ImageUrl,
                Ingredients = vm.Ingredients,
                InstructionsHtml = vm.InstructionsHtml,
                DateAdded = DateTime.UtcNow,
                Slug = GenerateSlug(vm.Name),
                AuthorId = user.Id
            };

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            return RedirectToRoute(new { controller = "Admin", action = "Index" });
        }


        // GET: /Recipes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) return NotFound();

            var viewModel = new RecipeViewModel
            {
                Id = recipe.Id,
                Name = recipe.Name,
                ImageUrl = recipe.ImageUrl,
                Ingredients = recipe.Ingredients,
                InstructionsHtml = recipe.InstructionsHtml
            };

            return View(viewModel);
        }


        // POST: /Recipes/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(RecipeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var recipe = await _context.Recipes.FindAsync(model.Id);
            if (recipe == null) return NotFound();

            recipe.Name = model.Name;
            recipe.ImageUrl = model.ImageUrl;
            recipe.Ingredients = model.Ingredients;
            recipe.InstructionsHtml = model.InstructionsHtml;
            recipe.Slug = GenerateSlug(model.Name); // If you want to regenerate it on edit

            await _context.SaveChangesAsync();

            return RedirectToRoute(new { controller = "Admin", action = "Index" });
        }


        // GET: /Recipes/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) return NotFound();
            return View(recipe);
        }

        // POST: /Recipes/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) return NotFound();

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return RedirectToRoute(new { controller = "Admin", action = "Index" });
        }

        // GET: /recipes/[slug]
        [AllowAnonymous]
        [Route("/recipes/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Author)
                .FirstOrDefaultAsync(r => r.Slug == slug);

            if (recipe == null) return NotFound();

            return View(recipe);
        }

        private string GenerateSlug(string name)
        {
            string slug = Regex.Replace(name.ToLower(), @"\s+", "-"); // replace spaces
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", ""); // remove invalid chars
            return slug;
        }
    }
}
