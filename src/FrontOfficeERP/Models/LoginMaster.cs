using SQLite;

namespace FrontOfficeERP.Models;

[Table("LoginMaster")]
public class LoginMaster
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int UserId { get; set; }

    public DateTime LoginTime { get; set; }

    public DateTime? LogoutTime { get; set; }

    [MaxLength(50)]
    public string IpAddress { get; set; } = string.Empty;

    [MaxLength(200)]
    public string MachineName { get; set; } = string.Empty;

    public bool IsSuccess { get; set; }

    [MaxLength(500)]
    public string Remarks { get; set; } = string.Empty;
}
