
using CompanyDirectory.API.Common;
using Microsoft.EntityFrameworkCore;

namespace CompanyDirectory.API.Utils
{
    public static class ServiceUtils
    {
        public static async Task<(IEnumerable<object> Items, int TotalCount)> GetFilteredAsync<T>(
                                        IQueryable<T> query,
                                        string? fields = null,
                                        int pageNumber = 1,
                                        int pageSize = Constants.MAX_PAGES) where T : class
        {
            // Limiter la taille maximale des pages
            const int MaxPageSize = 100;
            pageSize = Math.Min(pageSize, MaxPageSize);

            // Vérifiez si la requête est déjà triée
            if (!query.Expression.ToString().Contains("OrderBy"))
            {
                // Appliquez un tri par défaut (exemple : par clé primaire ou un champ spécifique)
                query = query.OrderBy(e => EF.Property<object>(e, "Id"));
            }

            // Calculer le nombre total d'enregistrements
            var totalCount = await query.CountAsync();

            // Appliquer la pagination
            var paginatedQuery = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            // Récupérer les données paginées
            var items = await paginatedQuery.ToListAsync();

            // Appliquer le filtrage des champs
            var filteredItems = ApplyFieldFiltering(items, fields);

            return (filteredItems, totalCount);
        }

        public static IEnumerable<object> ApplyFieldFiltering<T>(IEnumerable<T> items, string? fields) where T : class
        {
            if (string.IsNullOrWhiteSpace(fields))
            {
                // Si aucun champ n'est spécifié, retourner tous les champs
                return items;
            }

            // Liste des champs demandés
            var fieldList = fields.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(f => f.Trim())
                                  .ToList();

            // Trier les champs validés pour garantir un ordre constant
            var validFields = typeof(T)
                .GetProperties()
                .Where(p => fieldList.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                .OrderBy(p => p.Name) // Tri des champs par nom
                .ToList();

            if (validFields.Count == 0)
            {
                throw new ArgumentException("Invalid field(s) specified.");
            }

            // Appliquer le filtrage des champs dynamiquement
            return items.Select(item =>
            {
                var result = new Dictionary<string, object>();

                foreach (var field in validFields)
                {
                    var value = field.GetValue(item);

                    // Inclure le champ uniquement si la valeur n'est pas null
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