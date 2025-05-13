using System.ComponentModel.DataAnnotations;
using QuickMeds.Models.JunctionModels;
using QuickMeds.Types;

namespace QuickMeds.Models.DomainModels
{
    public class Product
    {
        [Key]
        public required Guid ProductId { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public required string Description { get; set; }
        public string? ImageUrl { get; set; }
        public required int OldPrice { get; set; }
        public required int NewPrice { get; set; }
        public required int Discount { get; set; }
        public required int Stock { get; set; }

        public int Rating { get; set; } = 4;
        public int? Sold { get; set; }
        public required ProductCategory Category { get; set; } = ProductCategory.All;
        public required string SubCategory { get; set; }
        public required Brand Brand { get; set; }
        public required bool IsDeleted { get; set; } = false;
        public required bool IsActive { get; set; } = true;
        public ICollection<CartItem> CartItems { get; set; } = [];
        public ICollection<OrderItem> OrderItems { get; set; } = [];
        
    }
}