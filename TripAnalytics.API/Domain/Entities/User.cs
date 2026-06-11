namespace TripAnalytics.API.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string username { get; set; } = null!;
        public string email { get; set; } = null!;  
        public string passwordHash { get; set; } = null!;
    }
}
