using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using SpendTracker.Resources;
using SpendTracker.Model;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;


namespace SpendTracker.Views
{
    public partial class SpendingSummary : PhoneApplicationPage
    {
        private bool isGoals = false;

        public SpendingSummary()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
            InitializeSpendingCategories();
            UpdateLiveTile();
             
        }

        private void UpdateLiveTile()
        {
            var PrimaryTile = ShellTile.ActiveTiles.First();
            if (PrimaryTile != null)
            {
                var tile = new StandardTileData();

                tile.BackContent = String.Format("Since {0}: {1:C}", SpendingDatasource.GetCurrentStartDateTime().ToShortDateString()
                    , SpendingDatasource.GetTotalSpending(SpendingDatasource.GetCurrentStartDateTime())); 
                PrimaryTile.Update(tile);

            } 
        }

        private void InitializeSpendingCategories()
        {
            if (SpendingDatasource.GetAllSpendingCategory(SpendingDatasource.CategoryType.Spending).Count == 0)
            {
                //default categories list
                var categoryData = new List<Category>
                {
                    new Category("Eat Out", 4500, new Uri("/Assets/Images/CategoryIcons/eatout.png", UriKind.Relative)),
                    new Category("Grocery", 1500, new Uri("/Assets/Images/CategoryIcons/grocery.png", UriKind.Relative)),
                    new Category("Entertainment", 2000, new Uri("/Assets/Images/CategoryIcons/entertainment.png", UriKind.Relative)),
                    new Category("Transportation", 1000, new Uri("/Assets/Images/CategoryIcons/transportation.png", UriKind.Relative)),
                    new Category("Shopping", 2000, new Uri("/Assets/Images/CategoryIcons/shopping.png", UriKind.Relative)),
                    new Category("Housing", 18000, new Uri("/Assets/Images/CategoryIcons/housing.png", UriKind.Relative)),
                    new Category("Medical", 1500, new Uri("/Assets/Images/CategoryIcons/medical.png", UriKind.Relative)),
                    new Category("Education", 500, new Uri("/Assets/Images/CategoryIcons/education.png", UriKind.Relative)),
                    new Category("Communication", 2500, new Uri("/Assets/Images/CategoryIcons/communication.png", UriKind.Relative)),
                    new Category("Others", 1000, new Uri("/Assets/Images/CategoryIcons/misc.png", UriKind.Relative)),
                    new Category("Travel", 3000, new Uri("/Assets/Images/CategoryIcons/travel.png", UriKind.Relative))
                };
                foreach (var category in categoryData)
                {
                    SpendingDatasource.SetSpendingCategory(SpendingDatasource.CategoryType.Spending, category.CategoryName
                                                           , category.YearlyLimit
                                                           , category.Image
                                                           ,() =>
                                                           MessageBox.Show(
                                                               "Something went wrong when trying to initilaize default spending categories"));
                }
            }
            if (SpendingDatasource.GetAllSpendingCategory(SpendingDatasource.CategoryType.Goal).Count == 0 )
            {
                //default categories list
                var categoryData = new List<Category>
                {
                    new Category("Saving goals", 18000, new Uri("/Assets/Images/CategoryIcons/bank.png", UriKind.Relative))
                };
                foreach (var category in categoryData)
                {
                    SpendingDatasource.SetSpendingCategory(SpendingDatasource.CategoryType.Goal, category.CategoryName
                                                           , category.YearlyLimit
                                                           , category.Image
                                                           , () =>
                                                           MessageBox.Show(
                                                               "Something went wrong when trying to initilaize default goals"));
                }
            }
            YearSummary.ItemsSource = SpendingDatasource.GetAllSpendingCategory(SpendingDatasource.CategoryType.Spending);
            GoalsSummary.ItemsSource = SpendingDatasource.GetAllSpendingCategory(SpendingDatasource.CategoryType.Goal);
        }
       
        // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            var appBarAddButton = new ApplicationBarIconButton(new Uri("/Assets/Images/appbar_add.png", UriKind.Relative))
                {
                    Text = AppResources.IconAdd
                };
            appBarAddButton.Click += AppBarAddButtonOnClick;
            ApplicationBar.Buttons.Add(appBarAddButton);

            // Create a new button and set the text value to the localized string from AppResources.
            var appBarCategoryButton = new ApplicationBarIconButton(new Uri("/Assets/Images/appbar_category.png", UriKind.Relative))
                {
                    Text = AppResources.IconCategory
                };
            appBarCategoryButton.Click += AppBarCategoryButtonOnClick;
            ApplicationBar.Buttons.Add(appBarCategoryButton);

            var menuItemShare = new ApplicationBarMenuItem("share");
            menuItemShare.Click += menuItemShareOnClick;
            ApplicationBar.MenuItems.Add(menuItemShare);

            var menuItemAbout = new ApplicationBarMenuItem("about");
            menuItemAbout.Click += menuItemAboutOnClick;
            ApplicationBar.MenuItems.Add(menuItemAbout);
        }

        private void menuItemShareOnClick(object sender, EventArgs eventArgs)
        {
            var emailcomposer = new EmailComposeTask();
            emailcomposer.Subject = "SpendTracker";
            emailcomposer.Body = SpendingDatasource.GetEmailBody();
            emailcomposer.Show();
        }

        private void menuItemAboutOnClick(object sender, EventArgs eventArgs) 
        {
            NavigationService.Navigate(new Uri("/Views/About.xaml", UriKind.Relative));
        }

        private void AppBarCategoryButtonOnClick(object sender, EventArgs eventArgs)
        {
            NavigationService.Navigate( new Uri("/Views/ManageCategory.xaml?goals=" + isGoals, UriKind.Relative));
        }

        private void AppBarAddButtonOnClick(object sender, EventArgs eventArgs)
        {
            NavigationService.Navigate(new Uri("/Views/NewSpending.xaml?goals=" + isGoals, UriKind.Relative));
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
                    isGoals = true;
                    summaryPivot.SelectedItem = GoalsPivotItem;
                    var addBtn = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                    addBtn.Text = AppResources.IconAddGoal;
                }
                else
                {
                    isGoals = false;
                    summaryPivot.SelectedItem = YearPivotItem;
                    var addBtn = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                    addBtn.Text = AppResources.IconAdd;
                }
            }
        }

        private void Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem currentPivot = (PivotItem)summaryPivot.SelectedItem;
            if (currentPivot.Name.Equals("YearPivotItem"))
            {
                ApplicationBar.IsVisible = true;
                isGoals = false;
                var addBtn = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                addBtn.Text = AppResources.IconAdd;
            }
            else if (currentPivot.Name.Equals("GoalsPivotItem"))
            {
                isGoals = true;
                ApplicationBar.IsVisible = true;
                var addBtn = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                addBtn.Text = AppResources.IconAddGoal;
            }
            else if (currentPivot.Name.Equals("TotalPivotItem"))
            {
                isGoals = false;
                ApplicationBar.IsVisible = false;
                TbTotalSummary.Text = SpendingDatasource.GetTotalSpending(SpendingDatasource.GetCurrentStartDateTime()).ToString();
                TbSince.Text = SpendingDatasource.GetCurrentStartDateTime().ToShortDateString();
            }
        }

        private void GoalsSummary_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (Category)GoalsSummary.SelectedItem;
            if (selectedItem != null)
            {
                NavigationService.Navigate(new Uri("/Views/ManageSpendings.xaml?goals=" + isGoals + "&catName=" + HttpUtility.UrlEncode(selectedItem.CategoryName), UriKind.Relative));
            }

        }

        private void GoalsSummary_OnTap(object sender, GestureEventArgs e)
        {
            var selectedItem = (Category)GoalsSummary.SelectedItem;
            if (selectedItem != null)
            {
                NavigationService.Navigate(new Uri("/Views/ManageSpendings.xaml?goals=" + isGoals + "&catName=" + HttpUtility.UrlEncode(selectedItem.CategoryName), UriKind.Relative));
            }
        }

        private void Summary_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (Category)YearSummary.SelectedItem;
            if (selectedItem != null)
            {
                NavigationService.Navigate(new Uri("/Views/ManageSpendings.xaml?goals=" + isGoals + "&catName=" + HttpUtility.UrlEncode(selectedItem.CategoryName), UriKind.Relative));
            }

        }

        private void YearSummary_OnTap(object sender, GestureEventArgs e)
        {
            var selectedItem = (Category)YearSummary.SelectedItem;
            if (selectedItem != null)
            {
                NavigationService.Navigate(new Uri("/Views/ManageSpendings.xaml?goals=" + isGoals + "&catName=" + HttpUtility.UrlEncode(selectedItem.CategoryName), UriKind.Relative));
            }
        }

        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            if (DpTime.Value != null)
            {
                TbTotalSummary.Text = SpendingDatasource.GetTotalSpending((DateTime) DpTime.Value).ToString();
                TbSince.Text = DpTime.ValueString;
            }
        }
    }
}