using Microsoft.AspNetCore.Mvc;
using CMCSGUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CMCSGUI.Controllers
{
    public class ClaimsController : Controller
    {
        // In-memory data store
        private static readonly List<Claim> _claims = new List<Claim>();

        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ClaimsController> _logger;

        public ClaimsController(IWebHostEnvironment env, ILogger<ClaimsController> logger)
        {
            _env = env;
            _logger = logger;
        }

        // Show all claims
        public IActionResult Index()
        {
            try
            {
                var list = _claims.OrderByDescending(c => c.ClaimDate).ToList();
                return View(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading claims list.");
                TempData["Error"] = "An unexpected error occurred while loading claims.";
                return View(new List<Claim>());
            }
        }

        // Show pending claims
        public IActionResult Pending()
        {
            try
            {
                var pending = _claims
                    .Where(c => c.Status == "Submitted" || c.Status == "Verified")
                    .ToList();
                return View("Index", pending);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading pending claims.");
                TempData["Error"] = "An unexpected error occurred while loading pending claims.";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Create()
        {
            try
            {
                return View(new Claim());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error displaying create form.");
                TempData["Error"] = "An error occurred while preparing the claim form.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [RequestSizeLimit(10_000_000)] // ~10MB
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim, IFormFile? supportingDocument)
        {
            try
            {
                // ✅ Basic manual validation
                if (string.IsNullOrWhiteSpace(claim.LecturerName))
                    ModelState.AddModelError(nameof(claim.LecturerName), "Employee name is required.");

                if (claim.HoursWorked <= 0)
                    ModelState.AddModelError(nameof(claim.HoursWorked), "Hours worked must be greater than zero.");

                if (claim.HourlyRate <= 0)
                    ModelState.AddModelError(nameof(claim.HourlyRate), "Hourly rate must be greater than zero.");

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please correct the highlighted errors before submitting.";
                    return View(claim);
                }

                // Handle file upload
                if (supportingDocument != null && supportingDocument.Length > 0)
                {
                    try
                    {
                        var allowed = new[] { ".pdf", ".docx", ".xlsx", ".doc" };
                        var ext = Path.GetExtension(supportingDocument.FileName).ToLowerInvariant();

                        if (!allowed.Contains(ext))
                        {
                            ModelState.AddModelError("SupportingDocument", "File type not allowed. Allowed: .pdf, .docx, .xlsx, .doc");
                            TempData["Error"] = "Unsupported file format.";
                            return View(claim);
                        }
                        if (supportingDocument.Length > 5_000_000)
                        {
                            ModelState.AddModelError("SupportingDocument", "File too large (max 5 MB).");
                            TempData["Error"] = "File size exceeds the 5 MB limit.";
                            return View(claim);
                        }

                        var uploads = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
                        Directory.CreateDirectory(uploads);

                        var fileName = $"{Guid.NewGuid()}{ext}";
                        var filePath = Path.Combine(uploads, fileName);

                        using (var stream = System.IO.File.Create(filePath))
                        {
                            await supportingDocument.CopyToAsync(stream);
                        }

                        claim.SupportingDocumentPath = $"/uploads/{fileName}";
                    }
                    catch (Exception fileEx)
                    {
                        _logger.LogError(fileEx, "File upload failed.");
                        ModelState.AddModelError("", "An error occurred while uploading the document. Please try again.");
                        TempData["Error"] = "An error occurred while uploading the document.";
                        return View(claim);
                    }
                }

                // ✅ Save claim if all is valid
                claim.Id = Guid.NewGuid();
                claim.ClaimDate = DateTime.UtcNow;
                claim.Status = "Submitted";
                _claims.Add(claim);

                TempData["Message"] = "Claim submitted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating claim.");
                TempData["Error"] = "An unexpected error occurred while creating the claim.";
                return View(claim);
            }
        }

        public IActionResult Details(Guid id)
        {
            try
            {
                var claim = _claims.FirstOrDefault(c => c.Id == id);
                if (claim == null)
                {
                    TempData["Error"] = "Claim not found.";
                    return RedirectToAction(nameof(Index));
                }
                return View(claim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading claim details for {ClaimId}", id);
                TempData["Error"] = "An error occurred while loading claim details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(Guid id)
        {
            try
            {
                var claim = _claims.FirstOrDefault(c => c.Id == id);
                if (claim == null)
                {
                    TempData["Error"] = "Claim not found.";
                    return RedirectToAction(nameof(Index));
                }

                claim.Status = "Approved";
                claim.LastUpdated = DateTime.UtcNow;

                TempData["Message"] = $"Claim {claim.Id} approved.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving claim {ClaimId}", id);
                TempData["Error"] = "An unexpected error occurred while approving the claim.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(Guid id, string? reason)
        {
            try
            {
                var claim = _claims.FirstOrDefault(c => c.Id == id);
                if (claim == null)
                {
                    TempData["Error"] = "Claim not found.";
                    return RedirectToAction(nameof(Index));
                }

                claim.Status = "Rejected";
                claim.LastUpdated = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(reason))
                    claim.Notes = (claim.Notes ?? "") + $"\nRejection reason: {reason}";

                TempData["Message"] = $"Claim {claim.Id} rejected.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting claim {ClaimId}", id);
                TempData["Error"] = "An unexpected error occurred while rejecting the claim.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
