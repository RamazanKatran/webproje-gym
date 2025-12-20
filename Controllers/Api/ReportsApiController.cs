using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProjeGym.Data;
using WebProjeGym.Models;

namespace WebProjeGym.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Sadece admin raporları görebilir
    public class ReportsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ReportsApi/TrainerStatistics
        // Antrenör istatistikleri - En çok randevu alan antrenörler
        [HttpGet("TrainerStatistics")]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainerStatistics()
        {
            var statistics = await _context.Appointments
                .Include(a => a.Trainer)
                .GroupBy(a => new { a.TrainerId, a.Trainer.FirstName, a.Trainer.LastName })
                .Select(g => new
                {
                    trainerId = g.Key.TrainerId,
                    trainerName = $"{g.Key.FirstName} {g.Key.LastName}",
                    totalAppointments = g.Count(),
                    approvedAppointments = g.Count(a => a.Status == AppointmentStatus.Approved),
                    pendingAppointments = g.Count(a => a.Status == AppointmentStatus.Pending),
                    cancelledAppointments = g.Count(a => a.Status == AppointmentStatus.Cancelled),
                    totalRevenue = g.Where(a => a.Status == AppointmentStatus.Approved).Sum(a => a.Price)
                })
                .OrderByDescending(s => s.totalAppointments)
                .ToListAsync();

            return Ok(statistics);
        }

        // GET: api/ReportsApi/ServiceStatistics
        // Hizmet istatistikleri - En popüler hizmetler
        [HttpGet("ServiceStatistics")]
        public async Task<ActionResult<IEnumerable<object>>> GetServiceStatistics()
        {
            var statistics = await _context.Appointments
                .Include(a => a.Service)
                .GroupBy(a => new { a.ServiceId, a.Service.Name })
                .Select(g => new
                {
                    serviceId = g.Key.ServiceId,
                    serviceName = g.Key.Name,
                    totalAppointments = g.Count(),
                    approvedAppointments = g.Count(a => a.Status == AppointmentStatus.Approved),
                    totalRevenue = g.Where(a => a.Status == AppointmentStatus.Approved).Sum(a => a.Price),
                    averagePrice = g.Average(a => a.Price)
                })
                .OrderByDescending(s => s.totalAppointments)
                .ToListAsync();

            return Ok(statistics);
        }

        // GET: api/ReportsApi/MonthlyStatistics?year=2025&month=1
        // Aylık randevu istatistikleri
        [HttpGet("MonthlyStatistics")]
        public async Task<ActionResult<object>> GetMonthlyStatistics([FromQuery] int year, [FromQuery] int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var statistics = await _context.Appointments
                .Where(a => a.StartDateTime >= startDate && a.StartDateTime <= endDate)
                .GroupBy(a => a.Status)
                .Select(g => new
                {
                    status = g.Key.ToString(),
                    count = g.Count(),
                    totalRevenue = g.Where(a => a.Status == AppointmentStatus.Approved).Sum(a => a.Price)
                })
                .ToListAsync();

            var totalAppointments = await _context.Appointments
                .CountAsync(a => a.StartDateTime >= startDate && a.StartDateTime <= endDate);

            var totalRevenue = await _context.Appointments
                .Where(a => a.StartDateTime >= startDate 
                    && a.StartDateTime <= endDate 
                    && a.Status == AppointmentStatus.Approved)
                .SumAsync(a => a.Price);

            return Ok(new
            {
                year = year,
                month = month,
                totalAppointments = totalAppointments,
                totalRevenue = totalRevenue,
                statusBreakdown = statistics
            });
        }

        // GET: api/ReportsApi/DailyAppointments?date=2025-01-20
        // Belirli bir günün randevu detayları
        [HttpGet("DailyAppointments")]
        public async Task<ActionResult<IEnumerable<object>>> GetDailyAppointments([FromQuery] DateTime date)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Include(a => a.MemberProfile)
                    .ThenInclude(mp => mp.ApplicationUser)
                .Where(a => a.StartDateTime.Date == date.Date)
                .OrderBy(a => a.StartDateTime)
                .Select(a => new
                {
                    id = a.Id,
                    trainer = new
                    {
                        id = a.Trainer.Id,
                        fullName = a.Trainer.FullName
                    },
                    service = new
                    {
                        id = a.Service.Id,
                        name = a.Service.Name
                    },
                    member = a.MemberProfile.ApplicationUser != null ? new
                    {
                        email = a.MemberProfile.ApplicationUser.Email
                    } : null,
                    startDateTime = a.StartDateTime,
                    endDateTime = a.EndDateTime,
                    status = a.Status.ToString(),
                    price = a.Price
                })
                .ToListAsync();

            return Ok(new
            {
                date = date.Date,
                totalAppointments = appointments.Count,
                appointments = appointments
            });
        }
    }
}

