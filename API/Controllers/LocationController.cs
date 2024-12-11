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
    /// Contr�leur pour g�rer les op�rations CRUD sur les localisations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController(ICrudService<Location> locationService) : ControllerBase
    {
        private readonly ICrudService<Location> _locationService = locationService;

        /// <summary>
        /// Cr�e une nouvelle localisation.
        /// </summary>
        /// <param name="model">Donn�es pour la localisation � cr�er.</param>
        /// <returns>La localisation nouvellement cr��e.</returns>
        [HttpPost]
        public async Task<IActionResult> PostLocation([FromBody] LocationUpsertRequestViewModel model)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                // Conversion du ViewModel en mod�le d'entit�
                var location = new Location
                {
                    City = model.City
                };

                return await _locationService.CreateAsync(location);
            }, StatusCodes.Status201Created);
        }

        /// <summary>
        /// R�cup�re une liste pagin�e de localisations avec des champs optionnels et un terme de recherche.
        /// </summary>
        /// <param name="query">Param�tres de pagination et de filtrage.</param>
        /// <returns>R�ponse pagin�e contenant les localisations.</returns>
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
        /// R�cup�re une localisation sp�cifique par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant de la localisation.</param>
        /// <returns>La localisation correspondant � l'identifiant.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocation([FromQuery] GetRequestViewModel query)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
                await _locationService.GetAsync(query.Id, query.Fields));
        }

        /// <summary>
        /// V�rifie si une localisation existe pour un identifiant donn�.
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
        /// V�rifie si une localisation existe pour une ville donn�e.
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
        /// Met � jour une localisation existante.
        /// </summary>
        /// <param name="id">Identifiant de la localisation.</param>
        /// <param name="model">Donn�es � mettre � jour pour la localisation.</param>
        /// <returns>R�ponse indiquant le succ�s de l'op�ration.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocation(int id, [FromBody] LocationUpsertRequestViewModel model)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                // Conversion du ViewModel en mod�le d'entit�
                var location = new Location
                {
                    Id = id,
                    City = model.City
                };

                await _locationService.UpdateAsync(id, location);
                return (object?)null; // Indique que l'op�ration n'a pas de contenu � retourner.
            });
        }

        /// <summary>
        /// Supprime une localisation existante.
        /// </summary>
        /// <param name="id">Identifiant de la localisation � supprimer.</param>
        /// <returns>R�ponse indiquant le succ�s de l'op�ration.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            return await ResponseUtils.HandleResponseAsync(async () =>
            {
                await _locationService.DeleteAsync(id);
                return (object?)null; // Indique que l'op�ration n'a pas de contenu � retourner.
            });
        }
    }
}