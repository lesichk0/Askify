namespace Askify.BusinessLogicLayer.Interfaces
{
    public interface ICategoryClassificationService
    {
        /// <summary>
        /// Classifies a consultation question based on title and description
        /// </summary>
        /// <param name="title">The consultation title</param>
        /// <param name="description">The consultation description</param>
        /// <returns>The predicted category</returns>
        string ClassifyQuestion(string title, string description);
        
        /// <summary>
        /// Gets all available categories
        /// </summary>
        /// <returns>List of category names</returns>
        IEnumerable<string> GetAllCategories();
        
        /// <summary>
        /// Trains the model with new data (for future use)
        /// </summary>
        void TrainModel();
    }
}
