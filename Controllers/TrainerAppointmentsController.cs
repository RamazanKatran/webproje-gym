using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProjeGym.Data;
using WebProjeGym.Models;
using WebProjeGym.Helpers;

namespace WebProjeGym.Controllers
{
    [Authorize(Roles = "Trainer")]
    public class TrainerAppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TrainerAppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: TrainerAppointments
        public async Task<IActionResult> Index()
        {
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "trainer-appointments", hypothesisId = "H1", location = "TrainerAppointmentsController.Index:24", message = "Method entry", data = new { userAuthenticated = User?.Identity?.IsAuthenticated, userName = User?.Identity?.Name }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "trainer-appointments", hypothesisId = "H2", location = "TrainerAppointmentsController.Index:27", message = "User is null", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                return NotFound();
            }

            // #region agent log
            try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "trainer-appointments", hypothesisId = "H3", location = "TrainerAppointmentsController.Index:32", message = "User retrieved", data = new { userId = user.Id, userEmail = user.Email, userName = user.UserName }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion

            // Giriş yapan antrenörü bul - önce ApplicationUserId ile, bulunamazsa email ile
            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.ApplicationUserId == user.Id);

            // Eğer ApplicationUserId ile bulunamazsa, email'e göre arama yap (eski kayıtlar için)
            if (trainer == null)
            {
                // Email'e göre Trainer aramak için, önce tüm Trainer'ları kontrol et
                // Not: Trainer modelinde email yok, bu yüzden ApplicationUser üzerinden eşleştirme yapamayız
                // En iyi çözüm: ApplicationUserId'yi manuel olarak güncellemek
                // Şimdilik: Tüm Trainer'ları listele ve kullanıcıya seçtir
                var allTrainers = await _context.Trainers
                    .Where(t => t.ApplicationUserId == null)
                    .ToListAsync();
                
                // Eğer sadece 1 Trainer varsa ve ApplicationUserId null ise, otomatik eşleştir
                if (allTrainers.Count == 1)
                {
                    trainer = allTrainers.First();
                    trainer.ApplicationUserId = user.Id;
                    await _context.SaveChangesAsync();
                }
            }

            // #region agent log
            try 
            { 
                var allTrainers = await _context.Trainers.Select(t => new { t.Id, t.FirstName, t.LastName, t.ApplicationUserId }).ToListAsync();
                await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "trainer-appointments", hypothesisId = "H4", location = "TrainerAppointmentsController.Index:36", message = "Trainer lookup result", data = new { trainerFound = trainer != null, trainerId = trainer?.Id, trainerName = trainer != null ? $"{trainer.FirstName} {trainer.LastName}" : null, searchedUserId = user.Id, allTrainers = allTrainers }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); 
            } catch { }
            // #endregion

            if (trainer == null)
            {
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "trainer-appointments", hypothesisId = "H5", location = "TrainerAppointmentsController.Index:38", message = "Trainer not found", data = new { userId = user.Id, userEmail = user.Email }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                TempData["ErrorMessage"] = $"Antrenör bilgisi bulunamadı. Lütfen admin ile iletişime geçin. (Kullanıcı ID: {user.Id})";
                return RedirectToAction("Index", "Home");
            }

            // Bu antrenöre ait randevuları getir
            var appointments = await _context.Appointments
                .Include(a => a.MemberProfile)
                    .ThenInclude(mp => mp.ApplicationUser)
                .Include(a => a.Service)
                .Where(a => a.TrainerId == trainer.Id)
                .OrderByDescending(a => a.StartDateTime)
                .ToListAsync();

            return View(appointments);
        }

        // GET: TrainerAppointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.ApplicationUserId == user.Id);

            if (trainer == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.MemberProfile)
                    .ThenInclude(mp => mp.ApplicationUser)
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .FirstOrDefaultAsync(a => a.Id == id && a.TrainerId == trainer.Id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: TrainerAppointments/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.ApplicationUserId == user.Id);

            if (trainer == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.TrainerId == trainer.Id);

            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.Status == AppointmentStatus.Pending)
            {
                appointment.Status = AppointmentStatus.Approved;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Randevu onaylandı.";
            }
            else
            {
                TempData["ErrorMessage"] = "Sadece bekleyen randevular onaylanabilir.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: TrainerAppointments/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.ApplicationUserId == user.Id);

            if (trainer == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.TrainerId == trainer.Id);

            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.Status == AppointmentStatus.Pending)
            {
                appointment.Status = AppointmentStatus.Rejected;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Randevu reddedildi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Sadece bekleyen randevular reddedilebilir.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

