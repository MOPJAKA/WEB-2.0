namespace WEB.API.DTO
{
    public class ReadersUpdateDTO
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateOnly BirthDayDate { get; set; }

        public char Gender { get; set; }

        public string EducationLevel { get; set; }
    }
}
