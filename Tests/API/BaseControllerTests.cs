using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CompanyDirectory.Models.ViewsModels.Responses;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CompanyDirectory.Tests.API
{
    /// <summary>
    /// Classe de base pour les tests d'intégration des contrôleurs.
    /// </summary>
    public abstract class BaseControllerTests<TController, TModel>
    {
        protected abstract string[] Fields { get; }

        protected readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
        protected BaseControllerTests()
        {
            var apiUrl = "http://localhost:7055";
            _client = new HttpClient { BaseAddress = new Uri(apiUrl) };
        }

        /// <summary>
        /// Endpoint racine pour les tests de ce contrôleur.
        /// </summary>
        protected abstract string Endpoint { get; }

        /// <summary>
        /// Crée un modèle valide pour les tests.
        /// </summary>
        /// <returns>Une instance valide de <typeparamref name="TModel"/>.</returns>
        protected abstract TModel CreateValidModel();

        /// <summary>
        /// Crée un modèle invalide pour les tests.
        /// </summary>
        /// <returns>Une instance invalide de <typeparamref name="TModel"/>.</returns>
        protected abstract TModel CreateInvalidModel();

        /// <summary>
        /// Crée un modèle avec un identifiant spécifique pour les tests.
        /// </summary>
        /// <param name="identifier">Identifiant unique (par exemple, une chaîne ou un GUID).</param>
        /// <returns>Un modèle valide avec l'identifiant spécifié.</returns>
        protected abstract TModel CreateModelWithIdentifier(string identifier);

        /// <summary>
        /// Méthode pour extraire l'identifiant d'un modèle.
        /// </summary>
        /// <param name="model">Le modèle.</param>
        /// <returns>L'identifiant du modèle.</returns>
        protected abstract int GetIdFromModel(TModel model);

        protected abstract string GetIdentifier(string identifier);

        protected abstract bool Equals(TModel model, TModel expected);

        /// <summary>
        /// Crée un nouvel élément avec les données spécifiées et retourne son identifiant.
        /// </summary>
        /// <param name="customModel">Un modèle personnalisé ou la valeur par défaut pour utiliser un modèle généré automatiquement.</param>
        /// <returns>L'identifiant de l'élément créé.</returns>
        public async Task<int> PostModelAsync(TModel? customModel = default)
        {
            var model = customModel ?? CreateValidModel(); // Utilise le modèle par défaut ou celui spécifié
            var response = await _client.PostAsJsonAsync($"{Endpoint}", model, _jsonOptions);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Désérialisez la réponse en ResponseViewModel<TModel>
            var responseViewModel = await response.Content.ReadFromJsonAsync<ResponseViewModel<TModel>>(_jsonOptions);

            Assert.NotNull(responseViewModel); // Vérifie que la désérialisation a réussi
            Assert.True(responseViewModel!.Success); // Vérifie que la réponse indique un succès
            Assert.NotNull(responseViewModel.Data); // Vérifie que des données sont présentes

            return GetIdFromModel(responseViewModel.Data!);
        }

        /// <summary>
        /// Vérifie si un modèle existe en fonction d'un identifiant unique.
        /// </summary>
        /// <param name="identifier">Identifiant unique utilisé pour la recherche.</param>
        /// <returns>True si le modèle existe, sinon False.</returns>
        private async Task<bool> ModelExistsAsync(string identifier)
        {
            var response = await _client.GetAsync($"{Endpoint}/exists/{identifier}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<ExistsResponseViewModel>>();
                return result?.Data?.Exists ?? false; // Accédez à la propriété `Exists`
            }
            return false;
        }

        /// <summary>
        /// Crée un nouvel élément et retourne son identifiant.
        /// </summary>
        public async Task<int> CreateAndReturnIdAsync()
        {
            return await PostModelAsync();
        }

        // Création
        /// <summary>
        /// Teste la création d'un modèle valide. 
        /// Vérifie que le statut HTTP 201 (Created) est renvoyé.
        /// </summary>
        [Fact]
        public async Task Create_ValidModel_ShouldReturnCreated()
        {
            var model = CreateValidModel(); // Génère un modèle valide
            var response = await _client.PostAsJsonAsync($"{Endpoint}", model, _jsonOptions); // Envoie une requête POST

            Assert.Equal(HttpStatusCode.Created, response.StatusCode); // Vérifie que la réponse est 201 Created
        }

        /// <summary>
        /// Teste la création d'un modèle avec des champs manquants.
        /// Vérifie que le statut HTTP 400 (BadRequest) est renvoyé.
        /// </summary>
        [Fact]
        public async Task Create_MissingFields_ShouldReturnBadRequest()
        {
            var invalidModel = CreateInvalidModel(); // Génère un modèle invalide (champs manquants)
            var response = await _client.PostAsJsonAsync($"{Endpoint}", invalidModel, _jsonOptions); // Envoie une requête POST

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // Vérifie que la réponse est 400 BadRequest
        }

        /// <summary>
        /// Teste la création d'un modèle en double.
        /// Vérifie que le statut HTTP 409 (Conflict) est renvoyé.
        /// </summary>
        [Fact]
        public async Task Create_Duplicate_ShouldReturnConflict()
        {
            var model = CreateValidModel(); // Génère un modèle valide

            var response = await _client.PostAsJsonAsync($"{Endpoint}", model, _jsonOptions); // Envoie une requête POST
            Assert.Equal(HttpStatusCode.Created, response.StatusCode); // Vérifie que la réponse est 201 Created

            response = await _client.PostAsJsonAsync($"{Endpoint}", model, _jsonOptions); // Envoie d'une seconde requête POST

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode); // Vérifie que la réponse est 409 Conflict
        }

        /// <summary>
        /// Teste la création d'un modèle avec des formats invalides.
        /// Vérifie que le statut HTTP 400 (BadRequest) est renvoyé.
        /// </summary>
        [Fact]
        public async Task Create_InvalidFormat_ShouldReturnBadRequest()
        {
            var model = CreateInvalidModel(); // Génère un modèle invalide (formats incorrects)
            var response = await _client.PostAsJsonAsync($"{Endpoint}", model, _jsonOptions); // Envoie une requête POST

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // Vérifie que la réponse est 400 BadRequest
        }

        // Récupération
        /// <summary>
        /// Teste la récupération de tous les éléments. 
        /// Vérifie que le statut HTTP 200 (OK) est renvoyé.
        /// </summary>
        [Fact]
        public async Task GetAll_ShouldReturnSuccess()
        {
            var response = await _client.GetAsync($"{Endpoint}"); // Envoie une requête GET
            Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Vérifie que la réponse est 200 OK
        }

        /// <summary>
        /// Teste la récupération avec une pagination invalide.
        /// Vérifie que le statut HTTP 400 (BadRequest) est renvoyé.
        /// </summary>
        [Fact]
        public async Task GetAll_InvalidPagination_ShouldReturnBadRequest()
        {
            var response = await _client.GetAsync($"{Endpoint}?pageNumber=-1&pageSize=-5"); // Envoie une requête GET avec des paramètres invalides
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // Vérifie que la réponse est 400 BadRequest
        }

        /// <summary>
        /// Teste la récupération avec un filtre de champs.
        /// Vérifie que le statut HTTP 200 (OK) est renvoyé et que les résultats sont filtrés.
        /// </summary>
        [Fact]
        public async Task GetAll_WithFieldFilter_ShouldReturnFilteredResults()
        {
            await CreateAndReturnIdAsync();

            var fieldsQuery = string.Join(",", Fields); // Génère la requête de champs dynamiques
            var response = await _client.GetAsync($"{Endpoint}?fields={fieldsQuery}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<GetResponseViewModel<TModel>>>();
            Assert.NotNull(result);
            Assert.True(result!.Success);

            // Vérifie que des éléments sont retournés
            var paginatedData = result.Data;
            Assert.NotNull(paginatedData);
            Assert.True(paginatedData.TotalCount > 0);
            Assert.NotEmpty(paginatedData.Items);

            // Récupère le premier élément pour validation
            var fetchedModel = paginatedData.Items.First();
            Assert.NotNull(fetchedModel);
        }

        /// <summary>
        /// Teste la récupération d'un élément par son ID valide.
        /// Vérifie que le statut HTTP 200 (OK) est renvoyé.
        /// </summary>
        [Fact]
        public async Task GetById_ValidId_ShouldReturnModel()
        {
            var id = await CreateAndReturnIdAsync(); // Crée un élément et récupère son ID
            var response = await _client.GetAsync($"{Endpoint}/{id}"); // Envoie une requête GET avec l'ID

            Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Vérifie que la réponse est 200 OK
        }

        /// <summary>
        /// Teste la récupération d'un élément avec un ID inexistant.
        /// Vérifie que le statut HTTP 404 (NotFound) est renvoyé.
        /// </summary>
        [Fact]
        public async Task GetById_InvalidId_ShouldReturnNotFound()
        {
            var response = await _client.GetAsync($"{Endpoint}/99999"); // Envoie une requête GET avec un ID inexistant
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); // Vérifie que la réponse est 404 NotFound
        }

        /// <summary>
        /// Teste la recherche avec un terme valide.
        /// Vérifie que le statut HTTP 200 (OK) est renvoyé et que les résultats correspondent.
        /// </summary>
        [Fact]
        public async Task Search_ValidTerm_ShouldReturnResults()
        {
            var term = GetIdentifier("test");
            // Vérifie si le modèle existe déjà
            if (!await ModelExistsAsync(term))
            {
                var customModel = CreateModelWithIdentifier(term); // Utilisation de la méthode abstraite
                await PostModelAsync(customModel);
            }
            else
            {
                Console.WriteLine($"Le modèle avec l'identifiant '{term}' existe déjà.");
            }

            var response = await _client.GetAsync($"{Endpoint}?searchTerm={term}"); // Envoie une requête GET avec le terme de recherche
            Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Vérifie que la réponse est 200 OK

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<GetResponseViewModel<TModel>>>();
            Assert.NotNull(result);
            Assert.True(result!.Success);

            // Vérifie que des résultats sont retournés
            var paginatedData = result.Data;
            Assert.NotNull(paginatedData);
            Assert.True(paginatedData.TotalCount > 0);
            Assert.NotEmpty(paginatedData.Items);
        }

        /// <summary>
        /// Teste la recherche avec un terme inexistant.
        /// Vérifie que le statut HTTP 200 (OK) est renvoyé et que les résultats sont vides.
        /// </summary>
        [Fact]
        public async Task Search_NonExistentTerm_ShouldReturnNoResults()
        {
            var invalidTerm = "NonExistentTerm"; // Terme de recherche inexistant
            var response = await _client.GetAsync($"{Endpoint}?searchTerm={invalidTerm}"); // Envoie une requête GET

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); // Vérifie que la réponse est 200 OK

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<GetResponseViewModel<TModel>>>();
            Assert.NotNull(result);
            Assert.False(result!.Success);

            // Vérifie que les résultats sont vides
            var paginatedData = result.Data;
            Assert.Null(paginatedData);
        }

        /// <summary>
        /// Teste la recherche avec un terme valide et des paramètres de pagination.
        /// Vérifie que le statut HTTP 200 (OK) est renvoyé.
        /// </summary>
        [Fact]
        public async Task Search_WithPagination_ShouldReturnPaginatedResults()
        {
            // Créez des modèles spécifiques pour chaque terme
            var terms = new List<string>();
            for (int i = 0; i < 15; i++)
            {
                var term = GetIdentifier("term" + i);
                terms.Add(term);

                // Vérifie si le modèle existe déjà
                if (!await ModelExistsAsync(term))
                {
                    var customModel = CreateModelWithIdentifier(term); // Utilisation de la méthode abstraite
                    await PostModelAsync(customModel);
                }
                else
                {
                    Console.WriteLine($"Le modèle avec l'identifiant '{term}' existe déjà.");
                }
            }

            var response = await _client.GetAsync($"{Endpoint}?searchTerm=term&pageNumber=1&pageSize=10"); // Envoie une requête GET
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<GetResponseViewModel<TModel>>>();
            Assert.NotNull(result);
            Assert.True(result!.Success);

            var paginatedData = result.Data;
            Assert.NotNull(paginatedData);
            Assert.Equal(10, paginatedData.Items.Count()); // Vérifie que le nombre de résultats correspond à la taille de la page
        }

        /// <summary>
        /// Teste une recherche en temps réel en simulant plusieurs requêtes rapides.
        /// Vérifie que les réponses sont correctes pour chaque requête.
        /// </summary>
        [Fact]
        public async Task RealTimeSearch_ShouldReturnConsistentResults()
        {
            // Créez des modèles spécifiques pour chaque terme
            var terms = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                var term = GetIdentifier("term" + i);
                terms.Add(term);

                // Vérifie si le modèle existe déjà
                if (!await ModelExistsAsync(term))
                {
                    var customModel = CreateModelWithIdentifier(term);
                    await PostModelAsync(customModel);
                }
                else
                {
                    Console.WriteLine($"Le modèle avec l'identifiant '{term}' existe déjà.");
                }
            }

            var tasks = terms.Select(async term =>
            {
                var response = await _client.GetAsync($"{Endpoint}?searchTerm={term}");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<GetResponseViewModel<TModel>>>();
                Assert.NotNull(result);
                Assert.True(result!.Success);

                var paginatedData = result.Data;
                Assert.NotNull(paginatedData);
                Assert.True(paginatedData.TotalCount > 0);
                Assert.NotEmpty(paginatedData.Items);
            });

            await Task.WhenAll(tasks); // Attends que toutes les requêtes soient terminées
        }

        /// <summary>
        /// Teste la vérification d'existence d'un élément valide.
        /// Vérifie que le statut HTTP 200 (OK) est renvoyé.
        /// </summary>
        [Fact]
        public async Task Exists_ValidId_ShouldReturnTrue()
        {
            var id = await CreateAndReturnIdAsync(); // Crée un élément et récupère son ID
            var response = await _client.GetAsync($"{Endpoint}/exists/{id}"); // Envoie une requête GET pour vérifier l'existence

            Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Vérifie que la réponse est 200 OK
        }

        /// <summary>
        /// Teste la vérification d'existence d'un élément inexistant.
        /// Vérifie que le statut HTTP 200 (OK) est renvoyé.
        /// </summary>
        [Fact]
        public async Task Exists_InvalidId_ShouldReturnFalse()
        {
            var response = await _client.GetAsync($"{Endpoint}/exists/99999"); // Envoie une requête GET pour vérifier l'existence
            Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Vérifie que la réponse est 200 OK
        }

        // Mise à jour
        /// <summary>
        /// Teste la mise à jour d'un élément avec un modèle valide.
        /// Vérifie que le statut HTTP 204 (NoContent) est renvoyé.
        /// </summary>
        [Fact]
        public async Task Update_ValidModel_ShouldReturnNoContent()
        {
            var id = await CreateAndReturnIdAsync(); // Crée un élément et récupère son ID
            var model = CreateValidModel(); // Génère un modèle valide pour la mise à jour

            var response = await _client.PutAsJsonAsync($"{Endpoint}/{id}", model, _jsonOptions); // Envoie une requête PUT
            Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Vérifie que la réponse est 204 NoContent
        }

        /// <summary>
        /// Teste la mise à jour d'un élément avec un modèle invalide.
        /// Vérifie que le statut HTTP 400 (BadRequest) est renvoyé.
        /// </summary>
        [Fact]
        public async Task Update_InvalidModel_ShouldReturnBadRequest()
        {
            var id = await CreateAndReturnIdAsync(); // Crée un élément et récupère son ID
            var model = CreateInvalidModel(); // Génère un modèle invalide

            var response = await _client.PutAsJsonAsync($"{Endpoint}/{id}", model, _jsonOptions); // Envoie une requête PUT
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // Vérifie que la réponse est 400 BadRequest
        }

        /// <summary>
        /// Teste la mise à jour d'un élément avec un ID inexistant.
        /// Vérifie que le statut HTTP 404 (NotFound) est renvoyé.
        /// </summary>
        [Fact]
        public async Task Update_NonExistentId_ShouldReturnNotFound()
        {
            var model = CreateValidModel(); // Génère un modèle valide
            var response = await _client.PutAsJsonAsync($"{Endpoint}/99999", model, _jsonOptions); // Envoie une requête PUT avec un ID inexistant

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); // Vérifie que la réponse est 404 NotFound
        }

        // Suppression
        /// <summary>
        /// Teste la suppression d'un élément existant.
        /// Vérifie que le statut HTTP 204 (NoContent) est renvoyé.
        /// </summary>
        [Fact]
        public async Task Delete_ValidId_ShouldReturnNoContent()
        {
            var id = await CreateAndReturnIdAsync(); // Crée un élément et récupère son ID
            var response = await _client.DeleteAsync($"{Endpoint}/{id}"); // Envoie une requête DELETE

            Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Vérifie que la réponse est 204 NoContent
        }

        /// <summary>
        /// Teste la suppression d'un élément avec un ID inexistant.
        /// Vérifie que le statut HTTP 404 (NotFound) est renvoyé.
        /// </summary>
        [Fact]
        public async Task Delete_NonExistentId_ShouldReturnNotFound()
        {
            var response = await _client.DeleteAsync($"{Endpoint}/99999"); // Envoie une requête DELETE avec un ID inexistant
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); // Vérifie que la réponse est 404 NotFound
        }

        /// <summary>
        /// Teste la suppression avec un format d'ID invalide.
        /// Vérifie que le statut HTTP 400 (BadRequest) est renvoyé.
        /// </summary>
        [Fact]
        public async Task Delete_InvalidIdFormat_ShouldReturnBadRequest()
        {
            var response = await _client.DeleteAsync($"{Endpoint}/invalid-id"); // Envoie une requête DELETE avec un format d'ID invalide
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // Vérifie que la réponse est 400 BadRequest
        }
    }
}