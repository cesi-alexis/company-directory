using CompanyDirectory.API.Contexts;
using CompanyDirectory.API.Interfaces;
using CompanyDirectory.API.Utils;
using CompanyDirectory.Common;
using CompanyDirectory.Common.Exceptions;
using CompanyDirectory.Models.Entities;
using CompanyDirectory.Models.ViewsModels.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace CompanyDirectory.API.Services
{
    /// <summary>
    /// Service pour gérer les opérations CRUD et le transfert d'employés.
    /// </summary>
    public class WorkerService(
        BusinessDirectoryDbContext context,
        IMemoryCache cache,
        IOptions<CacheSettings> cacheSettings,
        ICrudService<Service> serviceService,
        ICrudService<Location> locationService) : IWorkerService
    {
        private readonly BusinessDirectoryDbContext _context = context;
        private readonly IMemoryCache _cache = cache;
        private readonly CacheSettings _cacheSettings = cacheSettings.Value;
        private readonly ICrudService<Service> _serviceService = serviceService;
        private readonly ICrudService<Location> _locationService = locationService;

        /// <summary>
        /// Crée un nouvel employé.
        /// </summary>
        public async Task<Worker> CreateAsync(Worker worker)
        {
            ValidateAsync(worker);

            if (await IsDuplicateAsync(worker.Email))
                throw new ConflictException(string.Format(Messages.DuplicateEmail, worker.Email));

            if (!await _serviceService.ExistsAsync(worker.ServiceId))
                throw new NotFoundException(string.Format(Messages.ServiceNotFound, worker.ServiceId));

            if (!await _locationService.ExistsAsync(worker.LocationId))
                throw new NotFoundException(string.Format(Messages.LocationNotFound, worker.LocationId));

            _context.Workers.Add(worker);
            await _context.SaveChangesAsync();

            _cache.Remove("Workers_*");
            return worker;
        }

        /// <summary>
        /// Méthode interne commune pour récupérer des employés avec des critères dynamiques.
        /// </summary>
        /// <param name="searchTerm">Terme de recherche pour filtrer les employés.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse, séparés par des virgules.</param>
        /// <param name="pageNumber">Numéro de la page à récupérer.</param>
        /// <param name="pageSize">Nombre d'éléments par page.</param>
        /// <param name="locationId">Identifiant de la localisation (optionnel).</param>
        /// <param name="serviceId">Identifiant du service (optionnel).</param>
        /// <returns>Une liste paginée d'employés correspondant aux critères.</returns>
        private async Task<(IEnumerable<object>? Items, int TotalCount)> GetFilteredInternalAsync(
            string? searchTerm = null,
            string? fields = null,
            int pageNumber = 1,
            int pageSize = Constants.MAX_PAGES,
            int? locationId = null,
            int? serviceId = null)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                throw new ValidationException(Messages.PaginationInvalid);

            var query = _context.Workers.AsQueryable();

            // Filtrage par localisation et/ou service
            if (locationId.HasValue)
                query = query.Where(w => w.LocationId == locationId.Value);

            if (serviceId.HasValue)
                query = query.Where(w => w.ServiceId == serviceId.Value);

            // Filtrage par terme de recherche
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(w =>
                    EF.Functions.Like(w.FirstName, $"%{searchTerm}%") ||
                    EF.Functions.Like(w.LastName, $"%{searchTerm}%") ||
                    EF.Functions.Like(w.PhoneFixed, $"%{searchTerm}%") ||
                    EF.Functions.Like(w.PhoneMobile, $"%{searchTerm}%") ||
                    EF.Functions.Like(w.Email, $"%{searchTerm}%"));
            }

            // Applique un tri par défaut
            query = query.OrderBy(w => w.Id);

            var totalCount = await query.CountAsync();

            // Pagination de base
            var skipped = (pageNumber - 1) * pageSize;

            if (!string.IsNullOrWhiteSpace(fields))
            {
                // Projection dynamique
                var projectedQuery = query.Select($"new ({fields})")
                                          .Skip(skipped)
                                          .Take(pageSize);

                var items = await projectedQuery.ToDynamicListAsync();
                var fieldsArray = fields.Split(',').Select(f => f.Trim()).ToArray();

                var responseItems = new List<WorkerGetResponseViewModel>();

                foreach (var item in items)
                {
                    dynamic d = item;
                    var vm = new WorkerGetResponseViewModel();

                    if (fieldsArray.Any(f => string.Equals(f, nameof(WorkerGetResponseViewModel.Id), StringComparison.OrdinalIgnoreCase)))
                        vm.Id = (int?)d.Id;
                    if (fieldsArray.Any(f => string.Equals(f, nameof(WorkerGetResponseViewModel.FirstName), StringComparison.OrdinalIgnoreCase)))
                        vm.FirstName = (string?)d.FirstName;
                    if (fieldsArray.Any(f => string.Equals(f, nameof(WorkerGetResponseViewModel.LastName), StringComparison.OrdinalIgnoreCase)))
                        vm.LastName = (string?)d.LastName;
                    if (fieldsArray.Any(f => string.Equals(f, nameof(WorkerGetResponseViewModel.Email), StringComparison.OrdinalIgnoreCase)))
                        vm.Email = (string?)d.Email;
                    if (fieldsArray.Any(f => string.Equals(f, nameof(WorkerGetResponseViewModel.PhoneMobile), StringComparison.OrdinalIgnoreCase)))
                        vm.PhoneMobile = (string?)d.PhoneMobile;
                    if (fieldsArray.Any(f => string.Equals(f, nameof(WorkerGetResponseViewModel.PhoneFixed), StringComparison.OrdinalIgnoreCase)))
                        vm.PhoneFixed = (string?)d.PhoneFixed;
                    if (fieldsArray.Any(f => string.Equals(f, nameof(WorkerGetResponseViewModel.ServiceId), StringComparison.OrdinalIgnoreCase)))
                        vm.ServiceId = (int?)d.ServiceId;
                    if (fieldsArray.Any(f => string.Equals(f, nameof(WorkerGetResponseViewModel.LocationId), StringComparison.OrdinalIgnoreCase)))
                        vm.LocationId = (int?)d.LocationId;

                    responseItems.Add(vm);
                }

                return (responseItems, totalCount);
            }
            else
            {
                // Projection complète si aucun champ spécifique n'est demandé
                var fullItems = await query.Skip(skipped)
                                           .Take(pageSize)
                                           .Select(w => new WorkerGetResponseViewModel
                                           {
                                               Id = w.Id,
                                               FirstName = w.FirstName,
                                               LastName = w.LastName,
                                               Email = w.Email,
                                               PhoneMobile = w.PhoneMobile,
                                               PhoneFixed = w.PhoneFixed,
                                               ServiceId = w.ServiceId,
                                               LocationId = w.LocationId
                                           }).ToListAsync();

                return (fullItems, totalCount);
            }
        }

        /// <summary>
        /// Récupère une liste paginée d'employés avec des critères dynamiques.
        /// </summary>
        /// <param name="searchTerm">Terme de recherche pour filtrer les employés.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse, séparés par des virgules.</param>
        /// <param name="pageNumber">Numéro de la page à récupérer.</param>
        /// <param name="pageSize">Nombre d'éléments par page.</param>
        /// <returns>Une liste paginée d'employés avec les champs spécifiés.</returns>
        public async Task<(IEnumerable<object>? Items, int TotalCount)> GetFilteredAsync(
            string? searchTerm = null,
            string? fields = null,
            int pageNumber = 1,
            int pageSize = Constants.MAX_PAGES)
        {
            return await GetFilteredInternalAsync(searchTerm, fields, pageNumber, pageSize);
        }

        /// <summary>
        /// Récupère une liste paginée d'employés en filtrant par localisation et/ou service.
        /// </summary>
        /// <param name="searchTerm">Terme de recherche pour filtrer les employés.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse, séparés par des virgules.</param>
        /// <param name="pageNumber">Numéro de la page à récupérer.</param>
        /// <param name="pageSize">Nombre d'éléments par page.</param>
        /// <param name="locationId">Identifiant de la localisation (optionnel).</param>
        /// <param name="serviceId">Identifiant du service (optionnel).</param>
        /// <returns>Une liste paginée d'employés correspondant aux critères.</returns>
        public async Task<(IEnumerable<object>? Items, int TotalCount)> GetFilteredAsync(
            string? searchTerm = null,
            string? fields = null,
            int pageNumber = 1,
            int pageSize = Constants.MAX_PAGES,
            int? locationId = null,
            int? serviceId = null)
        {
            return await GetFilteredInternalAsync(searchTerm, fields, pageNumber, pageSize, locationId, serviceId);
        }

        /// <summary>
        /// Méthode interne commune pour récupérer un employé spécifique par son identifiant avec des filtres dynamiques.
        /// </summary>
        /// <param name="id">Identifiant de l'employé.</param>
        /// <param name="fields">Champs optionnels à inclure dans la réponse, séparés par des virgules.</param>
        /// <param name="locationId">Identifiant optionnel de la localisation pour filtrer.</param>
        /// <param name="serviceId">Identifiant optionnel du service pour filtrer.</param>
        /// <returns>L'employé correspondant aux critères ou null si non trouvé.</returns>
        private async Task<object?> GetWorkerInternalAsync(int id, string? fields = null, int? locationId = null, int? serviceId = null)
        {
            if (id <= 0)
                throw new ValidationException(Messages.InvalidId);

            var query = _context.Workers.Where(w => w.Id == id);

            // Ajout des filtres optionnels
            if (locationId.HasValue)
                query = query.Where(w => w.LocationId == locationId.Value);

            if (serviceId.HasValue)
                query = query.Where(w => w.ServiceId == serviceId.Value);

            // Projection dynamique avec des champs spécifiques
            if (!string.IsNullOrWhiteSpace(fields))
            {
                var projectedQuery = query.Select($"new ({fields})");
                return await projectedQuery.Cast<object>().FirstOrDefaultAsync();
            }

            // Si aucun champ spécifique n'est demandé, retourne une réponse complète
            return await query.Select(w => new WorkerGetResponseViewModel
            {
                Id = w.Id,
                FirstName = w.FirstName,
                LastName = w.LastName,
                PhoneFixed = w.PhoneFixed,
                PhoneMobile = w.PhoneMobile,
                Email = w.Email,
                ServiceId = w.ServiceId,
                LocationId = w.LocationId
            }).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Récupère un employé spécifique par son identifiant avec des champs dynamiques.
        /// </summary>
        /// <param name="id">Identifiant de l'employé.</param>
        /// <param name="fields">Champs optionnels à inclure dans la réponse, séparés par des virgules.</param>
        /// <returns>L'employé correspondant ou null si non trouvé.</returns>
        public async Task<object?> GetAsync(int id, string? fields = null)
        {
            return await GetWorkerInternalAsync(id, fields);
        }

        /// <summary>
        /// Récupère un employé spécifique par son identifiant avec des champs dynamiques et des filtres optionnels sur la localisation et/ou le service.
        /// </summary>
        /// <param name="id">Identifiant de l'employé.</param>
        /// <param name="fields">Champs optionnels à inclure dans la réponse, séparés par des virgules.</param>
        /// <param name="locationId">Identifiant optionnel de la localisation pour filtrer.</param>
        /// <param name="serviceId">Identifiant optionnel du service pour filtrer.</param>
        /// <returns>L'employé correspondant ou null si non trouvé.</returns>
        public async Task<object?> GetAsync(int id, string? fields = null, int? locationId = null, int? serviceId = null)
        {
            return await GetWorkerInternalAsync(id, fields, locationId, serviceId);
        }

        /// <summary>
        /// Vérifie si un employé existe par ID.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Workers.AnyAsync(e => e.Id == id);
        }

        /// <summary>
        /// Vérifie si un employé existe pour un email donné.
        /// </summary>
        public async Task<bool> ExistsAsync(string email)
        {
            return await _context.Workers
                .AsNoTracking()
                .AnyAsync(s => s.Email.Trim().ToLower() == email.Trim().ToLower());
        }

        /// <summary>
        /// Vérifie si un email est en doublon pour un employé.
        /// </summary>
        public async Task<bool> IsDuplicateAsync(string email, int? excludeId = null)
        {
            return await _context.Workers.AnyAsync(e =>
                e.Email == email && (!excludeId.HasValue || e.Id != excludeId.Value));
        }

        /// <summary>
        /// Met à jour un employé existant.
        /// </summary>
        public async Task<bool> UpdateAsync(int id, Worker worker)
        {
            if (id != worker.Id)
                throw new ValidationException(Messages.IdMismatch);

            ValidateAsync(worker);

            if (!await _serviceService.ExistsAsync(worker.ServiceId))
                throw new NotFoundException(string.Format(Messages.ServiceNotFound, worker.ServiceId));

            if (!await _locationService.ExistsAsync(worker.LocationId))
                throw new NotFoundException(string.Format(Messages.LocationNotFound, worker.LocationId));

            _context.Entry(worker).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _cache.Remove($"Worker_{id}");
                _cache.Remove("Workers_*");
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ExistsAsync(id))
                    throw new NotFoundException(string.Format(Messages.WorkerNotFound, id));
                throw;
            }
        }

        /// <summary>
        /// Supprime un employé existant.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var worker = await _context.Workers.FindAsync(id)
                ?? throw new NotFoundException(string.Format(Messages.WorkerNotFound, id));

            _context.Workers.Remove(worker);
            await _context.SaveChangesAsync();

            _cache.Remove($"Worker_{id}");
            _cache.Remove("Workers_*");
            return true;
        }

        /// <summary>
        /// Transfère un ou plusieurs employés vers une nouvelle localisation et/ou un nouveau service.
        /// </summary>
        /// <param name="workerIds">Liste des identifiants des employés à transférer.</param>
        /// <param name="newLocationId">Nouvelle localisation (optionnelle).</param>
        /// <param name="newServiceId">Nouveau service (optionnel).</param>
        /// <param name="allowPartialTransfer">Indique si les transferts partiels sont autorisés en cas d'erreur.</param>
        /// <returns>
        /// Tuple contenant :
        /// — le nombre total d'employés ciblés,
        /// — le nombre de transferts réussis,
        /// — une liste des erreurs rencontrées pour chaque employé.
        /// </returns>
        public async Task<(int TotalWorkers, int SuccessCount, List<(int WorkerId, string ErrorMessage)> Errors)> TransferWorkersAsync(
            List<int> workerIds, int? newLocationId, int? newServiceId, bool allowPartialTransfer)
        {
            var totalWorkers = workerIds.Count;
            var successCount = 0;
            var errors = new List<(int WorkerId, string ErrorMessage)>();

            // Récupérer les employés spécifiés avec une projection typée
            var workers = await _context.Workers
                .Where(w => workerIds.Contains(w.Id))
                .Select(w => new WorkerGetResponseViewModel
                {
                    Id = w.Id,
                    FirstName = w.FirstName,
                    LastName = w.LastName,
                    Email = w.Email,
                    PhoneFixed = w.PhoneFixed,
                    PhoneMobile = w.PhoneMobile,
                    LocationId = w.LocationId,
                    ServiceId = w.ServiceId
                })
                .ToListAsync();

            if (workers is null || !workers.Any())
                throw new ValidationException(Messages.ResourceNotFound);

            foreach (var workerViewModel in workers)
            {
                try
                {
                    // Convertit le WorkerGetResponseViewModel en entité Worker pour mise à jour
                    var worker = new Worker
                    {
                        Id = workerViewModel.Id ?? throw new InvalidOperationException(Messages.InvalidWorkerId),
                        FirstName = workerViewModel.FirstName ?? throw new InvalidOperationException(Messages.InvalidWorkerFirstName),
                        LastName = workerViewModel.LastName ?? throw new InvalidOperationException(Messages.InvalidWorkerLastName),
                        Email = workerViewModel.Email ?? throw new InvalidOperationException(Messages.InvalidWorkerEmail),
                        PhoneFixed = workerViewModel.PhoneFixed ?? throw new InvalidOperationException(Messages.InvalidWorkerPhoneFixed),
                        PhoneMobile = workerViewModel.PhoneMobile ?? throw new InvalidOperationException(Messages.InvalidWorkerPhoneMobile),
                        LocationId = workerViewModel.LocationId ?? 0, // Défaut à 0 si null
                        ServiceId = workerViewModel.ServiceId ?? 0   // Défaut à 0 si null
                    };

                    // Met à jour la localisation si spécifiée
                    if (newLocationId.HasValue)
                    {
                        worker.LocationId = newLocationId.Value;
                    }

                    // Met à jour le service si spécifié
                    if (newServiceId.HasValue)
                    {
                        worker.ServiceId = newServiceId.Value;
                    }

                    // Valide les données de l'employé
                    ValidateAsync(worker);

                    // Met à jour l'employé dans la base de données
                    await UpdateAsync(worker.Id, worker);

                    successCount++;
                }
                catch (Exception ex)
                {
                    if (!allowPartialTransfer)
                        throw;

                    // Ajoute l'erreur pour cet employé
                    errors.Add((WorkerId: workerViewModel.Id ?? 0, ErrorMessage: ex.Message));
                }
            }

            return (totalWorkers, successCount, errors);
        }

        /// <summary>
        /// Valide les champs d'un employé.
        /// </summary>
        public void ValidateAsync(Worker worker)
        {
            if (!Formats.IsValidName(worker.FirstName))
                throw new ValidationException(string.Format(Messages.InvalidNameFormat, worker.FirstName));

            if (!Formats.IsValidName(worker.LastName))
                throw new ValidationException(string.Format(Messages.InvalidNameFormat, worker.LastName));

            if (!Formats.IsValidEmail(worker.Email))
                throw new ValidationException(Messages.InvalidEmailFormat);

            if (!Formats.IsValidPhoneNumber(worker.PhoneFixed))
                throw new ValidationException(Messages.InvalidPhoneNumberFormat);

            if (!Formats.IsValidPhoneNumber(worker.PhoneMobile))
                throw new ValidationException(Messages.InvalidPhoneNumberFormat);
        }
    }
}