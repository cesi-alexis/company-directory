using CompanyDirectory.Models.Entities;

namespace CompanyDirectory.API.Interfaces
{
    public interface IWorkerService : ICrudService<Worker>
    {
        Task<(int TotalWorkers, int SuccessCount, List<(int WorkerId, string ErrorMessage)> Errors)> TransferWorkersAsync(
            List<int> workerIds, int? newLocationId, int? newServiceId, bool allowPartialTransfer);
    }
}