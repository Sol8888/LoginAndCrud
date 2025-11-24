using LoginAndCrud.Contracts;
using LoginAndCrud.Domain;
using LoginAndCrud.Infrastructure;
using Microsoft.EntityFrameworkCore;
using LoginAndCrud.Infrastructure.Security;

namespace LoginAndCrud.Application;

public interface IReservationService
{
    Task<ReservationResponse> CreateAsync(CreateReservationRequest req, int userId, string actor, CancellationToken ct);
    Task<PagedReservationsResponse> GetMyReservationsAsync(int userId, int page, int pageSize, CancellationToken ct);
    Task<PagedReservationsResponse> GetByUserAsync(int userId, int page, int pageSize, CancellationToken ct);
    Task<PagedReservationsResponse> GetByCompanyAsync(int companyId, int page, int pageSize, CancellationToken ct);

}
public class ReservationService(AppDbContext db) : IReservationService
{
    public async Task<ReservationResponse> CreateAsync(CreateReservationRequest req, int userId, string actor, CancellationToken ct)
    {
        var activity = await db.Activities
            .Where(a => a.Id == req.ActivityId)
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException("Actividad no encontrada");

            // Calcular la cantidad total de lugares ya reservados para esta actividad
        var reserved = await db.Reservations
            .Where(r => r.ActivityId == req.ActivityId)
            .SumAsync(r => (int?)r.Quantity, ct) ?? 0;

        if (activity.Capacity.HasValue && reserved + req.Quantity > activity.Capacity)
        {
            throw new InvalidOperationException("No hay suficientes cupos disponibles para esta actividad.");
        }

        var reservation = new Reservation
        {
            ActivityId = activity.Id,
            UserId = userId,
            Quantity = req.Quantity,
            UnitPrice = activity.Price ?? 0,
            Status = "Pending",
            ReservedAt = DateTime.UtcNow,
            CreatedBy = actor
        };

        db.Reservations.Add(reservation);
        await db.SaveChangesAsync(ct);

        return new ReservationResponse(
            reservation.Id, reservation.ActivityId, reservation.UserId,
            reservation.Quantity, reservation.UnitPrice, reservation.TotalAmount,
            reservation.Status, reservation.ReservedAt, reservation.ExpiresAt, reservation.CreatedBy
        );
    }

    public async Task<PagedReservationsResponse> GetMyReservationsAsync(int userId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Reservations.Where(r => r.UserId == userId);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(r => r.ReservedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationResponse(
                r.Id, r.ActivityId, r.UserId, r.Quantity, r.UnitPrice, r.TotalAmount,
                r.Status, r.ReservedAt, r.ExpiresAt, r.CreatedBy))
            .ToListAsync(ct);

        return new(page, pageSize, total, items);
    }

    public async Task<PagedReservationsResponse> GetByUserAsync(int userId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Reservations
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.ReservedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationResponse(
                r.Id, r.ActivityId, r.UserId, r.Quantity, r.UnitPrice,
                r.TotalAmount, r.Status, r.ReservedAt, r.ExpiresAt, r.CreatedBy))
            .ToListAsync(ct);

        return new(page, pageSize, total, items);
    }
    public async Task<PagedReservationsResponse> GetByCompanyAsync(int companyId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Reservations
            .Where(r => r.Activity.CompanyId == companyId)
            .OrderByDescending(r => r.ReservedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationResponse(
                r.Id, r.ActivityId, r.UserId, r.Quantity, r.UnitPrice,
                r.TotalAmount, r.Status, r.ReservedAt, r.ExpiresAt, r.CreatedBy))
            .ToListAsync(ct);

        return new(page, pageSize, total, items);
    }

}
