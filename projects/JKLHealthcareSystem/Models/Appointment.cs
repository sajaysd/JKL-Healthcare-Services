namespace JKLHealthcareSystem.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int CaregiverId { get; set; }
        public Caregiver? Caregiver { get; set; } // Nullable to avoid validation errors

        public int PatientId { get; set; }
        public Patient? Patient { get; set; } // Nullable to avoid validation errors

        public DateTime ScheduledTime { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
