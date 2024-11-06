namespace IdentityApi.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Pib { get; set; } = string.Empty;
        public string MaticniBroj { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
