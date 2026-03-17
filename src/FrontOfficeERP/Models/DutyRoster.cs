using SQLite;

namespace FrontOfficeERP.Models;

[Table("DutyRoster")]
public class DutyRoster
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int EmployeeId { get; set; }

    [Indexed]
    public int DutyId { get; set; }

    public DateTime DutyDate { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Scheduled";

    [MaxLength(500)]
    public string Remarks { get; set; } = string.Empty;

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation helpers (not stored in DB)
    [Ignore]
    public string EmployeeName { get; set; } = string.Empty;

    [Ignore]
    public string DutyName { get; set; } = string.Empty;

    [Ignore]
    public string DutyCode { get; set; } = string.Empty;

    [Ignore]
    public string ShiftType { get; set; } = string.Empty;
}
