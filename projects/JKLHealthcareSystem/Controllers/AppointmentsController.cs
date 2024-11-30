using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using JKLHealthcareSystem.Hubs;
using JKLHealthcareSystem.Models;
using JKLHealthcareSystem.Data;

namespace JKLHealthcareSystem.Controllers
{
    [Route("[controller]")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AppointmentsController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Display all appointments
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Caregiver)
                .Include(a => a.Patient)
                .ToListAsync();

            return View(appointments);
        }

        // Render the Schedule Appointment form
        [HttpGet("Schedule")]
        public async Task<IActionResult> Schedule()
        {
            ViewBag.Caregivers = await _context.Caregivers.ToListAsync();
            ViewBag.Patients = await _context.Patients.ToListAsync();
            return View();
        }

        // Schedule a new appointment
        [HttpPost("Schedule")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(AppointmentDto request)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid input data.";
                return RedirectToAction("Schedule");
            }

            var caregiver = await _context.Caregivers.FindAsync(request.CaregiverId);
            var patient = await _context.Patients.FindAsync(request.PatientId);

            if (caregiver == null || patient == null)
            {
                TempData["ErrorMessage"] = "Caregiver or Patient not found.";
                return RedirectToAction("Schedule");
            }

            if (!caregiver.IsAvailable)
            {
                TempData["ErrorMessage"] = "Caregiver is not available.";
                return RedirectToAction("Schedule");
            }

            caregiver.IsAvailable = false;

            var appointment = new Appointment
            {
                CaregiverId = request.CaregiverId,
                PatientId = request.PatientId,
                ScheduledTime = request.ScheduledTime,
                Notes = request.Notes
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Notify clients
            var message = $"New appointment scheduled for Patient {patient.Name} with Caregiver {caregiver.Name} at {appointment.ScheduledTime}.";
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);

            TempData["SuccessMessage"] = "Appointment scheduled successfully!";
            return RedirectToAction("Index");
        }

        // Cancel an appointment
        [HttpPost("Cancel/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Caregiver)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("Index");
            }

            if (appointment.Caregiver != null)
            {
                appointment.Caregiver.IsAvailable = true;
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            // Notify clients
            var message = $"Appointment canceled: Patient {appointment.Patient.Name} with Caregiver {appointment.Caregiver.Name}.";
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);

            TempData["SuccessMessage"] = "Appointment canceled successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet("Edit/{id}")]
public async Task<IActionResult> Edit(int id)
{
    var appointment = await _context.Appointments
        .Include(a => a.Caregiver)
        .Include(a => a.Patient)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (appointment == null)
    {
        TempData["ErrorMessage"] = "Appointment not found.";
        return RedirectToAction("Index");
    }

    ViewBag.Caregivers = await _context.Caregivers.ToListAsync();
    ViewBag.Patients = await _context.Patients.ToListAsync();
    return View(appointment);
}

[HttpPost("Edit/{id}")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, AppointmentDto request)
{
    var appointment = await _context.Appointments
        .Include(a => a.Caregiver)
        .Include(a => a.Patient)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (appointment == null)
    {
        TempData["ErrorMessage"] = "Appointment not found.";
        return RedirectToAction("Index");
    }

    var caregiver = await _context.Caregivers.FindAsync(request.CaregiverId);
    var patient = await _context.Patients.FindAsync(request.PatientId);

    if (caregiver == null || patient == null)
    {
        TempData["ErrorMessage"] = "Caregiver or Patient not found.";
        return RedirectToAction("Edit", new { id });
    }

    appointment.CaregiverId = request.CaregiverId;
    appointment.PatientId = request.PatientId;
    appointment.ScheduledTime = request.ScheduledTime;
    appointment.Notes = request.Notes;

    _context.Appointments.Update(appointment);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Appointment updated successfully!";
    return RedirectToAction("Index");
}


    }
}
