using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProjeGym.Data;
using WebProjeGym.Models;

namespace WebProjeGym.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // API'ye erişim için giriş yapılmış olmalı
    public class TrainersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TrainersApi
        // Tüm antrenörleri listeleme
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainers()
        {
            var trainers = await _context.Trainers
                .Include(t => t.GymBranch)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .Include(t => t.TrainerAvailabilities)
                .Select(t => new
                {
                    id = t.Id,
                    firstName = t.FirstName,
                    lastName = t.LastName,
                    fullName = t.FullName,
                    specialization = t.Specialization,
                    bio = t.Bio,
                    gymBranch = t.GymBranch != null ? new
                    {
                        id = t.GymBranch.Id,
                        name = t.GymBranch.Name,
                        address = t.GymBranch.Address
                    } : null,
                    services = t.TrainerServices.Select(ts => new
                    {
                        id = ts.Service.Id,
                        name = ts.Service.Name,
                        durationMinutes = ts.Service.DurationMinutes,
                        price = ts.Service.Price
                    }).ToList(),
                    availabilities = t.TrainerAvailabilities.Select(ta => new
                    {
                        dayOfWeek = ta.DayOfWeek.ToString(),
                        startTime = ta.StartTime.ToString(@"hh\:mm"),
                        endTime = ta.EndTime.ToString(@"hh\:mm")
                    }).ToList()
                })
                .ToListAsync();

            return Ok(trainers);
        }

        // GET: api/TrainersApi/5
        // Belirli bir antrenörü getirme
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTrainer(int id)
        {
            var trainer = await _context.Trainers
                .Include(t => t.GymBranch)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .Include(t => t.TrainerAvailabilities)
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    id = t.Id,
                    firstName = t.FirstName,
                    lastName = t.LastName,
                    fullName = t.FullName,
                    specialization = t.Specialization,
                    bio = t.Bio,
                    gymBranch = t.GymBranch != null ? new
                    {
                        id = t.GymBranch.Id,
                        name = t.GymBranch.Name,
                        address = t.GymBranch.Address
                    } : null,
                    services = t.TrainerServices.Select(ts => new
                    {
                        id = ts.Service.Id,
                        name = ts.Service.Name,
                        durationMinutes = ts.Service.DurationMinutes,
                        price = ts.Service.Price
                    }).ToList(),
                    availabilities = t.TrainerAvailabilities.Select(ta => new
                    {
                        dayOfWeek = ta.DayOfWeek.ToString(),
                        startTime = ta.StartTime.ToString(@"hh\:mm"),
                        endTime = ta.EndTime.ToString(@"hh\:mm")
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (trainer == null)
            {
                return NotFound(new { message = "Antrenör bulunamadı." });
            }

            return Ok(trainer);
        }

        // GET: api/TrainersApi/Available?date=2025-01-20
        // Belirli bir tarihte uygun antrenörleri getirme
        [HttpGet("Available")]
        public async Task<ActionResult<IEnumerable<object>>> GetAvailableTrainers([FromQuery] DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;

            // O gün için müsaitlik tanımlı olan antrenörleri bul
            var availableTrainers = await _context.Trainers
                .Include(t => t.GymBranch)
                .Include(t => t.TrainerAvailabilities)
                .Include(t => t.Appointments)
                .Where(t => t.TrainerAvailabilities.Any(ta => ta.DayOfWeek == dayOfWeek))
                .Select(t => new
                {
                    id = t.Id,
                    firstName = t.FirstName,
                    lastName = t.LastName,
                    fullName = t.FullName,
                    specialization = t.Specialization,
                    gymBranch = t.GymBranch != null ? new
                    {
                        id = t.GymBranch.Id,
                        name = t.GymBranch.Name,
                        address = t.GymBranch.Address
                    } : null,
                    availabilities = t.TrainerAvailabilities
                        .Where(ta => ta.DayOfWeek == dayOfWeek)
                        .Select(ta => new
                        {
                            startTime = ta.StartTime.ToString(@"hh\:mm"),
                            endTime = ta.EndTime.ToString(@"hh\:mm")
                        }).ToList(),
                    // O gün için onaylanmış randevuları kontrol et
                    approvedAppointments = t.Appointments
                        .Where(a => a.StartDateTime.Date == date.Date && a.Status == AppointmentStatus.Approved)
                        .Select(a => new
                        {
                            startTime = a.StartDateTime.ToString("HH:mm"),
                            endTime = a.EndDateTime.ToString("HH:mm")
                        }).ToList()
                })
                .ToListAsync();

            return Ok(availableTrainers);
        }

        // GET: api/TrainersApi/ByGymBranch/5
        // Belirli bir spor salonuna ait antrenörleri getirme
        [HttpGet("ByGymBranch/{gymBranchId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainersByGymBranch(int gymBranchId)
        {
            var trainers = await _context.Trainers
                .Include(t => t.GymBranch)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .Where(t => t.GymBranchId == gymBranchId)
                .Select(t => new
                {
                    id = t.Id,
                    firstName = t.FirstName,
                    lastName = t.LastName,
                    fullName = t.FullName,
                    specialization = t.Specialization,
                    services = t.TrainerServices.Select(ts => new
                    {
                        id = ts.Service.Id,
                        name = ts.Service.Name
                    }).ToList()
                })
                .ToListAsync();

            return Ok(trainers);
        }
    }
}

