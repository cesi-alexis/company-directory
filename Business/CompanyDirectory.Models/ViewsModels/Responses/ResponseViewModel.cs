using CompanyDirectory.Common;

namespace CompanyDirectory.Models.ViewsModels.Responses
{
    /// <summary>
    /// Modèle générique pour structurer les réponses des contrôleurs de l'API.
    /// </summary>
    /// <typeparam name="T">Le type de l'objet de données inclus dans la réponse.</typeparam>
    public class ResponseViewModel<T>
    {
        /// <summary>
        /// Indique si l'opération a été réalisée avec succès.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Code de statut HTTP associé à la réponse.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Message décrivant l'état ou le résultat de l'opération.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Objet de données renvoyé dans la réponse.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Informations additionnelles ou métadonnées pour enrichir la réponse.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = [];

        /// <summary>
        /// Crée une instance réussie de ResponseViewModel.
        /// </summary>
        /// <param name="data">Les données à inclure dans la réponse.</param>
        /// <param name="message">Un message de succès optionnel.</param>
        /// <param name="statusCode">Code HTTP (par défaut : 200).</param>
        /// <param name="metadata">Métadonnées optionnelles.</param>
        /// <returns>Instance de ResponseViewModel avec succès défini sur true.</returns>
        public static ResponseViewModel<T> SuccessResponse(
            T? data = default,
            string message = Messages.Success,
            int statusCode = 200, // Success
            Dictionary<string, object>? metadata = null)
        {
            return new ResponseViewModel<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data,
                Metadata = metadata ?? []
            };
        }

        /// <summary>
        /// Crée une instance échouée de ResponseViewModel.
        /// </summary>
        /// <param name="message">Le message d'erreur.</param>
        /// <param name="statusCode">Code HTTP (par défaut : 400).</param>
        /// <param name="metadata">Métadonnées optionnelles.</param>
        /// <returns>Instance de ResponseViewModel avec succès défini sur false.</returns>
        public static ResponseViewModel<T> FailureResponse(
            string message,
            int statusCode = 400,
            Dictionary<string, object>? metadata = null)
        {
            return new ResponseViewModel<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Metadata = metadata ?? []
            };
        }
    }
}