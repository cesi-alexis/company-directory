using CompanyDirectory.API.Services;
using CompanyDirectory.API.Utils;
using CompanyDirectory.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using CompanyDirectory.Models.ViewsModels.Requests;
using CompanyDirectory.Models.ViewsModels.Responses;
using CompanyDirectory.API.Interfaces;
using CompanyDirectory.Common.Exceptions;
using CompanyDirectory.Common;

namespace CompanyDirectory.API.Controllers
{
    /// <summary>
    /// Contr�leur pour g�rer les op�rations CRUD sur les items.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController(ICrudService<Service> serviceService) : ControllerBase
    {
        private readonly ICrudService<Service> _serviceService = serviceService;

        /// <summary>
        /// Cr�e un nouveau service.
        /// </summary>
        /// <param name="model">Donn�es pour le service � cr�er.</param>
        /// <returns>Le service nouvellement cr��.</returns>
        [HttpPost]
        public async Task<IActionResult> PostService([FromBody] ServiceUpsertRequestViewModel model)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                // Conversion du ViewModel en mod�le d'entit�
                var service = new Service
                {
                    Name = model.Name
                };

                return await _serviceService.CreateAsync(service);
            }, StatusCodes.Status201Created);
        }

        /// <summary>
        /// R�cup�re une liste pagin�e de services avec des champs optionnels et un terme de recherche.
        /// </summary>
        /// <param name="query">Param�tres de pagination et de filtrage.</param>
        /// <returns>R�ponse pagin�e contenant les services.</returns>
        [HttpGet]
        public async Task<IActionResult> GetServices([FromQuery] GetRequestViewModel query)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                var (services, totalCount) = await _serviceService.GetFilteredAsync(
                    query.SearchTerm, query.Fields, query.PageNumber, query.PageSize);

                if (services == null || !services.Any())
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
                    Items = services
                };
            });
        }

        /// <summary>
        /// R�cup�re un service sp�cifique par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant du service.</param>
        /// <returns>Le service correspondant � l'identifiant.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetService(int id)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
                await _serviceService.GetByIdAsync(id));
        }

        /// <summary>
        /// V�rifie si un service existe pour un nom donn�.
        /// </summary>
        /// <param name="name">Nom du service.</param>
        /// <returns>Un objet indiquant si le service existe ou non.</returns>
        [HttpGet("exists/{name}")]
        public async Task<IActionResult> ServiceExists(string name)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
                new ExistsResponseViewModel
                {
                    Exists = await _serviceService.ExistsAsync(name)
                });
        }

        /// <summary>
        /// Met � jour un service existant.
        /// </summary>
        /// <param name="id">Identifiant du service.</param>
        /// <param name="model">Donn�es � mettre � jour pour le service.</param>
        /// <returns>R�ponse indiquant le succ�s de l'op�ration.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutService(int id, [FromBody] ServiceUpsertRequestViewModel model)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                // Conversion du ViewModel en mod�le d'entit�
                var service = new Service
                {
                    Id = id,
                    Name = model.Name
                };

                await _serviceService.UpdateAsync(id, service);
                return (object?)null; // Indique que l'op�ration n'a pas de contenu � retourner.
            });
        }

        /// <summary>
        /// Supprime un service existant.
        /// </summary>
        /// <param name="id">Identifiant du service � supprimer.</param>
        /// <returns>R�ponse indiquant le succ�s de l'op�ration.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                await _serviceService.DeleteAsync(id);
                return (object?)null; // Indique que l'op�ration n'a pas de contenu � retourner.
            });
        }
    }
}