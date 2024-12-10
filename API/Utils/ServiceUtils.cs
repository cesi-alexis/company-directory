using CompanyDirectory.Common;
using Microsoft.EntityFrameworkCore;

namespace CompanyDirectory.API.Utils
{
    /// <summary>
    /// Utilitaire pour appliquer des opérations génériques sur des entités, comme le filtrage des champs et la pagination.
    /// </summary>
    public static class ServiceUtils
    {
        /// <summary>
        /// Applique un filtrage et une pagination sur une requête de base de données.
        /// </summary>
        /// <typeparam name="T">Le type des entités manipulées.</typeparam>
        /// <param name="query">La requête IQueryable à filtrer et paginer.</param>
        /// <param name="fields">Les champs spécifiques à inclure dans les résultats (facultatif).</param>
        /// <param name="pageNumber">Le numéro de la page à récupérer.</param>
        /// <param name="pageSize">Le nombre d'éléments par page.</param>
        /// <returns>Un tuple contenant les éléments filtrés et paginés ainsi que le nombre total d'éléments.</returns>
        /// <exception cref="ArgumentException">Lève une exception si les paramètres de pagination sont invalides.</exception>
        public static async Task<(IEnumerable<object> Items, int TotalCount)> GetFilteredAsync<T>(
                                        IQueryable<T> query,
                                        string? fields = null,
                                        int pageNumber = 1,
                                        int pageSize = Constants.MAX_PAGES) where T : class
        {
            const int MaxPageSize = 100; // Taille maximale autorisée pour une page.
            pageSize = Math.Min(pageSize, MaxPageSize); // Restreindre la taille de la page si elle dépasse la limite.

            // Appliquer un tri par défaut si aucun tri n'est spécifié dans la requête.
            if (!query.Expression.ToString().Contains("OrderBy"))
            {
                query = query.OrderBy(e => EF.Property<object>(e, "Id")); // Tri par la propriété "Id".
            }

            // Compter le nombre total d'éléments dans la requête.
            var totalCount = await query.CountAsync();

            // Vérification des paramètres de pagination.
            if (pageNumber <= 0 || pageSize <= 0)
            {
                throw new ArgumentException(Messages.PaginationInvalid); // Lève une exception si les paramètres sont invalides.
            }

            // Appliquer la pagination : ignorer les éléments des pages précédentes et limiter à la taille de page.
            var paginatedQuery = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            // Exécuter la requête pour récupérer les éléments paginés.
            var items = await paginatedQuery.ToListAsync();

            // Appliquer un filtrage des champs, si spécifié.
            var filteredItems = ApplyFieldFiltering(items, fields);

            return (filteredItems, totalCount);
        }

        /// <summary>
        /// Applique un filtrage des champs spécifiés sur une liste d'éléments.
        /// </summary>
        /// <typeparam name="T">Le type des entités manipulées.</typeparam>
        /// <param name="items">La liste des éléments sur lesquels appliquer le filtrage.</param>
        /// <param name="fields">Les champs spécifiques à inclure dans les résultats (facultatif).</param>
        /// <returns>Une liste d'objets contenant uniquement les champs spécifiés.</returns>
        /// <exception cref="ArgumentException">Lève une exception si aucun champ valide n'est spécifié.</exception>
        public static IEnumerable<object> ApplyFieldFiltering<T>(IEnumerable<T> items, string? fields) where T : class
        {
            if (string.IsNullOrWhiteSpace(fields))
            {
                return items; // Retourner tous les champs si aucun n'est spécifié.
            }

            // Liste des champs demandés.
            var fieldList = fields.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(f => f.Trim())
                                  .ToList();

            // Liste des champs valides dans le type T.
            var validFields = typeof(T)
                .GetProperties()
                .Where(p => fieldList.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                .OrderBy(p => p.Name)
                .ToList();

            // Identifier les champs invalides.
            var invalidFields = fieldList.Except(validFields.Select(f => f.Name), StringComparer.OrdinalIgnoreCase).ToList();

            // Si des champs invalides existent, lever une exception.
            if (invalidFields.Any())
            {
                var invalidFieldsMessage = string.Join(", ", invalidFields);
                throw new ArgumentException(string.Format(Messages.InvalidFieldName, invalidFieldsMessage));
            }

            // Retourner les éléments avec uniquement les champs demandés.
            return items.Select(item =>
            {
                var result = new Dictionary<string, object>();
                foreach (var field in validFields)
                {
                    var value = field.GetValue(item);
                    if (value != null)
                    {
                        result[field.Name] = value;
                    }
                }

                return result;
            });
        }
    }
}