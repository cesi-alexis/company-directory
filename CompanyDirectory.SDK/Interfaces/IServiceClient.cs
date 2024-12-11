using CompanyDirectory.Models.ViewsModels.Requests;
using CompanyDirectory.Models.ViewsModels.Responses;

namespace CompanyDirectory.SDK.Interfaces
{
    /// <summary>
    /// Interface générique pour les services client SDK. 
    /// Permet d'exposer les opérations CRUD de base et des fonctionnalités avancées.
    /// </summary>
    /// <typeparam name="TModel">Type du modèle manipulé par le service.</typeparam>
    /// <typeparam name="TUpsertViewModel">Type du modèle utilisé pour les opérations de création et mise à jour.</typeparam>
    public interface IServiceClient<TModel, TUpsertViewModel>
    {
        /// <summary>
        /// Crée un nouvel élément.
        /// </summary>
        /// <param name="model">Les données nécessaires pour créer l'élément.</param>
        /// <returns>L'élément nouvellement créé.</returns>
        Task<TModel> CreateAsync(TUpsertViewModel model);

        /// <summary>
        /// Récupère une liste paginée d'éléments avec des paramètres de recherche et de pagination.
        /// </summary>
        /// <param name="query">Les paramètres de la requête (terme de recherche, pagination, etc.).</param>
        /// <returns>Une réponse paginée contenant les éléments correspondants.</returns>
        Task<GetAllResponseViewModel<TModel>> GetAsync(GetAllRequestViewModel query);

        /// <summary>
        /// Récupère un élément spécifique par son identifiant.
        /// </summary>
        /// <param name="query">Modèle contenant l'identifiant de l'élément et les champs à inclure.</param>
        /// <returns>L'élément correspondant ou <c>null</c> s'il n'existe pas.</returns>
        Task<TModel?> GetAsync(GetRequestViewModel query);

        /// <summary>
        /// Vérifie si un modèle existe dans la base de données en fonction de son identifiant unique.
        /// </summary>
        /// <param name="id">L'identifiant unique du modèle à vérifier.</param>
        /// <returns>Un booléen indiquant si le modèle existe ou non.</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Vérifie si un modèle existe dans la base de données en fonction d'un identifiant textuel spécifique (ex. : nom ou email).
        /// </summary>
        /// <param name="identifier">L'identifiant textuel unique du modèle à vérifier.</param>
        /// <returns>Un booléen indiquant si le modèle existe ou non.</returns>
        Task<bool> ExistsAsync(string identifier);

        /// <summary>
        /// Met à jour un élément existant.
        /// </summary>
        /// <param name="id">L'identifiant de l'élément à mettre à jour.</param>
        /// <param name="model">Les nouvelles données pour l'élément.</param>
        Task UpdateAsync(int id, TUpsertViewModel model);

        /// <summary>
        /// Supprime un élément existant.
        /// </summary>
        /// <param name="id">L'identifiant de l'élément à supprimer.</param>
        Task DeleteAsync(int id);
    }
}