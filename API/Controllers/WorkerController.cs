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
    /// Contr�leur pour g�rer les op�rations CRUD sur les employ�s.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WorkerController(IWorkerService workerService) : ControllerBase
    {
        private readonly IWorkerService _workerService = workerService;

        /// <summary>
        /// Cr�e un nouvel employ�.
        /// </summary>
        /// <param name="model">Donn�es pour l'employ� � cr�er.</param>
        /// <returns>L'employ� nouvellement cr��.</returns>
        [HttpPost]
        public async Task<IActionResult> PostWorker([FromBody] WorkerUpsertRequestViewModel model)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                // Conversion du ViewModel en mod�le d'entit�
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
        /// R�cup�re une liste pagin�e d'employ�s avec des champs dynamiques et des filtres optionnels.
        /// </summary>
        /// <param name="query">Param�tres de recherche, pagination, et filtres.</param>
        /// <returns>R�ponse pagin�e contenant les employ�s.</returns>
        [HttpGet]
        public async Task<IActionResult> GetWorkers([FromQuery] GetAllRequestViewModel query)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                // Appelle le service pour r�cup�rer les employ�s avec les filtres sp�cifi�s
                var (workers, totalCount) = await _workerService.GetFilteredAsync(
                    searchTerm: query.SearchTerm,
                    fields: query.Fields,
                    pageNumber: query.PageNumber,
                    pageSize: query.PageSize,
                    locationId: query.LocationId,
                    serviceId: query.ServiceId);

                if (workers == null || !workers.Any())
                {
                    throw new NotFoundException(string.Format(
                        Messages.NotFoundFromQuery,
                        query.SearchTerm ?? "null",
                        query.Fields ?? "null",
                        query.PageNumber,
                        query.PageSize));
                }

                return new GetAllResponseViewModel<object>
                {
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalCount = totalCount,
                    Items = workers
                };
            });
        }

        /// <summary>
        /// R�cup�re un employ� sp�cifique par son identifiant avec des filtres optionnels sur la localisation et le service.
        /// </summary>
        /// <param name="query">Mod�le contenant les param�tres de la requ�te.</param>
        /// <returns>L'employ� correspondant aux crit�res sp�cifi�s.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorker([FromQuery] GetRequestViewModel query)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
                await _workerService.GetAsync(query.Id, query.Fields));
        }

        /// <summary>
        /// R�cup�re un employ� sp�cifique par son identifiant avec des filtres optionnels sur la localisation et le service.
        /// </summary>
        /// <param name="id">Identifiant unique de l'employ�.</param>
        /// <param name="fields">Liste des champs sp�cifiques � inclure dans la r�ponse (optionnel).</param>
        /// <param name="locationId">Identifiant de la localisation pour filtrer (optionnel).</param>
        /// <param name="serviceId">Identifiant du service pour filtrer (optionnel).</param>
        /// <returns>Une r�ponse contenant l'employ� correspondant aux crit�res sp�cifi�s ou un message d'erreur si non trouv�.</returns>
        [HttpGet("Localized/{id}")]
        public async Task<IActionResult> GetLocalizedWorker(
            [FromRoute] int id,
            [FromQuery] string? fields = null,
            [FromQuery] int? locationId = null,
            [FromQuery] int? serviceId = null)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
                await _workerService.GetAsync(id, fields, locationId, serviceId));
        }

        /// <summary>
        /// V�rifie si un emmpoy� existe pour un identifiant donn�.
        /// </summary>
        /// <param name="id">Identifiant de l'employ�.</param>
        /// <returns>Un objet indiquant si l'employ� existe ou non.</returns>
        [HttpGet("exists-by-id/{id:int}")]
        public async Task<IActionResult> WorkerExistsById(int id)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
                new ExistsResponseViewModel
                {
                    Exists = await _workerService.ExistsAsync(id)
                });
        }

        /// <summary>
        /// V�rifie si un employ� existe pour un email donn�.
        /// </summary>
        /// <param name="email">Email de l'employ�.</param>
        /// <returns>Un objet indiquant si l'employ� existe ou non.</returns>
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
        /// Met � jour un employ� existant.
        /// </summary>
        /// <param name="id">Identifiant de l'employ�.</param>
        /// <param name="model">Donn�es � mettre � jour pour l'employ�.</param>
        /// <returns>R�ponse indiquant le succ�s de l'op�ration.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWorker(int id, [FromBody] WorkerUpsertRequestViewModel model)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                // Conversion du ViewModel en mod�le d'entit�
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
                return (object?)null; // Indique que l'op�ration n'a pas de contenu � retourner.
            });
        }

        /// <summary>
        /// Supprime un employ� existant.
        /// </summary>
        /// <param name="id">Identifiant de l'employ� � supprimer.</param>
        /// <returns>R�ponse indiquant le succ�s de l'op�ration.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorker(int id)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                await _workerService.DeleteAsync(id);
                return (object?)null; // Indique que l'op�ration n'a pas de contenu � retourner.
            });
        }

        /// <summary>
        /// Transf�re un ou plusieurs employ�s vers une nouvelle localisation et/ou un nouveau service.
        /// </summary>
        /// <param name="model">Donn�es pour effectuer le transfert.</param>
        /// <returns>R�sum� des r�sultats du transfert.</returns>
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