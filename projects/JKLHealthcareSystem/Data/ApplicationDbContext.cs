using JKLHealthcareSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace JKLHealthcareSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Caregiver> Caregivers { get; set; } = null!;
        public DbSet<Assignment> Assignments { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;



    }




}
