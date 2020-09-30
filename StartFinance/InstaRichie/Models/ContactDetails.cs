using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;

namespace StartFinance.Models
{
    class ContactDetails
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [NotNull]
        public string FirstNameContact { get; set; }

        [NotNull]
        public string LastNameContact { get; set; }

        [NotNull]
        public string CompanyNameContact { get; set; }

        [NotNull, Unique]
        public string MobilePhoneContact { get; set; }
    }
}
