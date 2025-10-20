using System;

namespace CMCSGUI.Models
{
    public class Claim
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string LecturerName { get; set; } = string.Empty;
        public DateTime ClaimDate { get; set; } = DateTime.UtcNow;
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal Amount => Math.Round(HoursWorked * HourlyRate, 2);
        public string Status { get; set; } = "Submitted";
        public string? SupportingDocumentPath { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? Notes { get; set; }
    }
}