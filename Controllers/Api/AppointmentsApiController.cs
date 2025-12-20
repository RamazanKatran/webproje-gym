using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProjeGym.Data;
using WebProjeGym.Models;

namespace WebProjeGym.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // API'ye erişim için giriş yapılmış olmalı
    public class AppointmentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/AppointmentsApi
        // Üye randevularını getirme (giriş yapan kullanıcının randevuları)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { message = "Kullanıcı bulunamadı." });
            }

            var memberProfile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            if (memberProfile == null)
            {
                return NotFound(new { message = "Üye profili bulunamadı." });
            }

            var appointments = await _context.Appointments
                .Include(a => a.Trainer)
                    .ThenInclude(t => t.GymBranch)
                .Include(a => a.Service)
                .Where(a => a.MemberProfileId == memberProfile.Id)
                .OrderByDescending(a => a.StartDateTime)
                .Select(a => new
                {
                    id = a.Id,
                    trainer = new
                    {
                        id = a.Trainer.Id,
                        firstName = a.Trainer.FirstName,
                        lastName = a.Trainer.LastName,
                        fullName = a.Trainer.FullName,
                        gymBranch = a.Trainer.GymBranch != null ? new
                        {
                            id = a.Trainer.GymBranch.Id,
                            name = a.Trainer.GymBranch.Name
                        } : null
                    },
                    service = new
                    {
                        id = a.Service.Id,
                        name = a.Service.Name,
                        durationMinutes = a.Service.DurationMinutes,
                        price = a.Service.Price
                    },
                    startDateTime = a.StartDateTime,
                    endDateTime = a.EndDateTime,
                    durationMinutes = a.DurationMinutes,
                    price = a.Price,
                    status = a.Status.ToString()
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // GET: api/AppointmentsApi/ByStatus?status=Pending
        // Duruma göre randevuları filtreleme
        [HttpGet("ByStatus")]
        public async Task<ActionResult<IEnumerable<object>>> GetAppointmentsByStatus([FromQuery] string status)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { message = "Kullanıcı bulunamadı." });
            }

            var memberProfile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            if (memberProfile == null)
            {
                return NotFound(new { message = "Üye profili bulunamadı." });
            }

            if (!Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            {
                return BadRequest(new { message = "Geçersiz durum değeri. (Pending, Approved, Cancelled, Rejected)" });
            }

            var appointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.MemberProfileId == memberProfile.Id && a.Status == statusEnum)
                .OrderByDescending(a => a.StartDateTime)
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
                    startDateTime = a.StartDateTime,
                    endDateTime = a.EndDateTime,
                    status = a.Status.ToString()
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // GET: api/AppointmentsApi/ByDateRange?startDate=2025-01-01&endDate=2025-01-31
        // Tarih aralığına göre randevuları filtreleme
        [HttpGet("ByDateRange")]
        public async Task<ActionResult<IEnumerable<object>>> GetAppointmentsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { message = "Kullanıcı bulunamadı." });
            }

            var memberProfile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            if (memberProfile == null)
            {
                return NotFound(new { message = "Üye profili bulunamadı." });
            }

            var appointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.MemberProfileId == memberProfile.Id
                    && a.StartDateTime.Date >= startDate.Date
                    && a.StartDateTime.Date <= endDate.Date)
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
                    startDateTime = a.StartDateTime,
                    endDateTime = a.EndDateTime,
                    status = a.Status.ToString()
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // GET: api/AppointmentsApi/Upcoming
        // Yaklaşan randevuları getirme
        [HttpGet("Upcoming")]
        public async Task<ActionResult<IEnumerable<object>>> GetUpcomingAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { message = "Kullanıcı bulunamadı." });
            }

            var memberProfile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            if (memberProfile == null)
            {
                return NotFound(new { message = "Üye profili bulunamadı." });
            }

            var now = DateTime.Now;
            var appointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.MemberProfileId == memberProfile.Id
                    && a.StartDateTime >= now
                    && a.Status != AppointmentStatus.Cancelled
                    && a.Status != AppointmentStatus.Rejected)
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
                    startDateTime = a.StartDateTime,
                    endDateTime = a.EndDateTime,
                    status = a.Status.ToString(),
                    daysUntil = (a.StartDateTime - now).Days
                })
                .ToListAsync();

            return Ok(appointments);
        }
    }
}

