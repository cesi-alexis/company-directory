using CompanyDirectory.API.Services;
using CompanyDirectory.API.Utils;
using Microsoft.AspNetCore.Mvc;
using CompanyDirectory.Models.ViewsModels.Requests;
using CompanyDirectory.Models.ViewsModels.Responses;
using CompanyDirectory.Models.Entities;
using CompanyDirectory.API.Interfaces;
using CompanyDirectory.Common.Exceptions;
using CompanyDirectory.Common;

namespace CompanyDirectory.API.Controllers
{
    /// <summary>
    /// Contrôleur pour gérer les opérations CRUD sur les localisations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController(ICrudService<Location> locationService) : ControllerBase
    {
        private readonly ICrudService<Location> _locationService = locationService;

        /// <summary>
        /// Crée une nouvelle localisation.
        /// </summary>
        /// <param name="model">Données pour la localisation à créer.</param>
        /// <returns>La localisation nouvellement créée.</returns>
        [HttpPost]
        public async Task<IActionResult> PostLocation([FromBody] LocationUpsertRequestViewModel model)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                // Conversion du ViewModel en modèle d'entité
                var location = new Location
                {
                    City = model.City
                };

                return await _locationService.CreateAsync(location);
            }, StatusCodes.Status201Created);
        }

        /// <summary>
        /// Récupère une liste paginée de localisations avec des champs optionnels et un terme de recherche.
        /// </summary>
        /// <param name="query">Paramètres de pagination et de filtrage.</param>
        /// <returns>Réponse paginée contenant les localisations.</returns>
        [HttpGet]
        public async Task<IActionResult> GetLocations([FromQuery] GetAllRequestViewModel query)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                var (locations, totalCount) = await _locationService.GetFilteredAsync(
                    query.SearchTerm, query.Fields, query.PageNumber, query.PageSize);

                if (locations == null || !locations.Any())
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
                    Items = locations
                };
            });
        }

        /// <summary>
        /// Récupère une localisation spécifique par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant de la localisation.</param>
        /// <returns>La localisation correspondant à l'identifiant.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocation([FromQuery] GetRequestViewModel query)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
                await _locationService.GetAsync(query.Id, query.Fields));
        }

        /// <summary>
        /// Vérifie si une localisation existe pour un identifiant donné.
        /// </summary>
        /// <param name="id">Identifiant due la localisation.</param>
        /// <returns>Un objet indiquant si la localisation existe ou non.</returns>
        [HttpGet("exists-by-id/{id:int}")]
        public async Task<IActionResult> LocationExistsById(int id)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
                new ExistsResponseViewModel
                {
                    Exists = await _locationService.ExistsAsync(id)
                });
        }

        /// <summary>
        /// Vérifie si une localisation existe pour une ville donnée.
        /// </summary>
        /// <param name="city">Nom de la ville.</param>
        /// <returns>Un objet indiquant si la localisation existe ou non.</returns>
        [HttpGet("exists/{city}")]
        public async Task<IActionResult> LocationExists(string city)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
                new ExistsResponseViewModel
                {
                    Exists = await _locationService.ExistsAsync(city)
                });
        }

        /// <summary>
        /// Met à jour une localisation existante.
        /// </summary>
        /// <param name="id">Identifiant de la localisation.</param>
        /// <param name="model">Données à mettre à jour pour la localisation.</param>
        /// <returns>Réponse indiquant le succès de l'opération.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocation(int id, [FromBody] LocationUpsertRequestViewModel model)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                // Conversion du ViewModel en modèle d'entité
                var location = new Location
                {
                    Id = id,
                    City = model.City
                };

                await _locationService.UpdateAsync(id, location);
                return (object?)null; // Indique que l'opération n'a pas de contenu à retourner.
            });
        }

        /// <summary>
        /// Supprime une localisation existante.
        /// </summary>
        /// <param name="id">Identifiant de la localisation à supprimer.</param>
        /// <returns>Réponse indiquant le succès de l'opération.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                await _locationService.DeleteAsync(id);
                return (object?)null; // Indique que l'opération n'a pas de contenu à retourner.
            });
        }
    }
}