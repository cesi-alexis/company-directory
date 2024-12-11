using CompanyDirectory.Common;
using Microsoft.EntityFrameworkCore;

namespace CompanyDirectory.API.Utils
{
    /// <summary>
    /// Utilitaire pour appliquer la pagination sur une requête.
    /// </summary>
    public static class ServiceUtils
    {
        /// <summary>
        /// Applique une pagination sur une requête de base de données.
        /// </summary>
        /// <typeparam name="T">Le type des entités manipulées.</typeparam>
        /// <param name="query">La requête IQueryable à paginer.</param>
        /// <param name="pageNumber">Le numéro de la page à récupérer.</param>
        /// <param name="pageSize">Le nombre d'éléments par page.</param>
        /// <returns>Un tuple contenant les éléments paginés et le nombre total d'éléments.</returns>
        public static async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedResultAsync<T>(
            IQueryable<T> query,
            int pageNumber = 1,
            int pageSize = Constants.MAX_PAGES) where T : class
        {
            const int MaxPageSize = 100;
            pageSize = Math.Min(pageSize, MaxPageSize);

            if (pageNumber <= 0 || pageSize <= 0)
                throw new ArgumentException(Messages.PaginationInvalid);

            // S'assurer qu'il y ait un ordre. Si aucun OrderBy n'est appliqué, on ordonne par Id.
            if (!query.Expression.ToString().Contains("OrderBy"))
            {
                query = query.OrderBy(e => EF.Property<object>(e, "Id"));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}