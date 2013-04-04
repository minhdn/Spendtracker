using System;
using System.Collections.Generic;
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
    public partial class NewSpending : PhoneApplicationPage
    {
        private SpendingDatasource.CategoryType catType = SpendingDatasource.CategoryType.Spending;
        public NewSpending()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
        }

        private void InitializeNewSpending()
        {
            LpCategory.ItemsSource = SpendingDatasource.GetAllSpendingCategory(catType);
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

            InitializeNewSpending();
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

        private void AppBarSaveButtonOnClick(object sender, EventArgs eventArgs)
        {
            var selectedCat = (Category) LpCategory.SelectedItem;
            float amount;

            if (selectedCat != null && float.TryParse(TbAmount.Text, out amount) && DpTime.Value != null)
            {
                var spending = new Spending((DateTime)DpTime.Value, amount, TbDescription.Text);
                SpendingDatasource.SaveSpending(catType, selectedCat.CategoryName, spending, () => MessageBox.Show(
                                                               "Something went wrong when trying to save this spending"));
            }

            //Get here means we are good so just navigate back to summary page dont annoy users :) 
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