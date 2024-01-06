using System.ComponentModel.DataAnnotations.Schema;

namespace MultiShop.Models
{
    public class Slide
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public int Order { get; set; }
        [NotMapped]
        public IFormFile Photo { get; set; }
    }
}
