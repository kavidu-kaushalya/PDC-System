using System.ComponentModel;

namespace PDC_System
{
    public class QuotationItem : INotifyPropertyChanged
    {
        private int? qty;
        private string description = "";
        private decimal? unitAmount;
        private decimal? totalAmount;
        private bool isTitle;

        public int? Qty
        {
            get => qty;
            set
            {
                if (qty != value)
                {
                    qty = value;
                    OnPropertyChanged(nameof(Qty));
                    if (!IsTitle && qty.HasValue && unitAmount.HasValue) RecalculateTotal();
                }
            }
        }

        public string Description
        {
            get => description;
            set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        public decimal? UnitAmount
        {
            get => unitAmount;
            set
            {
                if (unitAmount != value)
                {
                    unitAmount = value;
                    OnPropertyChanged(nameof(UnitAmount));
                    if (!IsTitle && qty.HasValue && unitAmount.HasValue) RecalculateTotal();
                }
            }
        }

        public decimal? TotalAmount
        {
            get => totalAmount;
            set
            {
                if (totalAmount != value)
                {
                    totalAmount = value;
                    OnPropertyChanged(nameof(TotalAmount));
                    if (!IsTitle && Qty.HasValue && Qty != 0 && totalAmount.HasValue)
                    {
                        unitAmount = totalAmount / Qty;
                        OnPropertyChanged(nameof(UnitAmount));
                    }
                }
            }
        }

        public bool IsTitle
        {
            get => isTitle;
            set
            {
                if (isTitle != value)
                {
                    isTitle = value;
                    OnPropertyChanged(nameof(IsTitle));
                }
            }
        }

        private void RecalculateTotal()
        {
            if (Qty.HasValue && UnitAmount.HasValue)
            {
                totalAmount = Qty * UnitAmount;
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
