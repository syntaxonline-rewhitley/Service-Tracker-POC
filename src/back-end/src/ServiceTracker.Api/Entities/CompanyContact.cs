namespace ServiceTracker.Api.Entities;

public class CompanyContact
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public Guid ContactId { get; set; }
    public Contact Contact { get; set; } = null!;

    public DateTime LinkedAt { get; set; }
}
