using Askify.BusinessLogicLayer.Interfaces;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Askify.BusinessLogicLayer.Services
{
    /// <summary>
    /// ML.NET based text classification service for categorizing consultation questions
    /// </summary>
    public class CategoryClassificationService : ICategoryClassificationService
    {
        private readonly MLContext _mlContext;
        private ITransformer? _model;
        private PredictionEngine<QuestionInput, CategoryPrediction>? _predictionEngine;
        private readonly object _lock = new object();

        // Define categories for consultation questions
        private static readonly string[] Categories = new[]
        {
            "Technology",
            "Health & Medicine", 
            "Legal",
            "Finance & Business",
            "Education",
            "Career & Employment",
            "Relationships",
            "Home & Living",
            "Travel",
            "Other"
        };

        // Training data - keyword patterns for each category
        private static readonly Dictionary<string, string[]> CategoryKeywords = new()
        {
            ["Technology"] = new[] { 
                "computer", "software", "programming", "code", "app", "website", "internet", 
                "database", "server", "network", "security", "hack", "bug", "error", "api",
                "mobile", "android", "ios", "windows", "linux", "mac", "cloud", "ai", 
                "machine learning", "data", "algorithm", "javascript", "python", "java",
                "html", "css", "react", "angular", "node", "sql", "git", "docker", "devops",
                "tech", "digital", "online", "cyber", "hardware", "laptop", "phone", "tablet"
            },
            ["Health & Medicine"] = new[] { 
                "health", "medical", "doctor", "hospital", "medicine", "treatment", "symptom",
                "disease", "illness", "pain", "therapy", "mental", "anxiety", "depression",
                "diet", "nutrition", "exercise", "fitness", "weight", "sleep", "stress",
                "vaccine", "virus", "infection", "allergy", "surgery", "diagnosis", "prescription",
                "pharmacy", "nurse", "clinic", "patient", "blood", "heart", "brain", "cancer",
                "diabetes", "pregnancy", "dental", "skin", "eye", "ear", "headache", "fever"
            },
            ["Legal"] = new[] { 
                "law", "legal", "lawyer", "attorney", "court", "judge", "lawsuit", "sue",
                "contract", "agreement", "rights", "liability", "criminal", "civil", "divorce",
                "custody", "will", "estate", "property", "tenant", "landlord", "eviction",
                "copyright", "trademark", "patent", "immigration", "visa", "citizenship",
                "arrest", "police", "fine", "penalty", "regulation", "compliance", "license"
            },
            ["Finance & Business"] = new[] { 
                "money", "finance", "bank", "loan", "credit", "debt", "investment", "stock",
                "market", "business", "startup", "entrepreneur", "tax", "accounting", "budget",
                "salary", "income", "expense", "profit", "loss", "insurance", "mortgage",
                "retirement", "pension", "savings", "crypto", "bitcoin", "trading", "fund",
                "company", "corporation", "partnership", "marketing", "sales", "revenue",
                "client", "customer", "product", "service", "strategy", "management"
            },
            ["Education"] = new[] { 
                "school", "university", "college", "education", "learn", "study", "course",
                "class", "teacher", "professor", "student", "exam", "test", "grade", "degree",
                "diploma", "certificate", "scholarship", "tuition", "homework", "assignment",
                "research", "thesis", "dissertation", "academic", "curriculum", "textbook",
                "tutoring", "online learning", "training", "workshop", "seminar", "lecture"
            },
            ["Career & Employment"] = new[] { 
                "job", "career", "work", "employment", "hire", "fired", "resume", "cv",
                "interview", "salary", "promotion", "boss", "manager", "colleague", "office",
                "remote", "freelance", "contract", "benefits", "vacation", "leave", "hr",
                "human resources", "performance", "review", "skills", "experience", "internship",
                "qualification", "profession", "industry", "workplace", "employee", "employer"
            },
            ["Relationships"] = new[] { 
                "relationship", "dating", "marriage", "partner", "spouse", "husband", "wife",
                "boyfriend", "girlfriend", "family", "parent", "child", "sibling", "friend",
                "love", "breakup", "divorce", "conflict", "communication", "trust", "cheating",
                "jealousy", "wedding", "engagement", "counseling", "therapy", "support"
            },
            ["Home & Living"] = new[] { 
                "home", "house", "apartment", "rent", "buy", "sell", "moving", "renovation",
                "repair", "maintenance", "furniture", "decor", "garden", "kitchen", "bathroom",
                "bedroom", "cleaning", "organize", "storage", "utility", "electricity", "water",
                "heating", "cooling", "appliance", "neighbor", "community", "pet", "animal"
            },
            ["Travel"] = new[] { 
                "travel", "trip", "vacation", "holiday", "flight", "airline", "airport", "hotel",
                "booking", "reservation", "passport", "visa", "destination", "tour", "guide",
                "luggage", "baggage", "ticket", "itinerary", "sightseeing", "adventure", "cruise",
                "train", "bus", "car rental", "transportation", "abroad", "international", "domestic"
            }
        };

        public CategoryClassificationService()
        {
            _mlContext = new MLContext(seed: 42);
            InitializeModel();
        }

        private void InitializeModel()
        {
            lock (_lock)
            {
                // Create training data from keyword patterns
                var trainingData = GenerateTrainingData();
                var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

                // Build the ML pipeline
                var pipeline = _mlContext.Transforms.Text
                    .FeaturizeText("Features", nameof(QuestionInput.Text))
                    .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(QuestionInput.Category)))
                    .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

                // Train the model
                _model = pipeline.Fit(dataView);
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<QuestionInput, CategoryPrediction>(_model);
            }
        }

        private IEnumerable<QuestionInput> GenerateTrainingData()
        {
            var trainingData = new List<QuestionInput>();

            foreach (var category in CategoryKeywords)
            {
                foreach (var keyword in category.Value)
                {
                    // Generate multiple variations for each keyword
                    trainingData.Add(new QuestionInput 
                    { 
                        Text = $"How to {keyword}?", 
                        Category = category.Key 
                    });
                    trainingData.Add(new QuestionInput 
                    { 
                        Text = $"I need help with {keyword}", 
                        Category = category.Key 
                    });
                    trainingData.Add(new QuestionInput 
                    { 
                        Text = $"Question about {keyword}", 
                        Category = category.Key 
                    });
                    trainingData.Add(new QuestionInput 
                    { 
                        Text = $"Can you explain {keyword}?", 
                        Category = category.Key 
                    });
                    trainingData.Add(new QuestionInput 
                    { 
                        Text = $"{keyword} issue", 
                        Category = category.Key 
                    });
                    trainingData.Add(new QuestionInput 
                    { 
                        Text = $"Problem with {keyword}", 
                        Category = category.Key 
                    });
                }
            }

            // Add some "Other" category examples
            var otherExamples = new[] {
                "Random question", "Something else", "Not sure what category",
                "General inquiry", "Miscellaneous topic", "Various things",
                "Mixed question", "Unrelated topic", "Other stuff"
            };
            foreach (var example in otherExamples)
            {
                trainingData.Add(new QuestionInput { Text = example, Category = "Other" });
            }

            return trainingData;
        }

        public string ClassifyQuestion(string title, string description)
        {
            if (_predictionEngine == null)
            {
                return "Other";
            }

            try
            {
                // Combine title and description for better classification
                var combinedText = $"{title} {description}".ToLowerInvariant();
                
                var input = new QuestionInput { Text = combinedText };
                var prediction = _predictionEngine.Predict(input);
                
                return prediction.PredictedLabel ?? "Other";
            }
            catch
            {
                return "Other";
            }
        }

        public IEnumerable<string> GetAllCategories()
        {
            return Categories;
        }

        public void TrainModel()
        {
            InitializeModel();
        }
    }

    /// <summary>
    /// Input class for ML model
    /// </summary>
    public class QuestionInput
    {
        public string Text { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Output class for ML predictions
    /// </summary>
    public class CategoryPrediction
    {
        [ColumnName("PredictedLabel")]
        public string? PredictedLabel { get; set; }
        
        public float[]? Score { get; set; }
    }
}
