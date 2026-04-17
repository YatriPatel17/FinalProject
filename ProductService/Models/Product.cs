using System.ComponentModel.DataAnnotations;

namespace ProductService.Models
{
    public class Product
    {
        [Key]
        public int Id {get; set; }

        [Required(ErrorMessage = "Product Name is required!")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is Required!")]
        [Range(0.01, 1000000, ErrorMessage = "Price must be between 0.01 and 1000000")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Product Quantity is required")]
        [Range(0, 10000 , ErrorMessage = "Stock must be between 0 and 10000")]
        public int ProductQuantity { get; set; }

        [StringLength(65)]
        public string ProductCategory { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

    }
}