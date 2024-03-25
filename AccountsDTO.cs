using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LoginApp
{
    [XmlRoot("AccountCollection")]
    public class AccountsDTO
    {
        [XmlArray]
        public List<AccountDTO> Accounts { get; set; }

        public AccountsDTO() 
        {
            Accounts = new List<AccountDTO>();
        }
    }
}
