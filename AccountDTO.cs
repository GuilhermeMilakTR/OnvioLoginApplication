using System.Xml.Serialization;

namespace LoginApp
{
    [XmlType("Account")]
    public class AccountDTO
    {
        [XmlIgnore]
        public int Code { get; set; }

        [XmlElement]
        public string CompanyId { get; set; }

        [XmlElement]
        public string ContactId { get; set; }

        [XmlElement]
        public string UserId { get; set; }

        [XmlElement]
        public string Username { get; set; }

        [XmlElement]
        public string Password { get; set; }

        [XmlElement]
        public string Environment { get; set; }
    }
}