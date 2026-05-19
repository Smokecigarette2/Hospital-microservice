namespace Hospital.Catalog.Api.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
    }
}
