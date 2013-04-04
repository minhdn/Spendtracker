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
    public partial class ManageCategory : PhoneApplicationPage
    {
        private SpendingDatasource.CategoryType catType = SpendingDatasource.CategoryType.Spending;
        public ManageCategory()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
        }

        private void InitializeSpendingCategories()
        {
            CategorySummary.ItemsSource = SpendingDatasource.GetAllSpendingCategory(catType);
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

            InitializeSpendingCategories();
        }

        // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            var appBarEditButton = new ApplicationBarIconButton(new Uri("/Assets/Images/appbar_edit.png", UriKind.Relative))
            {
                Text = AppResources.IconEdit
            };
            appBarEditButton.Click += AppBarEditButtonOnClick;

            var appBarAddButton = new ApplicationBarIconButton(new Uri("/Assets/Images/appbar_add.png", UriKind.Relative))
            {
                Text = AppResources.IconAddCategory
            };
            appBarAddButton.Click += AppBarAddButtonOnClick;

            var appBarDeleteButton = new ApplicationBarIconButton(new Uri("/Assets/Images/appbar_delete.png", UriKind.Relative))
            {
                Text = AppResources.IconDelete
            };
            appBarDeleteButton.Click += AppBarDeleteButtonOnClick;

            ApplicationBar.Buttons.Add(appBarEditButton);
            ApplicationBar.Buttons.Add(appBarAddButton);
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
            var selectedItem = (Category)CategorySummary.SelectedItem;
            if (selectedItem != null)
            {
                SpendingDatasource.RemoveSpendingCategory(catType, selectedItem.CategoryName, () => MessageBox.Show(
                                                               "Something went wrong when trying to remove a category"));

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

        private void AppBarAddButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (catType == SpendingDatasource.CategoryType.Goal)
            {
                NavigationService.Navigate(new Uri("/Views/CategoryDetail.xaml?goals=true&add=true", UriKind.Relative));
            }
            else
            {
                NavigationService.Navigate(new Uri("/Views/CategoryDetail.xaml?goals=false&add=true", UriKind.Relative));
            }
            
        }

        private void AppBarEditButtonOnClick(object sender, EventArgs eventArgs)
        {
            var selectedItem = (Category)CategorySummary.SelectedItem;
            if (selectedItem != null)
            {
                if (catType == SpendingDatasource.CategoryType.Goal)
                {
                    NavigationService.Navigate(new Uri("/Views/CategoryDetail.xaml?goals=true&catName=" + HttpUtility.UrlEncode(selectedItem.CategoryName)
                                           + "&yearLimit=" + HttpUtility.UrlEncode(selectedItem.YearlyLimit.ToString()), UriKind.Relative));
                }
                else
                {
                    NavigationService.Navigate(new Uri("/Views/CategoryDetail.xaml?goals=false&catName=" + HttpUtility.UrlEncode(selectedItem.CategoryName)
                                           + "&yearLimit=" + HttpUtility.UrlEncode(selectedItem.YearlyLimit.ToString()), UriKind.Relative));
                }
            }

        }
    }
}