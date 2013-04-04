using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpendTracker.Model
{
    public class Spending : INotifyPropertyChanged
    {
        private DateTime spendingTime;

        private float amount;

        private string description;

        public DateTime SpendingTime
        {
            get
            {
                return spendingTime;
            }
            set
            {
                spendingTime = value;
                OnPropertyChanged("SpendingTime");
            }
        }


        public string SpendingTimeText
        {
            get { return SpendingTime.ToShortDateString(); }
        }

        public string AmountText
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture,"{0:c}", Amount);
            }
        }

        public float Amount
        {
            get
            {
                return amount;
            }
            set
            {
                amount = value;
                OnPropertyChanged("Amount");
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }

        public IList<string> Validate()
        {
            var validationErrors = new List<string>();

            if (Amount <= 0)
            {
                validationErrors.Add("No need to record spending <= 0.");
            }

            if (SpendingTime == null)
            {
                validationErrors.Add("We need a spending time.");
            }

            return validationErrors;
        }

        public Spending()
        {
            this.Amount = 0;
            this.SpendingTime = DateTime.MinValue;
        }
        public Spending(DateTime date, float amount)
        {
            SpendingTime = date;
            Amount = amount;
        }

        public Spending(DateTime date, float amount, string descr)
        {
            SpendingTime = date;
            Amount = amount;
            Description = descr;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
