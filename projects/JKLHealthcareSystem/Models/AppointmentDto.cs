namespace JKLHealthcareSystem.Models
{
    public class AppointmentDto
    {
        public int CaregiverId { get; set; }
        public int PatientId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
