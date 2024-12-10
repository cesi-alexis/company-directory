using CompanyDirectory.Common;

namespace CompanyDirectory.Models.ViewsModels.Requests
{
    /// <summary>
    /// Modèle de vue pour les requêtes paginées avec des champs spécifiques à inclure dans les réponses.
    /// </summary>
    public class GetRequestViewModel
    {
        /// <summary>
        /// Terme de recherche utilisé pour filtrer les résultats.
        /// Exemple : "John" pour rechercher des employés dont le nom ou l'email contient "John".
        /// 
        /// Si ce champ est vide ou null, aucun filtrage par recherche ne sera appliqué.
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Champs spécifiques à inclure dans la réponse, séparés par des virgules.
        /// Exemple : "Name,Email".
        /// 
        /// Cela permet d'optimiser les réponses en renvoyant uniquement les informations nécessaires
        /// au client, réduisant ainsi la taille des réponses et le temps de traitement.
        /// </summary>
        public string? Fields { get; set; }

        /// <summary>
        /// Numéro de la page à récupérer.
        /// Par défaut, cette valeur est définie sur 1, ce qui correspond à la première page.
        /// 
        /// Utilisé pour naviguer dans les résultats paginés. Par exemple, une valeur de 2
        /// signifie que vous récupérez les résultats de la deuxième page.
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Taille de la page, c'est-à-dire le nombre maximum d'éléments à inclure dans une réponse.
        /// La taille maximale autorisée est définie dans une constante (Constants.MAX_PAGES) pour
        /// éviter des charges excessives sur le serveur.
        /// 
        /// Par exemple, si PageSize = 10, chaque page contiendra au maximum 10 éléments.
        /// </summary>
        public int PageSize { get; set; } = Constants.MAX_PAGES;
    }
}