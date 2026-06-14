using Dapper;
using Microsoft.EntityFrameworkCore;
using ServiceTracker.Api.Data;
using ServiceTracker.Api.Models;

namespace ServiceTracker.Api.Repositories;

public class DashboardRepository(ServiceTrackerDbContext db) : IDashboardRepository
{
    public async Task<DashboardStats> GetStatsAsync(CancellationToken ct = default)
    {
        var conn = db.Database.GetDbConnection();

        const string sql = """
            -- Ticket counts by status (stored as text via HasConversion<string>())
            SELECT "Status" AS "Status", COUNT(*)::int AS "Count"
            FROM   "ServiceTickets"
            GROUP  BY "Status";

            -- Ticket counts by priority
            SELECT "Priority" AS "Priority", COUNT(*)::int AS "Count"
            FROM   "ServiceTickets"
            GROUP  BY "Priority";

            -- Tickets per technician (open + in-progress)
            SELECT COALESCE(t."FirstName" || ' ' || t."LastName", 'Unassigned') AS "TechnicianName",
                   COUNT(*) FILTER (WHERE st."Status" = 'Open')::int        AS "Open",
                   COUNT(*) FILTER (WHERE st."Status" = 'InProgress')::int  AS "InProgress",
                   COUNT(*)::int                                             AS "Total"
            FROM   "ServiceTickets" st
            LEFT   JOIN "Technicians" t ON t."Id" = st."TechnicianId"
            GROUP  BY t."FirstName", t."LastName"
            ORDER  BY "Total" DESC
            LIMIT  10;

            -- 10 most recent tickets
            SELECT st."Id"           AS "Id",
                   st."TicketNumber" AS "TicketNumber",
                   st."Title"        AS "Title",
                   st."Status"       AS "Status",
                   st."Priority"     AS "Priority",
                   c."Name"          AS "CompanyName",
                   st."CreatedAt"    AS "CreatedAt"
            FROM   "ServiceTickets" st
            JOIN   "Companies" c ON c."Id" = st."CompanyId"
            ORDER  BY st."CreatedAt" DESC
            LIMIT  10;

            -- Daily trend (last 30 days)
            SELECT TO_CHAR(DATE_TRUNC('day', "CreatedAt"), 'Mon DD') AS "Date",
                   COUNT(*)::int AS "Count"
            FROM   "ServiceTickets"
            WHERE  "CreatedAt" >= NOW() - INTERVAL '29 days'
            GROUP  BY DATE_TRUNC('day', "CreatedAt")
            ORDER  BY DATE_TRUNC('day', "CreatedAt");

            -- Scalar counts
            SELECT
                (SELECT COUNT(*)::int FROM "ServiceTickets")                                                                                 AS "TotalTickets",
                (SELECT COUNT(*)::int FROM "ServiceTickets" WHERE "Status" = 'Open')                                                         AS "OpenTickets",
                (SELECT COUNT(*)::int FROM "ServiceTickets" WHERE "Status" = 'InProgress')                                                   AS "InProgressTickets",
                (SELECT COUNT(*)::int FROM "ServiceTickets" WHERE "Status" = 'OnHold')                                                       AS "OnHoldTickets",
                (SELECT COUNT(*)::int FROM "ServiceTickets" WHERE "Status" = 'Resolved')                                                     AS "ResolvedTickets",
                (SELECT COUNT(*)::int FROM "ServiceTickets" WHERE "Status" = 'Closed')                                                       AS "ClosedTickets",
                (SELECT COUNT(*)::int FROM "ServiceTickets" WHERE DATE_TRUNC('month', "CreatedAt") = DATE_TRUNC('month', NOW()))              AS "MonthlyTickets",
                (SELECT COUNT(*)::int FROM "ServiceTickets" WHERE "CreatedAt" >= NOW() - INTERVAL '29 days')                                 AS "Last30DaysTickets",
                (SELECT COUNT(*)::int FROM "ServiceTickets" WHERE DATE_TRUNC('day', "CreatedAt") = DATE_TRUNC('day', NOW()))                 AS "DailyTickets",
                (SELECT COUNT(*)::int FROM "Technicians")                                                                                    AS "TotalTechnicians",
                (SELECT COUNT(*)::int FROM "Technicians"   WHERE "IsActive" = true)                                                          AS "ActiveTechnicians",
                (SELECT COUNT(*)::int FROM "Companies")                                                                                      AS "TotalCompanies",
                (SELECT COUNT(*)::int FROM "Contacts")                                                                                       AS "TotalContacts";
            """;

        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(ct);

        try
        {
            using var multi = await conn.QueryMultipleAsync(new CommandDefinition(sql, cancellationToken: ct));

            var byStatus = (await multi.ReadAsync<TicketsByStatusItem>()).ToList();
            var byPriority = (await multi.ReadAsync<TicketsByPriorityItem>()).ToList();
            var byTechnician = (await multi.ReadAsync<TicketsByTechnicianItem>()).ToList();
            var recent = (await multi.ReadAsync<RecentTicketItem>()).ToList();
            var trend = (await multi.ReadAsync<DailyTrendItem>()).ToList();
            var scalars = await multi.ReadSingleAsync<ScalarCounts>();

            return new DashboardStats(
                TotalTickets: scalars.TotalTickets,
                OpenTickets: scalars.OpenTickets,
                InProgressTickets: scalars.InProgressTickets,
                OnHoldTickets: scalars.OnHoldTickets,
                ResolvedTickets: scalars.ResolvedTickets,
                ClosedTickets: scalars.ClosedTickets,
                MonthlyTickets: scalars.MonthlyTickets,
                Last30DaysTickets: scalars.Last30DaysTickets,
                DailyTickets: scalars.DailyTickets,
                TotalTechnicians: scalars.TotalTechnicians,
                ActiveTechnicians: scalars.ActiveTechnicians,
                TotalCompanies: scalars.TotalCompanies,
                TotalContacts: scalars.TotalContacts,
                TicketsByStatus: byStatus,
                TicketsByPriority: byPriority,
                TicketsByTechnician: byTechnician,
                RecentTickets: recent,
                DailyTrend: trend
            );
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }
    }

    public async Task<TechnicianDashboardStats?> GetTechnicianStatsAsync(string userId, CancellationToken ct = default)
    {
        var conn = db.Database.GetDbConnection();

        const string sql = """
            -- Technician name
            SELECT COALESCE("FirstName" || ' ' || "LastName", '') AS "Name"
            FROM   "Technicians"
            WHERE  "UserId" = @UserId
            LIMIT  1;

            -- Tickets by status for this technician
            SELECT st."Status" AS "Status", COUNT(*)::int AS "Count"
            FROM   "ServiceTickets" st
            JOIN   "Technicians" t ON t."Id" = st."TechnicianId"
            WHERE  t."UserId" = @UserId
            GROUP  BY st."Status";

            -- Tickets by priority for this technician
            SELECT st."Priority" AS "Priority", COUNT(*)::int AS "Count"
            FROM   "ServiceTickets" st
            JOIN   "Technicians" t ON t."Id" = st."TechnicianId"
            WHERE  t."UserId" = @UserId
            GROUP  BY st."Priority";

            -- 10 most recent tickets for this technician
            SELECT st."Id"           AS "Id",
                   st."TicketNumber" AS "TicketNumber",
                   st."Title"        AS "Title",
                   st."Status"       AS "Status",
                   st."Priority"     AS "Priority",
                   c."Name"          AS "CompanyName",
                   st."CreatedAt"    AS "CreatedAt"
            FROM   "ServiceTickets" st
            JOIN   "Companies" c ON c."Id" = st."CompanyId"
            JOIN   "Technicians" t ON t."Id" = st."TechnicianId"
            WHERE  t."UserId" = @UserId
            ORDER  BY st."CreatedAt" DESC
            LIMIT  10;

            -- Daily trend for this technician (last 30 days)
            SELECT TO_CHAR(DATE_TRUNC('day', st."CreatedAt"), 'Mon DD') AS "Date",
                   COUNT(*)::int AS "Count"
            FROM   "ServiceTickets" st
            JOIN   "Technicians" t ON t."Id" = st."TechnicianId"
            WHERE  t."UserId" = @UserId
              AND  st."CreatedAt" >= NOW() - INTERVAL '29 days'
            GROUP  BY DATE_TRUNC('day', st."CreatedAt")
            ORDER  BY DATE_TRUNC('day', st."CreatedAt");

            -- Scalar counts for this technician
            SELECT
                (SELECT COUNT(*)::int FROM "ServiceTickets" st JOIN "Technicians" t ON t."Id" = st."TechnicianId" WHERE t."UserId" = @UserId)                                                                                     AS "TotalTickets",
                (SELECT COUNT(*)::int FROM "ServiceTickets" st JOIN "Technicians" t ON t."Id" = st."TechnicianId" WHERE t."UserId" = @UserId AND st."Status" = 'Open')                                                             AS "OpenTickets",
                (SELECT COUNT(*)::int FROM "ServiceTickets" st JOIN "Technicians" t ON t."Id" = st."TechnicianId" WHERE t."UserId" = @UserId AND st."Status" = 'InProgress')                                                       AS "InProgressTickets",
                (SELECT COUNT(*)::int FROM "ServiceTickets" st JOIN "Technicians" t ON t."Id" = st."TechnicianId" WHERE t."UserId" = @UserId AND st."Status" = 'OnHold')                                                           AS "OnHoldTickets",
                (SELECT COUNT(*)::int FROM "ServiceTickets" st JOIN "Technicians" t ON t."Id" = st."TechnicianId" WHERE t."UserId" = @UserId AND st."Status" = 'Resolved')                                                         AS "ResolvedTickets",
                (SELECT COUNT(*)::int FROM "ServiceTickets" st JOIN "Technicians" t ON t."Id" = st."TechnicianId" WHERE t."UserId" = @UserId AND st."Status" = 'Closed')                                                           AS "ClosedTickets",
                (SELECT COUNT(*)::int FROM "ServiceTickets" st JOIN "Technicians" t ON t."Id" = st."TechnicianId" WHERE t."UserId" = @UserId AND DATE_TRUNC('month', st."CreatedAt") = DATE_TRUNC('month', NOW()))                  AS "MonthlyTickets",
                (SELECT COUNT(*)::int FROM "ServiceTickets" st JOIN "Technicians" t ON t."Id" = st."TechnicianId" WHERE t."UserId" = @UserId AND st."CreatedAt" >= NOW() - INTERVAL '29 days')                                      AS "Last30DaysTickets",
                (SELECT COUNT(*)::int FROM "ServiceTickets" st JOIN "Technicians" t ON t."Id" = st."TechnicianId" WHERE t."UserId" = @UserId AND DATE_TRUNC('day', st."CreatedAt") = DATE_TRUNC('day', NOW()))                      AS "DailyTickets";
            """;

        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(ct);

        try
        {
            using var multi = await conn.QueryMultipleAsync(
                new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));

            var nameRow = await multi.ReadFirstOrDefaultAsync<TechName>();
            if (nameRow is null) return null;

            var byStatus = (await multi.ReadAsync<TicketsByStatusItem>()).ToList();
            var byPriority = (await multi.ReadAsync<TicketsByPriorityItem>()).ToList();
            var recent = (await multi.ReadAsync<RecentTicketItem>()).ToList();
            var trend = (await multi.ReadAsync<DailyTrendItem>()).ToList();
            var scalars = await multi.ReadSingleAsync<TechScalarCounts>();

            return new TechnicianDashboardStats(
                TechnicianName: nameRow.Name,
                TotalTickets: scalars.TotalTickets,
                OpenTickets: scalars.OpenTickets,
                InProgressTickets: scalars.InProgressTickets,
                OnHoldTickets: scalars.OnHoldTickets,
                ResolvedTickets: scalars.ResolvedTickets,
                ClosedTickets: scalars.ClosedTickets,
                MonthlyTickets: scalars.MonthlyTickets,
                Last30DaysTickets: scalars.Last30DaysTickets,
                DailyTickets: scalars.DailyTickets,
                TicketsByStatus: byStatus,
                TicketsByPriority: byPriority,
                RecentTickets: recent,
                DailyTrend: trend
            );
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }
    }

    private sealed record TechName(string Name);

    private sealed record TechScalarCounts(
        int TotalTickets,
        int OpenTickets,
        int InProgressTickets,
        int OnHoldTickets,
        int ResolvedTickets,
        int ClosedTickets,
        int MonthlyTickets,
        int Last30DaysTickets,
        int DailyTickets
    );

    // Private DTO for the scalar row
    private sealed record ScalarCounts(
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
        int TotalContacts
    );
}
