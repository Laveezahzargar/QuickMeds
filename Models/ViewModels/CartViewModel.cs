using QuickMeds.Models.DomainModels;
using QuickMeds.Models.JunctionModels;

namespace QuickMeds.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartItem> CartItems { get; set; } = [];
        public Cart? Cart { get; set; }
    }
}