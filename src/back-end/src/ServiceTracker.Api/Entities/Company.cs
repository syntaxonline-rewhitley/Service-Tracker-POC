namespace ServiceTracker.Api.Entities;

public class Company
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<CompanyContact> CompanyContacts { get; set; } = [];
}
