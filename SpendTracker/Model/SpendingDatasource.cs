using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpendTracker.Model
{
    public class SpendingDatasource
    {

        public enum CategoryType
        {
            Spending,
            Goal
        }

        private const string SPENDING_KEY = "SpendTracker.Spend.";
        private const string SPENDING_CAT_NAME_KEY = "SpendTracker.Name";

        private const string GOALS_KEY = "SpendTracker.Goals.";
        private const string GOALS_CAT_NAME_KEY = "SpendTracker.Goals.Name";

        private const string CURRENT_SPENDING_DATETIME_KEY = "SpendTracker.Time";

        private static readonly IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;
        private static Category category;
        private static List<Category> categories;

        public static event EventHandler SpendingUpdated;


        /// <summary>
        /// Get all categories from IsolatedStorageSettings
        /// </summary>
        /// <returns></returns>
        public static List<Category> GetAllSpendingCategory(CategoryType catType)
        {
            var catNameKey = String.Empty;
            var elementKey = String.Empty;

            if (catType == CategoryType.Goal)
            {
                catNameKey = GOALS_CAT_NAME_KEY;
                elementKey = GOALS_KEY;
            }
            if (catType == CategoryType.Spending)
            {
                catNameKey = SPENDING_CAT_NAME_KEY;
                elementKey = SPENDING_KEY;
            }

            categories = new List<Category>();
            if (!appSettings.Contains(catNameKey))
            {
                return categories;
            }

            var catNames = (List<String>)appSettings[catNameKey];

            foreach (var catName in catNames)
            {
                var catKey = elementKey + catName;
                categories.Add((Category)appSettings[catKey]);
            }
           
            return categories;
        }

        public static String GetEmailBody()
        {
            var body = new StringBuilder();
            var cats = GetAllSpendingCategory(CategoryType.Spending);
            var goals = GetAllSpendingCategory(CategoryType.Goal);

            body.AppendLine("SPENDING SUMMARY\n");
            foreach (var cat in cats)
            {
                body.AppendLine(cat.CategoryName);
                body.AppendLine("Limit: " + cat.YearlyLimitText);
                body.AppendLine("Money spent: " + cat.YearlyTotalText);
                body.AppendLine();
            }

            body.AppendLine("GOALS SUMMARY\n");
            foreach (var g in goals)
            {
                body.AppendLine(g.CategoryName);
                body.AppendLine("Goals: " + g.YearlyLimitText);
                body.AppendLine("Deposited: " + g.YearlyTotalText);
                body.AppendLine();
            }

            return body.ToString();
        }

        /// <summary>
        /// Remvoe a specific category
        /// </summary>
        /// <param name="name"></param>
        /// <param name="errorCallback"></param>
        public static void RemoveSpendingCategory(CategoryType catType, string name, Action errorCallback)
        {
            var catNameKey = String.Empty;
            var elementKey = String.Empty;

            if (catType == CategoryType.Goal)
            {
                catNameKey = GOALS_CAT_NAME_KEY;
                elementKey = GOALS_KEY;
            }
            if (catType == CategoryType.Spending)
            {
                catNameKey = SPENDING_CAT_NAME_KEY;
                elementKey = SPENDING_KEY;
            }

            var categoryKey = elementKey + name;
            if (!appSettings.Contains(categoryKey))
            {
                errorCallback();
                return;
            }

            try
            {
                appSettings.Remove(categoryKey);
                var catNames = (List<String>)appSettings[catNameKey];
                catNames.Remove(name);

                appSettings.Save();
                NotifySpendingUpdated();
            }
            catch (IsolatedStorageException)
            {
                errorCallback();
            }
        }
        
        public static float GetTotalSpending(DateTime start)
        {
            float total = 0;
            //Update current datetime
            appSettings[CURRENT_SPENDING_DATETIME_KEY] = start;

            var cats = GetAllSpendingCategory(CategoryType.Spending);

            foreach (var cat in cats)
            {
                foreach (var spending in cat.SpendingHistory)
                {
                    if (spending.SpendingTime.CompareTo(start) >= 0)
                    {
                        total += spending.Amount;
                    }
                }
            }

            return total;
        }

        public static DateTime GetCurrentStartDateTime()
        {
            if (appSettings.Contains(CURRENT_SPENDING_DATETIME_KEY))
            {
                return (DateTime) appSettings[CURRENT_SPENDING_DATETIME_KEY];
            }
            else
            {
                return DateTime.MinValue;
            }
        }
        /// <summary>
        /// Set new category 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="limit"></param>
        /// <param name="icon"></param>
        /// <param name="errorCallback"></param>
        public static void SetSpendingCategory(CategoryType catType, string name, float limit, Uri icon, Action errorCallback)
        {
            var catNameKey = String.Empty;
            var elementKey = String.Empty;

            if (catType == CategoryType.Goal)
            {
                catNameKey = GOALS_CAT_NAME_KEY;
                elementKey = GOALS_KEY;
            }
            if (catType == CategoryType.Spending)
            {
                catNameKey = SPENDING_CAT_NAME_KEY;
                elementKey = SPENDING_KEY;
            }

            var categoryKey = elementKey + name;
            if (appSettings.Contains(categoryKey))
            {
                return;
            }

            try
            {
                var cat = new Category(name, limit, icon);
                appSettings[categoryKey] = cat;
                if (appSettings.Contains(catNameKey))
                {
                    var catNames = (List<String>)appSettings[catNameKey];
                    catNames.Add(name);
                }
                else
                {
                    //First time set SPENDING_CAT_NAME_KEY
                    var catNames = new List<String> {name};
                    appSettings[catNameKey] = catNames;
                }
                appSettings.Save();
                NotifySpendingUpdated();
            }
            catch (IsolatedStorageException)
            {
                errorCallback();
            }
        }

        /// <summary>
        /// get category from IsolatedStorageSettings
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Category GetSpendingCategory(CategoryType catType, string name)
        {
            var elementKey = String.Empty;

            if (catType == CategoryType.Goal)
            {
                elementKey = GOALS_KEY;
            }
            if (catType == CategoryType.Spending)
            {
                elementKey = SPENDING_KEY;
            }

            var categoryKey = elementKey + name;

            if (appSettings.Contains(categoryKey))
            {
                category = (Category) appSettings[categoryKey];
                return category;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Validates the specified spending and then, if it is valid, adds it to
        /// spendingHistory collection. 
        /// </summary>
        /// <param name="catName"></param>
        /// <param name="spending">The Spending to save.</param>
        /// <param name="errorCallback">The action to execute if the storage attempt fails.</param>
        /// <returns>The validation results.</returns>
        public static SaveResult SaveSpending(CategoryType catType, string catName, Spending spending, Action errorCallback)
        {
            var elementKey = String.Empty;

            if (catType == CategoryType.Goal)
            {
                elementKey = GOALS_KEY;
            }
            if (catType == CategoryType.Spending)
            {
                elementKey = SPENDING_KEY;
            }

            var saveResult = new SaveResult();
            var validationResults = spending.Validate();
            if (validationResults.Count > 0)
            {
                saveResult.SaveSuccessful = false;
                saveResult.ErrorMessages = validationResults;
            }
            else
            {
                saveResult.SaveSuccessful = true;
                try
                {
                    var cat = (Category)appSettings[elementKey + catName];
                    cat.SpendingHistory.Insert(0, spending);
                    appSettings.Save();
                    saveResult.SaveSuccessful = true;
                }
                catch (IsolatedStorageException)
                {
                    errorCallback();
                }
            }
            return saveResult;
        }

        private static void NotifySpendingUpdated() 
        {
            var handler = SpendingUpdated;
            if (handler != null) handler(null, null);
        }
    }
}
