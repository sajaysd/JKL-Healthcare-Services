namespace JKLHealthcareSystem.Models
{
    public class Caregiver
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }
}
