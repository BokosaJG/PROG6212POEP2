using Microsoft.AspNetCore.Mvc;
using CMCS_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace CMCS_MVC.Controllers
{
    public class ClaimsController : Controller
    {
        // Simple in-memory store for prototype purposes
        private static List<Claim> _claims = new List<Claim>
        {
        
        };

        private readonly IWebHostEnvironment _env;
        public ClaimsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // Show all claims (for admin/coordinator/lecturer)
        public IActionResult Index()
        {
            return View(_claims.OrderByDescending(c => c.ClaimDate).ToList());
        }

        // Show pending claims only (for coordinators/managers)
        public IActionResult Pending()
        {
            var pending = _claims.Where(c => c.Status == "Submitted" || c.Status == "Verified").ToList();
            return View("Index", pending);
        }

        public IActionResult Create()
        {
            return View(new Claim());
        }

        [HttpPost]
        [RequestSizeLimit(10_000_000)] // ~10MB
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim, IFormFile? supportingDocument)
        {
            if (!ModelState.IsValid)
            {
                return View(claim);
            }

            // Basic validation
            if (claim.HoursWorked < 0 || claim.HourlyRate < 0)
            {
                ModelState.AddModelError("", "Hours and Rate must be non-negative.");
                return View(claim);
            }

            // Handle file upload
            if (supportingDocument != null && supportingDocument.Length > 0)
            {
                var allowed = new[] { ".pdf", ".docx", ".xlsx", ".doc" };
                var ext = Path.GetExtension(supportingDocument.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("SupportingDocument", "File type not allowed. Allowed: .pdf, .docx, .xlsx, .doc");
                    return View(claim);
                }
                if (supportingDocument.Length > 5_000_000)
                {
                    ModelState.AddModelError("SupportingDocument", "File too large (max 5 MB).");
                    return View(claim);
                }

                var uploads = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(),"wwwroot"), "uploads");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = System.IO.File.Create(filePath))
                {
                    await supportingDocument.CopyToAsync(stream);
                }
                claim.SupportingDocumentPath = $"/uploads/{fileName}";
            }

            claim.Id = Guid.NewGuid();
            claim.ClaimDate = DateTime.UtcNow;
            claim.Status = "Submitted";
            _claims.Add(claim);

            TempData["Message"] = "Claim submitted successfully.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(Guid id)
        {
            var claim = _claims.FirstOrDefault(c => c.Id == id);
            if (claim == null) return NotFound();
            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(Guid id)
        {
            var claim = _claims.FirstOrDefault(c => c.Id == id);
            if (claim == null) return NotFound();
            claim.Status = "Approved";
            claim.LastUpdated = DateTime.UtcNow;
            TempData["Message"] = $"Claim {claim.Id} approved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(Guid id, string? reason)
        {
            var claim = _claims.FirstOrDefault(c => c.Id == id);
            if (claim == null) return NotFound();
            claim.Status = "Rejected";
            claim.LastUpdated = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(reason))
                claim.Notes = (claim.Notes ?? "") + $"\nRejection reason: {reason}";
            TempData["Message"] = $"Claim {claim.Id} rejected.";
            return RedirectToAction(nameof(Index));
        }
    }
}
