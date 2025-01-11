using System;

namespace WEB.API.Models
{
    public class Readers
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime BirthDayDate { get; set; }

        public char Gender { get; set; }

        public string EducationLevel { get; set; }
    }
}
