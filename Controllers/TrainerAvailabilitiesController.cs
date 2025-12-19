using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebProjeGym.Data;
using WebProjeGym.Models;
using WebProjeGym.Helpers;

namespace WebProjeGym.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TrainerAvailabilitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainerAvailabilitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TrainerAvailabilities
        public async Task<IActionResult> Index(int? trainerId)
        {
            IQueryable<TrainerAvailability> availabilities = _context.TrainerAvailabilities
                .Include(t => t.Trainer);

            if (trainerId.HasValue)
            {
                availabilities = availabilities.Where(a => a.TrainerId == trainerId.Value);
                ViewBag.TrainerId = trainerId.Value;
                var trainer = await _context.Trainers.FindAsync(trainerId.Value);
                ViewBag.TrainerName = trainer != null ? $"{trainer.FirstName} {trainer.LastName}" : "";
            }

            return View(await availabilities.OrderBy(a => a.DayOfWeek).ThenBy(a => a.StartTime).ToListAsync());
        }

        // GET: TrainerAvailabilities/Create
        public IActionResult Create(int? trainerId)
        {
            ViewData["TrainerId"] = new SelectList(_context.Trainers.Select(t => new { 
                Id = t.Id, 
                FullName = $"{t.FirstName} {t.LastName}" 
            }), "Id", "FullName", trainerId);

            // Hafta günleri için dropdown - Türkçe
            var dayOfWeekList = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>()
                .Select(d => new SelectListItem 
                { 
                    Value = ((int)d).ToString(), 
                    Text = DayOfWeekHelper.GetTurkishName(d)
                }).ToList();
            ViewBag.DayOfWeek = new SelectList(dayOfWeekList, "Value", "Text");
            
            return View();
        }

        // POST: TrainerAvailabilities/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainerAvailability trainerAvailability)
        {
            // Çakışma kontrolü - Aynı antrenör, aynı gün, çakışan saatler
            var conflictingAvailability = await _context.TrainerAvailabilities
                .Where(a => a.TrainerId == trainerAvailability.TrainerId 
                    && a.DayOfWeek == trainerAvailability.DayOfWeek
                    && a.Id != trainerAvailability.Id
                    && (
                        (trainerAvailability.StartTime >= a.StartTime && trainerAvailability.StartTime < a.EndTime) ||
                        (trainerAvailability.EndTime > a.StartTime && trainerAvailability.EndTime <= a.EndTime) ||
                        (trainerAvailability.StartTime <= a.StartTime && trainerAvailability.EndTime >= a.EndTime)
                    ))
                .FirstOrDefaultAsync();

            if (conflictingAvailability != null)
            {
                ModelState.AddModelError("", 
                    $"{DayOfWeekHelper.GetTurkishName(trainerAvailability.DayOfWeek)} günü için " +
                    $"{conflictingAvailability.StartTime.ToString(@"hh\:mm")}-{conflictingAvailability.EndTime.ToString(@"hh\:mm")} " +
                    "saatleri arasında zaten bir müsaitlik kaydı bulunmaktadır. Lütfen farklı saatler seçin.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(trainerAvailability);
                await _context.SaveChangesAsync();
                
                if (Request.Query.ContainsKey("returnToTrainer"))
                {
                    return RedirectToAction("Details", "Trainers", new { id = trainerAvailability.TrainerId });
                }
                return RedirectToAction(nameof(Index), new { trainerId = trainerAvailability.TrainerId });
            }
            
            ViewData["TrainerId"] = new SelectList(_context.Trainers.Select(t => new { 
                Id = t.Id, 
                FullName = $"{t.FirstName} {t.LastName}" 
            }), "Id", "FullName", trainerAvailability.TrainerId);
            
            var dayOfWeekList = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>()
                .Select(d => new SelectListItem 
                { 
                    Value = ((int)d).ToString(), 
                    Text = DayOfWeekHelper.GetTurkishName(d) 
                }).ToList();
            ViewBag.DayOfWeek = new SelectList(dayOfWeekList, "Value", "Text", ((int)trainerAvailability.DayOfWeek).ToString());
            
            return View(trainerAvailability);
        }

        // GET: TrainerAvailabilities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainerAvailability = await _context.TrainerAvailabilities.FindAsync(id);
            if (trainerAvailability == null)
            {
                return NotFound();
            }
            
            ViewData["TrainerId"] = new SelectList(_context.Trainers.Select(t => new { 
                Id = t.Id, 
                FullName = $"{t.FirstName} {t.LastName}" 
            }), "Id", "FullName", trainerAvailability.TrainerId);
            
            var dayOfWeekList = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>()
                .Select(d => new SelectListItem 
                { 
                    Value = ((int)d).ToString(), 
                    Text = DayOfWeekHelper.GetTurkishName(d) 
                }).ToList();
            ViewBag.DayOfWeek = new SelectList(dayOfWeekList, "Value", "Text", ((int)trainerAvailability.DayOfWeek).ToString());
            
            return View(trainerAvailability);
        }

        // POST: TrainerAvailabilities/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrainerAvailability trainerAvailability)
        {
            if (id != trainerAvailability.Id)
            {
                return NotFound();
            }

            // Çakışma kontrolü - Aynı antrenör, aynı gün, çakışan saatler
            var conflictingAvailability = await _context.TrainerAvailabilities
                .Where(a => a.TrainerId == trainerAvailability.TrainerId 
                    && a.DayOfWeek == trainerAvailability.DayOfWeek
                    && a.Id != trainerAvailability.Id
                    && (
                        (trainerAvailability.StartTime >= a.StartTime && trainerAvailability.StartTime < a.EndTime) ||
                        (trainerAvailability.EndTime > a.StartTime && trainerAvailability.EndTime <= a.EndTime) ||
                        (trainerAvailability.StartTime <= a.StartTime && trainerAvailability.EndTime >= a.EndTime)
                    ))
                .FirstOrDefaultAsync();

            if (conflictingAvailability != null)
            {
                ModelState.AddModelError("", 
                    $"{DayOfWeekHelper.GetTurkishName(trainerAvailability.DayOfWeek)} günü için " +
                    $"{conflictingAvailability.StartTime.ToString(@"hh\:mm")}-{conflictingAvailability.EndTime.ToString(@"hh\:mm")} " +
                    "saatleri arasında zaten bir müsaitlik kaydı bulunmaktadır. Lütfen farklı saatler seçin.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainerAvailability);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainerAvailabilityExists(trainerAvailability.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { trainerId = trainerAvailability.TrainerId });
            }
            
            ViewData["TrainerId"] = new SelectList(_context.Trainers.Select(t => new { 
                Id = t.Id, 
                FullName = $"{t.FirstName} {t.LastName}" 
            }), "Id", "FullName", trainerAvailability.TrainerId);
            
            var dayOfWeekList = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>()
                .Select(d => new SelectListItem 
                { 
                    Value = ((int)d).ToString(), 
                    Text = DayOfWeekHelper.GetTurkishName(d) 
                }).ToList();
            ViewBag.DayOfWeek = new SelectList(dayOfWeekList, "Value", "Text", ((int)trainerAvailability.DayOfWeek).ToString());
            
            return View(trainerAvailability);
        }

        // GET: TrainerAvailabilities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainerAvailability = await _context.TrainerAvailabilities
                .Include(t => t.Trainer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trainerAvailability == null)
            {
                return NotFound();
            }

            return View(trainerAvailability);
        }

        // POST: TrainerAvailabilities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainerAvailability = await _context.TrainerAvailabilities.FindAsync(id);
            var trainerId = trainerAvailability?.TrainerId;
            
            if (trainerAvailability != null)
            {
                _context.TrainerAvailabilities.Remove(trainerAvailability);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { trainerId = trainerId });
        }

        private bool TrainerAvailabilityExists(int id)
        {
            return _context.TrainerAvailabilities.Any(e => e.Id == id);
        }
    }
}

