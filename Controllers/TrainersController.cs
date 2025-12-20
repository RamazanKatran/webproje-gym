using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebProjeGym.Data;
using WebProjeGym.Models;

namespace WebProjeGym.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TrainersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TrainersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Trainers
        public async Task<IActionResult> Index()
        {
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H1", location = "TrainersController.Index:26", message = "Index method entry", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion

            // #region agent log
            try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H1", location = "TrainersController.Index:28", message = "Before query execution", data = new { databaseName = _context.Database.GetDbConnection().Database }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion

            try
            {
                var trainers = _context.Trainers
                    .Include(t => t.GymBranch)
                    .Include(t => t.TrainerServices)
                        .ThenInclude(ts => ts.Service)
                    .Include(t => t.TrainerAvailabilities)
                    .OrderBy(t => t.FirstName);
                
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H1", location = "TrainersController.Index:40", message = "Query built, before ToListAsync", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion

                var result = await trainers.ToListAsync();
                
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H1", location = "TrainersController.Index:46", message = "Query executed successfully", data = new { count = result.Count }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion

                return View(result);
            }
            catch (Exception ex)
            {
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H1", location = "TrainersController.Index:52", message = "Exception caught", data = new { exceptionType = ex.GetType().Name, message = ex.Message, innerException = ex.InnerException?.Message }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                throw;
            }
        }

        // GET: Trainers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.GymBranch)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .Include(t => t.TrainerAvailabilities)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }

        // GET: Trainers/Create
        public IActionResult Create()
        {
            ViewData["GymBranchId"] = new SelectList(_context.GymBranches, "Id", "Name");
            ViewBag.DayOfWeek = new SelectList(Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>());
            ViewBag.Services = new List<Service>(); // İlk başta boş, salon seçilince doldurulacak
            return View();
        }

        // POST: Trainers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Trainer trainer, string email, string password, int[] selectedServices, List<TrainerAvailabilityInput> availabilities)
        {
            // Email ve şifre validasyonu
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "E-posta adresi zorunludur.");
            }
            else if (await _userManager.FindByEmailAsync(email) != null)
            {
                ModelState.AddModelError("", "Bu e-posta adresi zaten kullanılıyor.");
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                ModelState.AddModelError("", "Şifre en az 6 karakter olmalıdır.");
            }

            if (ModelState.IsValid)
            {
                // Önce ApplicationUser oluştur
                var trainerUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true // Antrenör için e-posta doğrulaması gerekmez
                };

                var result = await _userManager.CreateAsync(trainerUser, password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    ViewData["GymBranchId"] = new SelectList(_context.GymBranches, "Id", "Name", trainer.GymBranchId);
                    ViewBag.DayOfWeek = new SelectList(Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>());
                    ViewBag.Services = await _context.Services.Where(s => s.GymBranchId == trainer.GymBranchId).ToListAsync();
                    return View(trainer);
                }

                // Trainer rolüne ekle
                await _userManager.AddToRoleAsync(trainerUser, "Trainer");

                // Trainer'a ApplicationUserId'yi ata
                trainer.ApplicationUserId = trainerUser.Id;

                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H5", location = "TrainersController.Create:145", message = "Before Add trainer", data = new { trainerId = trainer.Id, firstName = trainer.FirstName, lastName = trainer.LastName, gymBranchId = trainer.GymBranchId, applicationUserId = trainer.ApplicationUserId, databaseName = _context.Database.GetDbConnection().Database }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion

                _context.Add(trainer);
                
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H5", location = "TrainersController.Create:150", message = "Before SaveChangesAsync (trainer)", data = new { trainerId = trainer.Id }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                
                var rowsAffected1 = await _context.SaveChangesAsync();
                
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H5", location = "TrainersController.Create:156", message = "SaveChangesAsync completed (trainer)", data = new { rowsAffected = rowsAffected1, trainerId = trainer.Id, databaseName = _context.Database.GetDbConnection().Database }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion

                // Seçilen hizmetleri ekle
                if (selectedServices != null)
                {
                    foreach (var serviceId in selectedServices)
                    {
                        _context.TrainerServices.Add(new TrainerService
                        {
                            TrainerId = trainer.Id,
                            ServiceId = serviceId
                        });
                    }
                }

                // Müsaitlik saatlerini ekle
                if (availabilities != null)
                {
                    foreach (var avail in availabilities)
                    {
                        if (avail.DayOfWeek.HasValue && avail.StartTime.HasValue && avail.EndTime.HasValue)
                        {
                            _context.TrainerAvailabilities.Add(new TrainerAvailability
                            {
                                TrainerId = trainer.Id,
                                DayOfWeek = avail.DayOfWeek.Value,
                                StartTime = avail.StartTime.Value,
                                EndTime = avail.EndTime.Value
                            });
                        }
                    }
                }

                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H5", location = "TrainersController.Create:182", message = "Before SaveChangesAsync (services and availabilities)", data = new { trainerId = trainer.Id, selectedServicesCount = selectedServices?.Length ?? 0, availabilitiesCount = availabilities?.Count ?? 0 }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion

                var rowsAffected2 = await _context.SaveChangesAsync();
                
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "H5", location = "TrainersController.Create:188", message = "SaveChangesAsync completed (services and availabilities)", data = new { rowsAffected = rowsAffected2, trainerId = trainer.Id, databaseName = _context.Database.GetDbConnection().Database }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                TempData["SuccessMessage"] = "Antrenör başarıyla oluşturuldu. E-posta: " + email;
                return RedirectToAction(nameof(Index));
            }
            ViewData["GymBranchId"] = new SelectList(_context.GymBranches, "Id", "Name", trainer.GymBranchId);
            ViewBag.DayOfWeek = new SelectList(Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>());
            if (trainer.GymBranchId > 0)
            {
                ViewBag.Services = await _context.Services.Where(s => s.GymBranchId == trainer.GymBranchId).ToListAsync();
            }
            return View(trainer);
        }

        // GET: Trainers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.TrainerServices)
                .Include(t => t.TrainerAvailabilities)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (trainer == null)
            {
                return NotFound();
            }
            
            ViewData["GymBranchId"] = new SelectList(_context.GymBranches, "Id", "Name", trainer.GymBranchId);
            ViewBag.DayOfWeek = new SelectList(Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>());
            ViewBag.Services = await _context.Services.Where(s => s.GymBranchId == trainer.GymBranchId).ToListAsync();
            ViewBag.SelectedServices = trainer.TrainerServices.Select(ts => ts.ServiceId).ToList();
            
            return View(trainer);
        }

        // POST: Trainers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Trainer trainer, int[] selectedServices, List<TrainerAvailabilityInput> availabilities)
        {
            if (id != trainer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainer);
                    
                    // Mevcut hizmetleri temizle ve yenilerini ekle
                    var existingServices = _context.TrainerServices.Where(ts => ts.TrainerId == trainer.Id);
                    _context.TrainerServices.RemoveRange(existingServices);
                    
                    if (selectedServices != null)
                    {
                        foreach (var serviceId in selectedServices)
                        {
                            _context.TrainerServices.Add(new TrainerService
                            {
                                TrainerId = trainer.Id,
                                ServiceId = serviceId
                            });
                        }
                    }

                    // Mevcut müsaitlik saatlerini temizle ve yenilerini ekle
                    var existingAvailabilities = _context.TrainerAvailabilities.Where(ta => ta.TrainerId == trainer.Id);
                    _context.TrainerAvailabilities.RemoveRange(existingAvailabilities);
                    
                    if (availabilities != null)
                    {
                        foreach (var avail in availabilities)
                        {
                            if (avail.DayOfWeek.HasValue && avail.StartTime.HasValue && avail.EndTime.HasValue)
                            {
                                _context.TrainerAvailabilities.Add(new TrainerAvailability
                                {
                                    TrainerId = trainer.Id,
                                    DayOfWeek = avail.DayOfWeek.Value,
                                    StartTime = avail.StartTime.Value,
                                    EndTime = avail.EndTime.Value
                                });
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainerExists(trainer.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["GymBranchId"] = new SelectList(_context.GymBranches, "Id", "Name", trainer.GymBranchId);
            ViewBag.DayOfWeek = new SelectList(Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>());
            if (trainer.GymBranchId > 0)
            {
                ViewBag.Services = await _context.Services.Where(s => s.GymBranchId == trainer.GymBranchId).ToListAsync();
            }
            return View(trainer);
        }

        // GET: Trainers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.GymBranch)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }

        // POST: Trainers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainer = await _context.Trainers
                .Include(t => t.Appointments)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (trainer != null)
            {
                // Antrenörün randevularını manuel olarak sil
                var appointments = await _context.Appointments
                    .Where(a => a.TrainerId == id)
                    .ToListAsync();
                
                if (appointments.Any())
                {
                    _context.Appointments.RemoveRange(appointments);
                }
                
                // Antrenörün TrainerService ilişkilerini manuel olarak sil
                var trainerServices = await _context.TrainerServices
                    .Where(ts => ts.TrainerId == id)
                    .ToListAsync();
                
                if (trainerServices.Any())
                {
                    _context.TrainerServices.RemoveRange(trainerServices);
                }
                
                // Antrenörü sil (TrainerAvailability cascade ile otomatik silinir)
                _context.Trainers.Remove(trainer);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Trainers/ManageServices/5
        public async Task<IActionResult> ManageServices(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (trainer == null)
            {
                return NotFound();
            }

            // Antrenörün çalıştığı salondaki tüm hizmetler
            var allServices = await _context.Services
                .Where(s => s.GymBranchId == trainer.GymBranchId)
                .ToListAsync();

            ViewBag.AllServices = allServices;
            ViewBag.TrainerServices = trainer.TrainerServices.Select(ts => ts.ServiceId).ToList();

            return View(trainer);
        }

        // POST: Trainers/ManageServices/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageServices(int id, int[] selectedServices)
        {
            var trainer = await _context.Trainers
                .Include(t => t.TrainerServices)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainer == null)
            {
                return NotFound();
            }

            // Mevcut hizmetleri temizle
            _context.TrainerServices.RemoveRange(trainer.TrainerServices);

            // Seçilen hizmetleri ekle
            if (selectedServices != null)
            {
                foreach (var serviceId in selectedServices)
                {
                    _context.TrainerServices.Add(new TrainerService
                    {
                        TrainerId = trainer.Id,
                        ServiceId = serviceId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = trainer.Id });
        }

        // GET: Trainers/GetServicesByGymBranch
        [HttpGet]
        public async Task<IActionResult> GetServicesByGymBranch(int gymBranchId)
        {
            var services = await _context.Services
                .Where(s => s.GymBranchId == gymBranchId)
                .Select(s => new { 
                    id = s.Id, 
                    name = s.Name, 
                    durationMinutes = s.DurationMinutes, 
                    price = s.Price 
                })
                .ToListAsync();
            
            return Json(services);
        }

        // GET: Trainers/GetGymBranchHours
        [HttpGet]
        public async Task<IActionResult> GetGymBranchHours(int gymBranchId)
        {
            var gymBranch = await _context.GymBranches.FindAsync(gymBranchId);
            if (gymBranch == null)
            {
                return Json(new { openingTime = "", closingTime = "" });
            }

            return Json(new { 
                openingTime = gymBranch.OpeningTime ?? "", 
                closingTime = gymBranch.ClosingTime ?? "" 
            });
        }

        private bool TrainerExists(int id)
        {
            return _context.Trainers.Any(e => e.Id == id);
        }
    }
}

