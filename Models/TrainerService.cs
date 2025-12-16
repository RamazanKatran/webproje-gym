namespace WebProjeGym.Models
{
    // Antrenör ve Hizmet arasındaki çoktan-çoğa ilişki tablosu
    public class TrainerService
    {
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; }
    }
}


