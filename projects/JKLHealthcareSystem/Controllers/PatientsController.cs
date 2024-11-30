using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JKLHealthcareSystem.Data;
using JKLHealthcareSystem.Models;

namespace JKLHealthcareSystem.Controllers
{
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all patients with caregiver information
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var patients = await _context.Patients
                .Include(p => p.Assignments)
                .ThenInclude(a => a.Caregiver)
                .ToListAsync();

            return View(patients);
        }

        // Render the Add Patient form
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        // Add a new patient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Patient request)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Address))
            {
                ModelState.AddModelError("", "Name and Address cannot be empty.");
                return View(request);
            }

            _context.Patients.Add(request);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Patient added successfully!";
            return RedirectToAction("Index");
        }

        // Render the Delete Confirmation View
        [HttpGet]
public async Task<IActionResult> Delete(int id)
{
    var patient = await _context.Patients.FindAsync(id);
    if (patient == null)
    {
        TempData["ErrorMessage"] = "Patient not found.";
        return RedirectToAction("Index");
    }

    // Remove patient assignments and update caregiver availability
    var assignments = await _context.Assignments
        .Include(a => a.Caregiver)
        .Where(a => a.PatientId == id)
        .ToListAsync();

    foreach (var assignment in assignments)
    {
        // Mark caregiver as available if no other assignments exist
        if (assignment.Caregiver != null)
        {
            var caregiverAssignmentsCount = await _context.Assignments.CountAsync(a => a.CaregiverId == assignment.CaregiverId && a.PatientId != id);
            if (caregiverAssignmentsCount == 0)
            {
                assignment.Caregiver.IsAvailable = true;
                _context.Caregivers.Update(assignment.Caregiver);
            }
        }

        _context.Assignments.Remove(assignment);
    }

    _context.Patients.Remove(patient);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Patient and associated assignments deleted successfully!";
    return RedirectToAction("Index");
}

        // Render the Assign Caregiver form
        [HttpGet]
        public async Task<IActionResult> Assign(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("Index");
            }

            ViewBag.Caregivers = await _context.Caregivers.ToListAsync();
            ViewBag.PatientName = patient.Name; // Pass the patient's name

            ViewBag.PreSelectedPatientId = patientId;
            return View();
        }

        // Assign caregiver (handle form submission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignmentDto assignmentDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Caregivers = await _context.Caregivers.ToListAsync();
                return View(assignmentDto);
            }

            var caregiver = await _context.Caregivers.FindAsync(assignmentDto.CaregiverId);
            var patient = await _context.Patients.FindAsync(assignmentDto.PatientId);

            if (caregiver == null || patient == null)
            {
                ModelState.AddModelError("", "Caregiver or Patient not found.");
                ViewBag.Caregivers = await _context.Caregivers.ToListAsync();
                return View(assignmentDto);
            }

            // Check if caregiver is already assigned to the patient
            var existingAssignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.CaregiverId == assignmentDto.CaregiverId && a.PatientId == assignmentDto.PatientId);

            if (existingAssignment != null)
            {
                ModelState.AddModelError("", "This caregiver is already assigned to the patient.");
                ViewBag.Caregivers = await _context.Caregivers.ToListAsync();
                return View(assignmentDto);
            }

            caregiver.IsAvailable = false;

            var assignment = new Assignment
            {
                CaregiverId = assignmentDto.CaregiverId,
                PatientId = assignmentDto.PatientId
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Caregiver assigned successfully!";
            return RedirectToAction("Index");
        }

        // Unassign caregiver from a patient
        [HttpPost("Patients/Unassign/{assignmentId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unassign(int assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Caregiver)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null)
            {
                TempData["ErrorMessage"] = "Assignment not found.";
                return RedirectToAction("Index");
            }

            // Mark caregiver as available
            if (assignment.Caregiver != null)
            {
                assignment.Caregiver.IsAvailable = true;
            }

            // Remove the assignment
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Caregiver unassigned successfully!";
            return RedirectToAction("Index");
        }
    }
}
