using CompanyDirectory.API.Interfaces;
using CompanyDirectory.API.Services;
using CompanyDirectory.API.Utils;
using CompanyDirectory.Common.Exceptions;
using CompanyDirectory.Common;
using CompanyDirectory.Models.Entities;
using CompanyDirectory.Models.ViewsModels.Requests;
using CompanyDirectory.Models.ViewsModels.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CompanyDirectory.API.Controllers
{
    /// <summary>
    /// Contrôleur pour gérer les opérations CRUD sur les employés.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WorkerController(IWorkerService workerService) : ControllerBase
    {
        private readonly IWorkerService _workerService = workerService;

        /// <summary>
        /// Crée un nouvel employé.
        /// </summary>
        /// <param name="model">Données pour l'employé à créer.</param>
        /// <returns>L'employé nouvellement créé.</returns>
        [HttpPost]
        public async Task<IActionResult> PostWorker([FromBody] WorkerUpsertRequestViewModel model)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                // Conversion du ViewModel en modèle d'entité
                var worker = new Worker
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneFixed = model.PhoneFixed,
                    PhoneMobile = model.PhoneMobile,
                    Email = model.Email,
                    ServiceId = model.ServiceId,
                    LocationId = model.LocationId
                };

                return await _workerService.CreateAsync(worker);
            }, StatusCodes.Status201Created);
        }

        /// <summary>
        /// Récupère une liste paginée d'employés avec des champs optionnels et un terme de recherche.
        /// </summary>
        /// <param name="query">Paramètres de pagination et de filtrage.</param>
        /// <returns>Réponse paginée contenant les employés.</returns>
        [HttpGet]
        public async Task<IActionResult> GetWorkers([FromQuery] GetRequestViewModel query)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                var (workers, totalCount) = await _workerService.GetFilteredAsync(
                    query.SearchTerm, query.Fields, query.PageNumber, query.PageSize);

                if (workers == null || !workers.Any())
                {
                    throw new NotFoundException(string.Format(
                        Messages.NotFoundFromQuery,
                        query.SearchTerm ?? "null",
                        query.Fields ?? "null",
                        query.PageNumber,
                        query.PageSize));
                }

                return new GetResponseViewModel<object>
                {
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalCount = totalCount,
                    Items = workers
                };
            });
        }

        /// <summary>
        /// Récupère un employé spécifique par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant de l'employé.</param>
        /// <returns>L'employé correspondant à l'identifiant.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorker(int id)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
                await _workerService.GetByIdAsync(id));
        }

        /// <summary>
        /// Vérifie si un employé existe pour un email donné.
        /// </summary>
        /// <param name="email">Email de l'employé.</param>
        /// <returns>Un objet indiquant si l'employé existe ou non.</returns>
        [HttpGet("exists/{email}")]
        public async Task<IActionResult> WorkerExists(string email)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
                new ExistsResponseViewModel
                {
                    Exists = await _workerService.ExistsAsync(email)
                });
        }


        /// <summary>
        /// Met à jour un employé existant.
        /// </summary>
        /// <param name="id">Identifiant de l'employé.</param>
        /// <param name="model">Données à mettre à jour pour l'employé.</param>
        /// <returns>Réponse indiquant le succès de l'opération.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWorker(int id, [FromBody] WorkerUpsertRequestViewModel model)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                // Conversion du ViewModel en modèle d'entité
                var worker = new Worker
                {
                    Id = id,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneFixed = model.PhoneFixed,
                    PhoneMobile = model.PhoneMobile,
                    Email = model.Email,
                    ServiceId = model.ServiceId,
                    LocationId = model.LocationId
                };

                await _workerService.UpdateAsync(id, worker);
                return (object?)null; // Indique que l'opération n'a pas de contenu à retourner.
            });
        }

        /// <summary>
        /// Supprime un employé existant.
        /// </summary>
        /// <param name="id">Identifiant de l'employé à supprimer.</param>
        /// <returns>Réponse indiquant le succès de l'opération.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorker(int id)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                await _workerService.DeleteAsync(id);
                return (object?)null; // Indique que l'opération n'a pas de contenu à retourner.
            });
        }

        /// <summary>
        /// Transfère un ou plusieurs employés vers une nouvelle localisation et/ou un nouveau service.
        /// </summary>
        /// <param name="model">Données pour effectuer le transfert.</param>
        /// <returns>Résumé des résultats du transfert.</returns>
        [HttpPost("transfer")]
        public async Task<IActionResult> TransferWorkers([FromBody] WorkerTransferViewModel model)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                var (totalWorkers, successCount, errors) = await _workerService.TransferWorkersAsync(
                    model.WorkerIds, model.NewLocationId, model.NewServiceId, model.AllowPartialTransfer);

                return new TransferResponseViewModel
                {
                    TotalWorkers = totalWorkers,
                    SuccessCount = successCount,
                    Errors = errors.Select(error => new TransferErrorDetailResponseViewModel
                    {
                        WorkerId = error.WorkerId,
                        ErrorMessage = error.ErrorMessage
                    })
                };
            });
        }
    }
}