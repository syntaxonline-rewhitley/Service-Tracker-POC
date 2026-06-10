namespace ServiceTracker.Api.Entities;

public enum ServiceTicketStatus
{
    Open,
    InProgress,
    OnHold,
    Resolved,
    Closed
}

public enum ServiceTicketPriority
{
    Low,
    Medium,
    High,
    Critical
}

public class ServiceTicket
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ServiceTicketStatus Status { get; set; } = ServiceTicketStatus.Open;
    public ServiceTicketPriority Priority { get; set; } = ServiceTicketPriority.Medium;

    // Client (Company)
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    // Contact at the client company
    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }

    // Assigned technician
    public Guid? TechnicianId { get; set; }
    public Technician? Technician { get; set; }

    public DateTime? ScheduledDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
