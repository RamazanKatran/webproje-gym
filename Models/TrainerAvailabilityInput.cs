using System.ComponentModel.DataAnnotations;

namespace WebProjeGym.Models
{
    public class TrainerAvailabilityInput
    {
        public DayOfWeek? DayOfWeek { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}

