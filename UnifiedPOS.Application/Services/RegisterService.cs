using UnifiedPOS.Application.Interfaces.ServiceInterface;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.HelperInterface;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.RegisterDTO;

namespace UnifiedPOS.Application.Services
{
    public class RegisterService(IUnitOfWork uow, IRequestContext request) : ServiceBase(uow, request), IRegisterService
    {
        public Task<ResponseMessage> OpenShiftAsync(OpenShiftRequest model, string key, CancellationToken ct) => MutateAsync("Shift.Open", key, model, async () =>
        {
            Money.Validate(model.StartingCash);
            var register = await RegisterAsync(model.RegisterId, ct);
            var location = await LocationAsync(register.LocationId, ct);
            if (!register.IsActive || !location.IsActive) throw PosException.Conflict("Register and location must be active.");
            if (await uow.RegisterRepository.AnyShiftsAsync(x => (x.RegisterId == register.Id || x.CashierId == ActorId) && x.ClosedOn == null, ct)) throw PosException.Conflict("Register or cashier already has an open shift.");
            var shift = new Shift { RegisterId = register.Id, CashierId = ActorId, StartingCash = model.StartingCash };
            uow.RegisterRepository.AddShift(shift); Audit("Shift.Opened", shift.Id, location.Id);
            return Map(shift);
        }, ct);
        public async Task<ResponseMessage> CurrentShiftAsync(Guid registerId, CancellationToken ct)
        {
            await RegisterAsync(registerId, ct);
            var shift = await uow.RegisterRepository.FindShiftAsync(x => x.RegisterId == registerId && x.ClosedOn == null, ct, false);
            if (shift != null && request.Role == UserRole.Cashier && shift.CashierId != ActorId) throw PosException.Forbidden();
            return ResponseMessage.Ok(shift == null ? null : Map(shift));
        }
        public Task<ResponseMessage> CloseShiftAsync(Guid id, CloseShiftRequest model, string key, CancellationToken ct) => MutateAsync("Shift.Close", key, new { id, model }, async () =>
        {
            Money.Validate(model.CountedCash);
            var shift = await ShiftAsync(id, ct);
            if (await uow.SalesRepository.AnySalesAsync(x => x.ShiftId == id && x.Status == SaleStatus.Pending, ct)) throw PosException.Conflict("Complete or void pending sales before closing the shift.");
            shift.ExpectedCash = shift.StartingCash + await uow.ReportRepository.CashBalanceAsync(id, ct);
            shift.CountedCash = model.CountedCash; shift.ClosedOn = DateTime.UtcNow;
            var register = await RegisterAsync(shift.RegisterId, ct);
            Audit("Shift.Closed", id, register.LocationId, shift.CountedCash != shift.ExpectedCash ? "Cash discrepancy requires manager review." : null);
            return Map(shift);
        }, ct);
        public Task<ResponseMessage> ReviewShiftAsync(Guid id, ReasonRequest model, string key, CancellationToken ct) => MutateAsync("Shift.Review", key, new { id, model }, async () =>
        {
            RequireManager(); var shift = await ShiftAsync(id, ct, false);
            if (shift.ClosedOn == null) throw PosException.Conflict("Close the shift before review.");
            shift.ReviewedBy = ActorId; shift.ReviewReason = model.Reason;
            var register = await RegisterAsync(shift.RegisterId, ct);
            Audit("Shift.Reviewed", id, register.LocationId, model.Reason);
            return Map(shift);
        }, ct);
        public static ShiftResponse Map(Shift x) => new(x.Id, x.RegisterId, x.CashierId, x.StartingCash, x.ExpectedCash, x.CountedCash, x.CountedCash - x.ExpectedCash, x.ClosedOn, x.ReviewedBy);
    }
}
