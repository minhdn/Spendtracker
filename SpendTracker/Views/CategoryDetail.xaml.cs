using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SpendTracker.Model;
using SpendTracker.Resources;

namespace SpendTracker.Views
{
    public partial class CategoryDetail : PhoneApplicationPage
    {
        private Category category = null;
        private bool isAddNewCat = false;
        private SpendingDatasource.CategoryType catType = SpendingDatasource.CategoryType.Spending;

        public CategoryDetail()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
        }

        // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            var appBarSaveButton = new ApplicationBarIconButton(new Uri("/Assets/Images/appbar_save.png", UriKind.Relative))
            {
                Text = AppResources.IconSave
            };
            appBarSaveButton.Click += AppBarSaveButtonOnClick;

            ApplicationBar.Buttons.Add(appBarSaveButton);

            var menuItem = new ApplicationBarMenuItem("about");
            menuItem.Click += menuItemOnClick;
            ApplicationBar.MenuItems.Add(menuItem);
        }

        private void menuItemOnClick(object sender, EventArgs eventArgs)
        {
            NavigationService.Navigate(new Uri("/Views/About.xaml", UriKind.Relative));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //Decide is this for Goal management or Spending management
            var isGoalsStr = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("goals", out isGoalsStr))
            {
                if (Boolean.Parse(isGoalsStr))
                {
                    catType = SpendingDatasource.CategoryType.Goal;
                }
            }

            //If we add new category 
            var isAdd = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("add", out isAdd))
            {
                if (Boolean.Parse(isAdd))
                {
                    isAddNewCat = true;
                    return;
                }
            }

            //If we want to edit existing category
            string catName;
            if (NavigationContext.QueryString.TryGetValue("catName", out catName))
            {
                //Assign category name
                TbCatName.Text = catName;
            }
            string yearLimit;
            if (NavigationContext.QueryString.TryGetValue("yearLimit", out yearLimit))
            {
                float fYearLimit = 0;
                float.TryParse(yearLimit, out fYearLimit);
                TbYearLimit.Text = fYearLimit.ToString("C");
            }

            if (!String.IsNullOrEmpty(catName))
            {
                category = SpendingDatasource.GetSpendingCategory(catType, catName);
            }
        }

        private void AppBarSaveButtonOnClick(object sender, EventArgs eventArgs)
        {
            bool updated = false;
            if (isAddNewCat)
            {
                //Add cagtegory
                float fYearLimit = 0;
                if (float.TryParse(TbYearLimit.Text.Trim(), NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out fYearLimit))
                {
                    SpendingDatasource.SetSpendingCategory(catType, TbCatName.Text.Trim(), fYearLimit,null, () => MessageBox.Show("Something went wrong!"));
                    updated = true;
                }
            }
            else
            {
                //Edit category
                if (category != null)
                {
                    category.CategoryName = TbCatName.Text.Trim();
                    float fYearLimit = 0;
                    if (float.TryParse(TbYearLimit.Text.Trim(), NumberStyles.Currency, CultureInfo.CurrentCulture.NumberFormat, out fYearLimit))
                    {
                        category.YearlyLimit = fYearLimit;
                        updated = true;
                    }
                    else
                    {
                        MessageBox.Show("Hmm something wrong with your input limit, please double check :(");
                    }
                }
            }

            if (updated)
            {
                var result = MessageBox.Show("Category updated!");

                //Go back to previous page after save
                if (result == MessageBoxResult.OK)
                {
                    if (catType == SpendingDatasource.CategoryType.Goal)
                    {
                        NavigationService.Navigate(new Uri("/Views/SpendingSummary.xaml?goals=true", UriKind.Relative));
                    }
                    else
                    {
                        NavigationService.Navigate(new Uri("/Views/SpendingSummary.xaml?goals=false", UriKind.Relative));
                    }

                    
                }
            }
            
        }
    }
}