
using Microsoft.EntityFrameworkCore;

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

        // Valider que les champs demandés existent dans le modèle
        var validFields = typeof(T)
            .GetProperties()
            .Where(p => fieldList.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (!validFields.Any())
        {
            throw new ArgumentException("Invalid field(s) specified.");
        }

        // Appliquer le filtrage des champs dynamiquement
        return items.Select(item =>
        {
            var result = new Dictionary<string, object>();

            foreach (var field in validFields)
            {
                result[field.Name] = field.GetValue(item);
            }

            return result;
        });
    }

}
