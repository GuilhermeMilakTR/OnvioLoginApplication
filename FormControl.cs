using System.ComponentModel;

namespace LoginApp
{
    public class FormControl : INotifyPropertyChanged
    {
        private static FormControl formControl;

        private bool isAddingOrEditing;

        private bool accountSelected;
        public bool IsAddingOrEditing
        {
            get
            {
                return isAddingOrEditing;
            }
            set
            {
                if (value != isAddingOrEditing)
                {
                    isAddingOrEditing = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAddingOrEditing)));
                }
            }
        }

        public bool AccountSelected
        {
            get
            {
                return accountSelected;
            }
            set
            {
                if (value != accountSelected)
                {
                    accountSelected = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccountSelected)));
                }
            }
        }        

        public event PropertyChangedEventHandler PropertyChanged;

        private FormControl() { }

        public static FormControl Instance
        {
            get
            {
                return formControl ??= new FormControl();
            }
        }
    }
}