namespace WebProjeGym.Helpers
{
    public static class DayOfWeekHelper
    {
        public static string GetTurkishName(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Sunday => "Pazar",
                DayOfWeek.Monday => "Pazartesi",
                DayOfWeek.Tuesday => "Salı",
                DayOfWeek.Wednesday => "Çarşamba",
                DayOfWeek.Thursday => "Perşembe",
                DayOfWeek.Friday => "Cuma",
                DayOfWeek.Saturday => "Cumartesi",
                _ => day.ToString()
            };
        }
    }
}

