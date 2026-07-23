namespace ServiceTracker.Api.Models;

public record DashboardStats(
    int TotalTickets,
    int OpenTickets,
    int InProgressTickets,
    int OnHoldTickets,
    int ResolvedTickets,
    int ClosedTickets,
    int MonthlyTickets,
    int Last30DaysTickets,
    int DailyTickets,
    int TotalTechnicians,
    int ActiveTechnicians,
    int TotalCompanies,
    int TotalContacts,
    IReadOnlyList<TicketsByStatusItem> TicketsByStatus,
    IReadOnlyList<TicketsByPriorityItem> TicketsByPriority,
    IReadOnlyList<TicketsByTechnicianItem> TicketsByTechnician,
    IReadOnlyList<RecentTicketItem> RecentTickets,
    IReadOnlyList<DailyTrendItem> DailyTrend
);

public record TicketsByStatusItem(string Status, int Count);

public record TicketsByPriorityItem(string Priority, int Count);

public record TicketsByTechnicianItem(string TechnicianName, int Open, int InProgress, int Total);

public record RecentTicketItem(
    Guid Id,
    string TicketNumber,
    string Title,
    string Status,
    string Priority,
    string CompanyName,
    DateTime CreatedAt
);

public record DailyTrendItem(string Date, int Count);

public record TechnicianDashboardStats(
    string TechnicianName,
    int TotalTickets,
    int OpenTickets,
    int InProgressTickets,
    int OnHoldTickets,
    int ResolvedTickets,
    int ClosedTickets,
    int MonthlyTickets,
    int Last30DaysTickets,
    int DailyTickets,
    IReadOnlyList<TicketsByStatusItem> TicketsByStatus,
    IReadOnlyList<TicketsByPriorityItem> TicketsByPriority,
    IReadOnlyList<RecentTicketItem> RecentTickets,
    IReadOnlyList<DailyTrendItem> DailyTrend
);
