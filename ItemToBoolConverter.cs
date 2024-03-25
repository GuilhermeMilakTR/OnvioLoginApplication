using System.Globalization;
using System.Windows.Data;

namespace LoginApp
{
    public class ItemToBoolConverter : IMultiValueConverter
    {
        private static readonly Dictionary<string, string[]> fields = new Dictionary<string, string[]>()
        {
            { "ICM", new string[] { "COMPANY_ID", "CONTACT_ID" } },
            { "ISS", new string[] { "COMPANY_ID", "CONTACT_ID", "USER_ID" } },
            { "SS", new string[] { "USERNAME", "PASSWORD" } }
        };

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool.TryParse(values[0].ToString(), out bool isAdding);
            string comboBoxItemTag = values[1].ToString()!;
            string textBoxTag = values[2].ToString()!;

            return isAdding || fields[comboBoxItemTag].Contains(textBoxTag);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}