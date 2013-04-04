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
    public partial class ManageSpendings : PhoneApplicationPage
    {
        private Category cat = null;
        private SpendingDatasource.CategoryType catType = SpendingDatasource.CategoryType.Spending;

        public ManageSpendings()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string catName;
             //Decide is this for Goal management or Spending management
            var isGoalsStr = string.Empty;
            if (NavigationContext.QueryString.TryGetValue("goals", out isGoalsStr))
            {
                if (Boolean.Parse(isGoalsStr))
                {
                    catType = SpendingDatasource.CategoryType.Goal;
                }
            }

            if (NavigationContext.QueryString.TryGetValue("catName", out catName))
            {
                cat = SpendingDatasource.GetSpendingCategory(catType, catName);
                pageTitle.Text = catName.ToLower();
                if(cat != null)
                    SpendingSummary.ItemsSource = cat.SpendingHistory;
            }
        }

        // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            var appBarDeleteButton = new ApplicationBarIconButton(new Uri("/Assets/Images/appbar_delete.png", UriKind.Relative))
            {
                Text = AppResources.IconDelete
            };
            appBarDeleteButton.Click += AppBarDeleteButtonOnClick;

            ApplicationBar.Buttons.Add(appBarDeleteButton);

            var menuItem = new ApplicationBarMenuItem("about");
            menuItem.Click += menuItemOnClick;
            ApplicationBar.MenuItems.Add(menuItem);
        }

        private void menuItemOnClick(object sender, EventArgs eventArgs)
        {
            NavigationService.Navigate(new Uri("/Views/About.xaml", UriKind.Relative));
        }

        private void AppBarDeleteButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (cat != null)
            {
                cat.SpendingHistory.Remove((Spending)SpendingSummary.SelectedItem);
            }
        }
    }
}