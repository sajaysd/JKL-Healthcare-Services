using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JKLHealthcareSystem.Data;
using JKLHealthcareSystem.Models;

namespace JKLHealthcareSystem.Controllers
{
    public class CaregiversController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CaregiversController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all caregivers with patient count
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var caregivers = await _context.Caregivers
                .Select(c => new
                {
                    Caregiver = c,
                    PatientCount = _context.Assignments.Count(a => a.CaregiverId == c.Id)
                })
                .ToListAsync();

            var model = caregivers.Select(c => new CaregiverWithPatientCount
            {
                Caregiver = c.Caregiver,
                PatientCount = c.PatientCount
            }).ToList();

            return View(model);
        }

        // Render the Add Caregiver form
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        // Add a new caregiver
        [HttpPost]
        public async Task<IActionResult> Add(Caregiver caregiver)
        {
            if (string.IsNullOrWhiteSpace(caregiver.Name))
            {
                ModelState.AddModelError("", "Name is required.");
                return View(caregiver);
            }

            caregiver.IsAvailable = true;
            await _context.Caregivers.AddAsync(caregiver);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Assign a caregiver to a patient
        [HttpPost]
        public async Task<IActionResult> Assign(AssignmentDto assignmentDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Caregivers = await _context.Caregivers.ToListAsync();
                ViewBag.Patients = await _context.Patients.ToListAsync();
                return View(assignmentDto);
            }

            var caregiver = await _context.Caregivers.FindAsync(assignmentDto.CaregiverId);
            var patient = await _context.Patients.FindAsync(assignmentDto.PatientId);

            if (caregiver == null || patient == null)
            {
                return NotFound("Caregiver or patient not found.");
            }

            caregiver.IsAvailable = false;

            var assignment = new Assignment
            {
                CaregiverId = assignmentDto.CaregiverId,
                PatientId = assignmentDto.PatientId
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Delete a caregiver[HttpPost]
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id)
{
    var caregiver = await _context.Caregivers.FindAsync(id);
    if (caregiver == null)
    {
        TempData["ErrorMessage"] = "Caregiver not found.";
        return RedirectToAction("Index");
    }

    // Check if the caregiver has active assignments
    var assignments = await _context.Assignments.Where(a => a.CaregiverId == id).ToListAsync();
    if (assignments.Any())
    {
        TempData["ErrorMessage"] = "Caregiver cannot be deleted as they have active assignments.";
        return RedirectToAction("Index");
    }

    // Delete the caregiver
    _context.Caregivers.Remove(caregiver);
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Caregiver deleted successfully!";
    return RedirectToAction("Index");
}

   
    }
}
