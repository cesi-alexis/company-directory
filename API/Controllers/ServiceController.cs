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
    /// Contrôleur pour gérer les opérations CRUD sur les items.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController(ICrudService<Service> serviceService) : ControllerBase
    {
        private readonly ICrudService<Service> _serviceService = serviceService;

        /// <summary>
        /// Crée un nouveau service.
        /// </summary>
        /// <param name="model">Données pour le service à créer.</param>
        /// <returns>Le service nouvellement créé.</returns>
        [HttpPost]
        public async Task<IActionResult> PostService([FromBody] ServiceUpsertRequestViewModel model)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                // Conversion du ViewModel en modèle d'entité
                var service = new Service
                {
                    Name = model.Name
                };

                return await _serviceService.CreateAsync(service);
            }, StatusCodes.Status201Created);
        }

        /// <summary>
        /// Récupère une liste paginée de services avec des champs optionnels et un terme de recherche.
        /// </summary>
        /// <param name="query">Paramètres de pagination et de filtrage.</param>
        /// <returns>Réponse paginée contenant les services.</returns>
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
        /// Récupère un service spécifique par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant du service.</param>
        /// <returns>Le service correspondant à l'identifiant.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetService(int id)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
                await _serviceService.GetByIdAsync(id));
        }

        /// <summary>
        /// Vérifie si un service existe pour un nom donné.
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
        /// Met à jour un service existant.
        /// </summary>
        /// <param name="id">Identifiant du service.</param>
        /// <param name="model">Données à mettre à jour pour le service.</param>
        /// <returns>Réponse indiquant le succès de l'opération.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutService(int id, [FromBody] ServiceUpsertRequestViewModel model)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                // Conversion du ViewModel en modèle d'entité
                var service = new Service
                {
                    Id = id,
                    Name = model.Name
                };

                await _serviceService.UpdateAsync(id, service);
                return (object?)null; // Indique que l'opération n'a pas de contenu à retourner.
            });
        }

        /// <summary>
        /// Supprime un service existant.
        /// </summary>
        /// <param name="id">Identifiant du service à supprimer.</param>
        /// <returns>Réponse indiquant le succès de l'opération.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                await _serviceService.DeleteAsync(id);
                return (object?)null; // Indique que l'opération n'a pas de contenu à retourner.
            });
        }
    }
}