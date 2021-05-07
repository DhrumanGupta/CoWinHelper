namespace CowinChecker
{
    public class Configuration
    {
        public int Interval { get; set; }
        public PersonData[] PersonData { get; set; }
    }

    public class PersonData
    {
        public string Name { get; set; }
        public int[] Districts { get; set; }
        public int[] PinCodes { get; set; }
        public int MinimumSeats { get; set; }
        public string[] CenterKeywords { get; set; }
        public string VaccineType { get; set; }
    }
}