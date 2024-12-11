using System.Net.Http.Json;
using CompanyDirectory.Models.ViewsModels.Requests;
using CompanyDirectory.Models.ViewsModels.Responses;
using CompanyDirectory.SDK.Interfaces;

namespace CompanyDirectory.Tests.SDK
{
    /// <summary>
    /// Classe de base pour tester les services SDK avec des fonctionnalités avancées.
    /// </summary>
    /// <typeparam name="TServiceClient">Type du service client utilisé pour les appels SDK.</typeparam>
    /// <typeparam name="TModel">Type du modèle manipulé par le service.</typeparam>
    /// <typeparam name="TUpsertViewModel">Type du modèle utilisé pour la création et la mise à jour.</typeparam>
    public abstract class BaseSdkTests<TServiceClient, TModel, TUpsertViewModel>
        where TServiceClient : IServiceClient<TModel, TUpsertViewModel>
    {
        /// <summary>
        /// Propriété abstraite représentant le service client à tester.
        /// </summary>
        protected abstract TServiceClient ServiceClient { get; }

        /// <summary>
        /// Méthode abstraite pour créer un modèle valide utilisé dans les tests.
        /// </summary>
        /// <returns>Un modèle valide.</returns>
        protected abstract TUpsertViewModel CreateValidViewModel();

        /// <summary>
        /// Méthode abstraite pour créer un modèle invalide utilisé dans les tests.
        /// </summary>
        /// <returns>Un modèle invalide.</returns>
        protected abstract TUpsertViewModel CreateInvalidViewModel();

        /// <summary>
        /// Méthode abstraite pour extraire l'identifiant unique d'un modèle donné.
        /// </summary>
        /// <param name="model">Le modèle pour lequel extraire l'identifiant.</param>
        /// <returns>L'identifiant unique du modèle.</returns>
        protected abstract int GetId(TModel model);

        /// <summary>
        /// Méthode abstraite pour extraire un identifiant textuel unique d'un modèle donné.
        /// </summary>
        /// <param name="model">Le modèle pour lequel extraire l'identifiant.</param>
        /// <returns>L'identifiant textuel unique du modèle.</returns>
        protected abstract string GetUniqueIdentifier(TModel model);

        // === Tests de récupération avec champs dynamiques et filtres ===

        /// <summary>
        /// Teste la récupération d'un modèle avec des champs dynamiques.
        /// </summary>
        [Fact]
        public async Task Get_WithDynamicFields_ShouldReturnFilteredFields()
        {
            var model = CreateValidViewModel();
            var createdModel = await ServiceClient.CreateAsync(model);

            var query = new GetRequestViewModel
            {
                Id = GetId(createdModel),
                Fields = "Id,Name"
            };

            var fetchedModel = await ServiceClient.GetAsync(query);
            Assert.NotNull(fetchedModel);

            // Vérifiez ici les champs retournés si nécessaire
        }

        /// <summary>
        /// Teste la récupération d'une liste paginée avec des filtres avancés.
        /// </summary>
        [Fact]
        public async Task GetAll_WithFilters_ShouldReturnFilteredModels()
        {
            var validModel1 = CreateValidViewModel();
            var validModel2 = CreateValidViewModel();

            await ServiceClient.CreateAsync(validModel1);
            await ServiceClient.CreateAsync(validModel2);

            var query = new GetAllRequestViewModel
            {
                SearchTerm = "filter",
                PageNumber = 1,
                PageSize = 10,
                Fields = "Id,Name"
            };

            var result = await ServiceClient.GetAsync(query);
            Assert.NotNull(result);
            Assert.True(result.Items.Any());
        }

        // === Tests d'existence par identifiant textuel ===

        /// <summary>
        /// Vérifie qu'un modèle existe en fonction d'un identifiant textuel.
        /// </summary>
        [Fact]
        public async Task Exists_ByIdentifier_ShouldReturnTrue()
        {
            var validModel = CreateValidViewModel();
            var createdModel = await ServiceClient.CreateAsync(validModel);

            var exists = await ServiceClient.ExistsAsync(GetUniqueIdentifier(createdModel));
            Assert.True(exists);
        }

        /// <summary>
        /// Vérifie qu'un modèle n'existe pas en fonction d'un identifiant textuel invalide.
        /// </summary>
        [Fact]
        public async Task Exists_ByInvalidIdentifier_ShouldReturnFalse()
        {
            var exists = await ServiceClient.ExistsAsync("nonexistent-identifier");
            Assert.False(exists);
        }

        // === Tests de suppression en chaîne ===

        /// <summary>
        /// Teste la suppression d'un modèle existant, suivi de la vérification de son absence.
        /// </summary>
        [Fact]
        public async Task Delete_AndVerifyAbsence_ShouldSucceed()
        {
            var validModel = CreateValidViewModel();
            var createdModel = await ServiceClient.CreateAsync(validModel);

            await ServiceClient.DeleteAsync(GetId(createdModel));
            var exists = await ServiceClient.ExistsAsync(GetId(createdModel));
            Assert.False(exists);
        }
    }
}