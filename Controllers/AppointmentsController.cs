using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebProjeGym.Data;
using WebProjeGym.Models;
using WebProjeGym.Helpers;

namespace WebProjeGym.Controllers
{
    [Authorize(Roles = "Member")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var memberProfile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            if (memberProfile == null)
            {
                return RedirectToAction("Index", "MemberProfiles");
            }

            var appointments = await _context.Appointments
                .Include(a => a.Trainer)
                    .ThenInclude(t => t.GymBranch)
                .Include(a => a.Service)
                .Where(a => a.MemberProfileId == memberProfile.Id)
                .OrderByDescending(a => a.StartDateTime)
                .ToListAsync();

            return View(appointments);
        }

        // GET: Appointments/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var memberProfile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            if (memberProfile == null)
            {
                TempData["ErrorMessage"] = "Önce profil bilgilerinizi tamamlamanız gerekmektedir.";
                return RedirectToAction("Index", "MemberProfiles");
            }

            // Profil bilgileri eksikse uyarı ver
            if (!memberProfile.HeightCm.HasValue || !memberProfile.WeightKg.HasValue || string.IsNullOrEmpty(memberProfile.Goal))
            {
                TempData["ErrorMessage"] = "Randevu alabilmek için profil bilgilerinizi (boy, kilo, hedef) tamamlamanız gerekmektedir.";
                return RedirectToAction("Index", "MemberProfiles");
            }

            // Antrenörler dropdown için
            ViewData["TrainerId"] = new SelectList(
                _context.Trainers
                    .Include(t => t.GymBranch)
                    .Select(t => new { 
                        Id = t.Id, 
                        Name = $"{t.FirstName} {t.LastName} - {t.GymBranch.Name}" 
                    }), 
                "Id", "Name");

            // Hizmetler dropdown için (ilk başta boş, antrenör seçilince doldurulacak)
            ViewData["ServiceId"] = new SelectList(new List<Service>(), "Id", "Name");

            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H1", location = "AppointmentsController.Create:96", message = "Create POST entry", data = new { trainerId = appointment.TrainerId, serviceId = appointment.ServiceId, appointmentDate = appointment.AppointmentDate?.ToString(), appointmentTime = appointment.AppointmentTime?.ToString(), memberProfileId = appointment.MemberProfileId, durationMinutes = appointment.DurationMinutes, modelStateIsValid = ModelState.IsValid, modelStateErrors = ModelState.Where(x => x.Value?.Errors.Count > 0).Select(x => new { key = x.Key, errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList() }).ToList() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H1", location = "AppointmentsController.Create:102", message = "User is null", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                return NotFound();
            }

            var memberProfile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            // #region agent log
            try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H1", location = "AppointmentsController.Create:110", message = "MemberProfile retrieved", data = new { memberProfileId = memberProfile?.Id, memberProfileExists = memberProfile != null }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion

            if (memberProfile == null)
            {
                return RedirectToAction("Index", "MemberProfiles");
            }

            // Tarih ve saati birleştir
            if (appointment.AppointmentDate.HasValue && appointment.AppointmentTime.HasValue)
            {
                appointment.StartDateTime = appointment.AppointmentDate.Value.Date.Add(appointment.AppointmentTime.Value);
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H3", location = "AppointmentsController.Create:120", message = "StartDateTime set", data = new { startDateTime = appointment.StartDateTime.ToString() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
            }
            else
            {
                if (!appointment.AppointmentDate.HasValue)
                {
                    ModelState.AddModelError("AppointmentDate", "Tarih seçimi zorunludur.");
                }
                if (!appointment.AppointmentTime.HasValue)
                {
                    ModelState.AddModelError("AppointmentTime", "Saat seçimi zorunludur.");
                }
            }

            appointment.MemberProfileId = memberProfile.Id;
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H1", location = "AppointmentsController.Create:135", message = "MemberProfileId set", data = new { memberProfileId = appointment.MemberProfileId }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion

            // Antrenör ve hizmet bilgilerini yükle
            var trainer = await _context.Trainers
                .Include(t => t.GymBranch)
                .FirstOrDefaultAsync(t => t.Id == appointment.TrainerId);

            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == appointment.ServiceId);

            if (trainer == null || service == null)
            {
                ModelState.AddModelError("", "Geçersiz antrenör veya hizmet seçimi.");
            }

            // Antrenörün bu hizmeti verip vermediğini kontrol et
            var trainerService = await _context.TrainerServices
                .FirstOrDefaultAsync(ts => ts.TrainerId == appointment.TrainerId && ts.ServiceId == appointment.ServiceId);

            if (trainerService == null)
            {
                ModelState.AddModelError("ServiceId", "Seçilen antrenör bu hizmeti vermemektedir.");
            }

            // Antrenörün müsaitlik saatlerine göre kontrol
            var dayOfWeek = appointment.StartDateTime.DayOfWeek;
            var startTime = appointment.StartDateTime.TimeOfDay;
            var endTime = appointment.StartDateTime.AddMinutes(appointment.DurationMinutes).TimeOfDay;

            var availability = await _context.TrainerAvailabilities
                .FirstOrDefaultAsync(a => 
                    a.TrainerId == appointment.TrainerId 
                    && a.DayOfWeek == dayOfWeek
                    && a.StartTime <= startTime 
                    && a.EndTime >= endTime);

            if (availability == null)
            {
                ModelState.AddModelError("StartDateTime", 
                    $"Seçilen antrenör {DayOfWeekHelper.GetTurkishName(dayOfWeek)} günü " +
                    $"{startTime:hh\\:mm}-{endTime:hh\\:mm} saatleri arasında müsait değildir.");
            }

            // Salon çalışma saatlerine göre kontrol
            if (trainer?.GymBranch != null)
            {
                if (!string.IsNullOrEmpty(trainer.GymBranch.OpeningTime) && !string.IsNullOrEmpty(trainer.GymBranch.ClosingTime))
                {
                    var openingTime = TimeSpan.Parse(trainer.GymBranch.OpeningTime);
                    var closingTime = TimeSpan.Parse(trainer.GymBranch.ClosingTime);

                    if (startTime < openingTime || endTime > closingTime)
                    {
                        ModelState.AddModelError("StartDateTime", 
                            $"Spor salonu {openingTime:hh\\:mm}-{closingTime:hh\\:mm} saatleri arasında açıktır.");
                    }
                }
            }

            // Çakışma kontrolü - Aynı antrenör aynı saatte onaylanmış veya bekleyen randevu var mı?
            var conflictingAppointment = await _context.Appointments
                .Where(a => a.TrainerId == appointment.TrainerId
                    && (a.Status == AppointmentStatus.Approved || a.Status == AppointmentStatus.Pending)
                    && a.Id != appointment.Id
                    && (
                        (appointment.StartDateTime >= a.StartDateTime && appointment.StartDateTime < a.StartDateTime.AddMinutes(a.DurationMinutes)) ||
                        (appointment.StartDateTime.AddMinutes(appointment.DurationMinutes) > a.StartDateTime && appointment.StartDateTime.AddMinutes(appointment.DurationMinutes) <= a.StartDateTime.AddMinutes(a.DurationMinutes)) ||
                        (appointment.StartDateTime <= a.StartDateTime && appointment.StartDateTime.AddMinutes(appointment.DurationMinutes) >= a.StartDateTime.AddMinutes(a.DurationMinutes))
                    ))
                .FirstOrDefaultAsync();

            if (conflictingAppointment != null)
            {
                ModelState.AddModelError("StartDateTime", 
                    "Seçilen saatte antrenörün başka bir randevusu bulunmaktadır. Lütfen farklı bir saat seçin.");
            }

            // Süre ve fiyat bilgilerini hizmetten al
            if (service != null)
            {
                appointment.DurationMinutes = service.DurationMinutes;
                appointment.Price = service.Price;
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H2", location = "AppointmentsController.Create:214", message = "DurationMinutes and Price set from service", data = new { durationMinutes = appointment.DurationMinutes, price = appointment.Price, serviceDuration = service.DurationMinutes, servicePrice = service.Price }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
            }
            else
            {
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H2", location = "AppointmentsController.Create:220", message = "Service is null, DurationMinutes not set", data = new { serviceId = appointment.ServiceId }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
            }

            // ModelState'den navigation property ve programatik olarak set edilen alanların hatalarını temizle
            ModelState.Remove("MemberProfile");
            ModelState.Remove("Trainer");
            ModelState.Remove("Service");
            ModelState.Remove("DurationMinutes"); // Service'ten set edildiği için validation hatasını temizle
            ModelState.Remove("Price"); // Service'ten set edildiği için validation hatasını temizle

            // #region agent log
            try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H3", location = "AppointmentsController.Create:225", message = "Before ModelState.IsValid check", data = new { modelStateIsValid = ModelState.IsValid, memberProfileId = appointment.MemberProfileId, trainerId = appointment.TrainerId, serviceId = appointment.ServiceId, durationMinutes = appointment.DurationMinutes, startDateTime = appointment.StartDateTime.ToString(), modelStateErrors = ModelState.Where(x => x.Value?.Errors.Count > 0).Select(x => new { key = x.Key, errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList() }).ToList() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion

            if (ModelState.IsValid)
            {
                appointment.Status = AppointmentStatus.Pending;
                _context.Add(appointment);
                
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H4", location = "AppointmentsController.Create:255", message = "Before SaveChangesAsync", data = new { appointmentId = appointment.Id, memberProfileId = appointment.MemberProfileId, trainerId = appointment.TrainerId, serviceId = appointment.ServiceId, startDateTime = appointment.StartDateTime.ToString(), durationMinutes = appointment.DurationMinutes, price = appointment.Price, status = appointment.Status.ToString() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                
                try
                {
                    var rowsAffected = await _context.SaveChangesAsync();
                    
                    // #region agent log
                    try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H4", location = "AppointmentsController.Create:262", message = "SaveChangesAsync completed", data = new { rowsAffected = rowsAffected, appointmentId = appointment.Id, databaseName = _context.Database.GetDbConnection().Database }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                    // #endregion
                }
                catch (Exception ex)
                {
                    // #region agent log
                    try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H4", location = "AppointmentsController.Create:268", message = "SaveChangesAsync exception", data = new { exceptionType = ex.GetType().Name, message = ex.Message, innerException = ex.InnerException?.Message, stackTrace = ex.StackTrace?.Substring(0, Math.Min(500, ex.StackTrace.Length)) }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                    // #endregion
                    throw;
                }
                
                TempData["SuccessMessage"] = "Randevunuz onaya gönderilmiştir. Antrenör onayı bekleniyor.";
                return RedirectToAction(nameof(Index));
            }

            // Hata durumunda dropdown'ları tekrar doldur
            // ModelState hatalarını TempData'ya ekle
            var errorMessages = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                .ToList();
            
            if (errorMessages.Any())
            {
                TempData["ErrorMessage"] = string.Join("<br/>", errorMessages);
            }
            ViewData["TrainerId"] = new SelectList(
                _context.Trainers
                    .Include(t => t.GymBranch)
                    .Select(t => new { 
                        Id = t.Id, 
                        Name = $"{t.FirstName} {t.LastName} - {t.GymBranch.Name}" 
                    }), 
                "Id", "Name", appointment.TrainerId);

            if (appointment.TrainerId > 0)
            {
                var services = await _context.TrainerServices
                    .Where(ts => ts.TrainerId == appointment.TrainerId)
                    .Include(ts => ts.Service)
                    .Select(ts => ts.Service)
                    .ToListAsync();

                ViewData["ServiceId"] = new SelectList(services, "Id", "Name", appointment.ServiceId);
            }
            else
            {
                ViewData["ServiceId"] = new SelectList(new List<Service>(), "Id", "Name");
            }

            return View(appointment);
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var memberProfile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            var appointment = await _context.Appointments
                .Include(a => a.Trainer)
                    .ThenInclude(t => t.GymBranch)
                .Include(a => a.Service)
                .Include(a => a.MemberProfile)
                .FirstOrDefaultAsync(m => m.Id == id && m.MemberProfileId == memberProfile.Id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var memberProfile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            var appointment = await _context.Appointments
                .Include(a => a.Trainer)
                    .ThenInclude(t => t.GymBranch)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.Id == id && m.MemberProfileId == memberProfile.Id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var memberProfile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.MemberProfileId == memberProfile.Id);

            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Randevu başarıyla iptal edildi.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Appointments/GetServicesByTrainer
        [HttpGet]
        public async Task<IActionResult> GetServicesByTrainer(int trainerId)
        {
            var services = await _context.TrainerServices
                .Where(ts => ts.TrainerId == trainerId)
                .Include(ts => ts.Service)
                .Select(ts => new { 
                    id = ts.Service.Id, 
                    name = ts.Service.Name, 
                    durationMinutes = ts.Service.DurationMinutes, 
                    price = ts.Service.Price 
                })
                .ToListAsync();
            
            return Json(services);
        }

        // GET: Appointments/GetTrainerAvailabilities
        [HttpGet]
        public async Task<IActionResult> GetTrainerAvailabilities(int trainerId, DateTime selectedDate)
        {
            var dayOfWeek = selectedDate.DayOfWeek;
            var availabilities = await _context.TrainerAvailabilities
                .Where(a => a.TrainerId == trainerId && a.DayOfWeek == dayOfWeek)
                .ToListAsync();

            // Onaylanmış randevuları al (o gün için)
            var approvedAppointments = await _context.Appointments
                .Where(a => a.TrainerId == trainerId 
                    && a.Status == AppointmentStatus.Approved
                    && a.StartDateTime.Date == selectedDate.Date)
                .Select(a => new { 
                    startTime = a.StartDateTime.TimeOfDay, 
                    endTime = a.EndDateTime.TimeOfDay 
                })
                .ToListAsync();

            // Müsait saatleri hesapla - onaylanmış randevuları çıkar
            var availableSlots = new List<object>();
            foreach (var availability in availabilities)
            {
                var currentStart = availability.StartTime;
                var endTime = availability.EndTime;

                // Onaylanmış randevuları bu müsaitlik aralığına göre filtrele
                var blockedSlots = approvedAppointments
                    .Where(apt => 
                        (apt.startTime >= currentStart && apt.startTime < endTime) ||
                        (apt.endTime > currentStart && apt.endTime <= endTime) ||
                        (apt.startTime <= currentStart && apt.endTime >= endTime))
                    .OrderBy(apt => apt.startTime)
                    .ToList();

                if (blockedSlots.Count == 0)
                {
                    // Hiç engellenmemiş, tüm aralık müsait
                    availableSlots.Add(new { 
                        startTime = currentStart.ToString(@"hh\:mm"), 
                        endTime = endTime.ToString(@"hh\:mm") 
                    });
                }
                else
                {
                    // Engellenmiş saatleri çıkar
                    var slotStart = currentStart;
                    foreach (var blocked in blockedSlots)
                    {
                        if (slotStart < blocked.startTime)
                        {
                            // Engellenmeden önce müsait bir aralık var
                            availableSlots.Add(new { 
                                startTime = slotStart.ToString(@"hh\:mm"), 
                                endTime = blocked.startTime.ToString(@"hh\:mm") 
                            });
                        }
                        slotStart = blocked.endTime > slotStart ? blocked.endTime : slotStart;
                    }
                    // Son engellemeden sonra kalan süre
                    if (slotStart < endTime)
                    {
                        availableSlots.Add(new { 
                            startTime = slotStart.ToString(@"hh\:mm"), 
                            endTime = endTime.ToString(@"hh\:mm") 
                        });
                    }
                }
            }

            // Antrenörün salon bilgilerini de gönder
            var trainer = await _context.Trainers
                .Include(t => t.GymBranch)
                .FirstOrDefaultAsync(t => t.Id == trainerId);

            return Json(new { 
                availabilities = availableSlots,
                gymBranchHours = trainer?.GymBranch != null ? new { 
                    openingTime = trainer.GymBranch.OpeningTime, 
                    closingTime = trainer.GymBranch.ClosingTime 
                } : null
            });
        }
    }
}

