using Microsoft.AspNetCore.Authorization;
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

        public TrainersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trainers
        public async Task<IActionResult> Index()
        {
            var trainers = _context.Trainers
                .Include(t => t.GymBranch)
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .Include(t => t.TrainerAvailabilities)
                .OrderBy(t => t.FirstName);
            return View(await trainers.ToListAsync());
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
        public async Task<IActionResult> Create(Trainer trainer, int[] selectedServices, List<TrainerAvailabilityInput> availabilities)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trainer);
                await _context.SaveChangesAsync();

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

                await _context.SaveChangesAsync();
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
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer != null)
            {
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

