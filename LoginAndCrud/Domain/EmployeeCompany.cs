using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoginAndCrud.Domain;

public class EmployeeCompany
{
    [Column(Order = 0)]
    public int CompanyId { get; set; }

    [Column(Order = 1)]
    public int UserId { get; set; }

    [MaxLength(50)]
    public string? RoleInCompany { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    public Company Company { get; set; } = null!;
    public User User { get; set; } = null!;
}
