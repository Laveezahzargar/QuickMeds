using QuickMeds.Models.DomainModels;

namespace QuickMeds.Models.ViewModels
{
    public class UserViewModel
    {
        public List<User> Users { get; set; } = [];

        public User? User { get; set; }
    }
}