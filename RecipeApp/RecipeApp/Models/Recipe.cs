namespace RecipeApp.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string InstructionsHtml { get; set; } = null!;
        public string Ingredients { get; set; } = null!;
        public DateTime DateAdded { get; set; }
        public string AuthorId { get; set; } = null!;
        public ApplicationUser Author { get; set; } = null!;
    }

}
