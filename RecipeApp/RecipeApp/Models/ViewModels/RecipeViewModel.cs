using System.ComponentModel.DataAnnotations;

namespace RecipeApp.Models.ViewModels
{
    public class RecipeViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; } = null!;

        [Required]
        public string Ingredients { get; set; } = null!;

        [Required]
        public string InstructionsHtml { get; set; } = null!;
    }

}
