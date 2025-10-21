using System;
using System.ComponentModel.DataAnnotations;

namespace CMCSGUI.Models
{
    public class Claim
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Employee name is required.")]
        public string LecturerName { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Hourly rate must be greater than 0.")]
        [Display(Name = "Hourly Rate (ZAR)")]
        [DataType(DataType.Currency)]
        public decimal HourlyRate { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Hours worked must be greater than 0.")]
        public decimal HoursWorked { get; set; }

        [Display(Name = "Total Amount (ZAR)")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount => HoursWorked * HourlyRate;

        public string Status { get; set; } = "Submitted";
        public string? Notes { get; set; }
        public string? SupportingDocumentPath { get; set; }
        public DateTime ClaimDate { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
