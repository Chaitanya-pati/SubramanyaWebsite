using System.ComponentModel.DataAnnotations;

namespace SubramanyaWebsite.Models;

public sealed class CareRequest
{
    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string PatientName { get; set; } = string.Empty;

    [StringLength(160)]
    public string? Location { get; set; }

    [Required, StringLength(120)]
    public string RequiredService { get; set; } = string.Empty;
}