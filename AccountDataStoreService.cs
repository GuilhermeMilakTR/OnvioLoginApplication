using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace LoginApp
{
    public static class AccountDataStoreService
    {
        private const string FileName = "StoredAccounts.xml";

        public static void Save(AccountsDTO accountsDTO)
        {
            using (StreamWriter writer = new StreamWriter(FileName))
            {
                XmlSerializer serializer = new XmlSerializer(accountsDTO.GetType(), string.Empty);
                serializer.Serialize(writer, accountsDTO);
            }
        }

        public static AccountsDTO Read()
        {
            try
            {
                using (XmlReader reader = XmlReader.Create(FileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(AccountsDTO));

                    object readAsObject = xmlSerializer.Deserialize(reader)!;
                    return readAsObject != null ? readAsObject as AccountsDTO ?? new AccountsDTO() : new AccountsDTO();
                }
            }
            catch
            {
                return new AccountsDTO();
            }
        }
    }

}