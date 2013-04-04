using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpendTracker.Model
{
    public class Category : INotifyPropertyChanged
    {
        private string categoryName;

        private float yLimit;

        private ObservableCollection<Spending> spendingHistory;

        public string CategoryName
        {
            get { return categoryName; }
            set
            {
                if (categoryName == value) return;
                categoryName = value;
                OnPropertyChanged("CategoryName");
            }
        }

        public string YearlyLimitText
        {
            get
            { return YearlyLimit.ToString("C"); }
        }

        public float YearlyLimit
        {
            get
            { return yLimit; }
            set
            {
                if (yLimit == value) return;
                yLimit = value;
                OnPropertyChanged("YearlyLimit");
            }
        }
        
        public Uri Image { get; set; }

        public string YearlyTotalText
        {
            get
            { return YearlyTotal.ToString("C"); }
        }

        public float YearlyTotal
        {
            get
            {
                if (SpendingHistory == null) return 0;

                //Total spending
                var totalSpending = SpendingHistory.Sum(spending => spending.Amount);

                return totalSpending;
            }
        }

        public string YearlyPercentage
        {
            get
            {
                if (Math.Abs(YearlyLimit - 0) > float.Epsilon) 
                {
                    var p = (YearlyTotal / YearlyLimit) * 100;
                    return String.Format("{0:0.##\\%}", p);
                }
                return String.Format("{0:0.##\\%}",0 ); ;
            }
        }

        public string YearlyPercentageColor
        {
            get 
            {
                return YearlyTotal > YearlyLimit ? "DarkRed" : "Chartreuse";
            }
        }

        public string YearlyTextColor
        {
            get
            {
                return YearlyTotal > YearlyLimit ? "DarkRed" : "Green";
            }
        }

        public string YearlyPercentageLine
        {
            get
            {
                if (Math.Abs(YearlyLimit - 0) > float.Epsilon)
                {
                    var p = (YearlyTotal / YearlyLimit) * 100 * 3;  //our line max is 300 = 100% so we *3 here
                    if (p >= 300) p = 300;
                    return String.Format("{0:0.#}", p);
                }
                return String.Format("{0:0.#}", 0); ;
            }
        }

        
        public string RestYearlyPercentageLine
        {
            get
            {
                var p = (300 - (YearlyTotal / YearlyLimit) * 100 * 3);  //300 - YearlyPercentageLine 
                if (p <= 0) p = 0;
                return String.Format("{0:0.#}", p); ;
            }
        }

        public ObservableCollection<Spending> SpendingHistory 
        {
            get { return spendingHistory ?? (spendingHistory = new ObservableCollection<Spending>()); }
            set
            {
                if (spendingHistory == value) return;
                spendingHistory = value;
                if (spendingHistory != null)
                {
                    spendingHistory.CollectionChanged += delegate
                    {
                        OnPropertyChanged("YearlyTotal");
                        OnPropertyChanged("YearlyTotalText");
                    };
                }
                OnPropertyChanged("YearlyTotal");
                OnPropertyChanged("YearlyTotalText");
            }
        }

        public Category()
        {
            this.CategoryName = null;
            this.YearlyLimit = 0;
            this.Image = null;
        }

        public Category(string name, float limit, Uri image)
        {
            this.CategoryName = name;
            this.YearlyLimit = limit;
            this.Image = image;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) 
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged
    }
}
