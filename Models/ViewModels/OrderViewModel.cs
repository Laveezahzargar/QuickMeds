using QuickMeds.Models.DomainModels;

namespace QuickMeds.Models.ViewModels
{
    public class OrderViewModel
    {

        public List<Order> Orders { get; set; } = [];

        public Order? Order { get; set; }
    }
}