namespace JKLHealthcareSystem.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        public int CaregiverId { get; set; } // Only the ID is required in the request
        public Caregiver? Caregiver { get; set; }

        public int PatientId { get; set; } // Only the ID is required in the request
        public Patient? Patient { get; set; }
    }
}

