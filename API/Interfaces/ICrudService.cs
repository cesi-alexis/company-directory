using CompanyDirectory.Common;
using CompanyDirectory.Models.Entities;

namespace CompanyDirectory.API.Interfaces
{
    public interface ICrudService<T>
    {
        /// <summary>
        /// Crée une nouvelle entité.
        /// </summary>
        Task<T> CreateAsync(T entity);

        /// <summary>
        /// Récupère une liste filtrée et paginée d'entités.
        /// </summary>
        Task<(IEnumerable<object>? Items, int TotalCount)> GetFilteredAsync(
            string? searchTerm = null,
            string? fields = null,
            int pageNumber = 1,
            int pageSize = Constants.MAX_PAGES);

        /// <summary>
        /// Récupère une entité par son identifiant numérique.
        /// </summary>
        Task<object?> GetAsync(int id, string? fields);

        /// <summary>
        /// Vérifie si une entité existe par son identifiant numérique.
        /// </summary>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Vérifie si une entité existe par un identifiant textuel.
        /// </summary>
        Task<bool> ExistsAsync(string identifier);

        /// <summary>
        /// Vérifie si un nom est en doublon pour une entitée.
        /// </summary>
        Task<bool> IsDuplicateAsync(string name, int? excludeId = null);

        /// <summary>
        /// Met à jour une entité existante.
        /// </summary>
        Task<bool> UpdateAsync(int id, T entity);

        /// <summary>
        /// Supprime une entité par son identifiant numérique.
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Valide une entité selon des règles spécifiques.
        /// </summary>
        void ValidateAsync(T entity);
    }
}