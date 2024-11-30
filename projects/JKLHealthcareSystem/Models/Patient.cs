namespace JKLHealthcareSystem.Models
{
  public class Patient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string MedicalRecords { get; set; } = string.Empty;

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}

}
