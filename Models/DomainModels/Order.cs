using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuickMeds.Models.JunctionModels;
using QuickMeds.Types;

namespace QuickMeds.Models.DomainModels
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; } = Guid.NewGuid();
        public required OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
        public Guid AddressId { get; set; }
        [ForeignKey("AddressId")]
        public Address? Address { get; set; }


        public required decimal TotalPrice { get; set; } = 0;


        public Guid UserId { get; set; }  // Fk 
        public User? Buyer { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = [];

        public DateTime? DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? ShippingDate { get; set; } = DateTime.UtcNow.AddDays(6);

    }
}