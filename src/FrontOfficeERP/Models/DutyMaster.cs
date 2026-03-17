using SQLite;

namespace FrontOfficeERP.Models;

[Table("DutyMaster")]
public class DutyMaster
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(50), NotNull]
    public string DutyCode { get; set; } = string.Empty;

    [MaxLength(200), NotNull]
    public string DutyName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(10)]
    public string StartTime { get; set; } = string.Empty;

    [MaxLength(10)]
    public string EndTime { get; set; } = string.Empty;

    [MaxLength(50)]
    public string ShiftType { get; set; } = "General";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
