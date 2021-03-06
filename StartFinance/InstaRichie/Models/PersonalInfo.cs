﻿using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace StartFinance.Models
{
	/**
	 PersonalID, FirstName, LastName, DOB, Gender, Email Address, MobilePhone
	 */
    class PersonalInfo
    {
        [PrimaryKey, AutoIncrement]
        public int PersonalID { get; set; }
        [NotNull]
        public string FirstName { get; set; }
        [NotNull]
        public string LastName { get; set; }
        [NotNull]
        public DateTime DOB { get; set; }
        [NotNull]
        public string Gender { get; set; }
        [NotNull, Unique]
        public string Email { get; set; }
        [NotNull, Unique]
        public string MobilePhone { get; set; }

        public string BirthDate
        {
            get
            {
                return String.Format("{0:MM/dd/yyyy}", DOB);
            }
        }
    }
}
