using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProjeGym.Data;
using WebProjeGym.Models;

namespace WebProjeGym.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GymBranchesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GymBranchesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: GymBranches
        public async Task<IActionResult> Index()
        {
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "H2", location = "GymBranchesController.Index:22", message = "Index method entry", data = new { databaseName = _context.Database.GetDbConnection().Database }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion

            try
            {
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "H2", location = "GymBranchesController.Index:28", message = "Before query execution", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion

                var gyms = await _context.GymBranches
                    .Include(g => g.Services)
                    .Include(g => g.Trainers)
                    .ToListAsync();

                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "H2", location = "GymBranchesController.Index:36", message = "Query executed successfully", data = new { count = gyms.Count }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion

                return View(gyms);
            }
            catch (Exception ex)
            {
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run2", hypothesisId = "H2", location = "GymBranchesController.Index:44", message = "Exception caught", data = new { exceptionType = ex.GetType().Name, message = ex.Message, innerException = ex.InnerException?.Message, stackTrace = ex.StackTrace != null ? ex.StackTrace.Substring(0, Math.Min(500, ex.StackTrace.Length)) : null }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                throw;
            }
        }

        // GET: GymBranches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gymBranch = await _context.GymBranches
                .Include(g => g.Services)
                .Include(g => g.Trainers)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (gymBranch == null)
            {
                return NotFound();
            }

            return View(gymBranch);
        }

        // GET: GymBranches/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GymBranches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GymBranch gymBranch)
        {
            // #region agent log
            System.IO.File.AppendAllText(
                "c:\\Users\\ASUS\\Desktop\\Hafta2Web\\WebProjeGym\\.cursor\\debug.log",
                "{\"sessionId\":\"debug-session\",\"runId\":\"gym-create\",\"hypothesisId\":\"G1\",\"location\":\"GymBranchesController.Create\",\"message\":\"Create POST called\",\"data\":{\"name\":\""
                + (gymBranch?.Name ?? "") + "\"},\"timestamp\":" + DateTimeOffset.Now.ToUnixTimeMilliseconds() + "}\n");
            // #endregion

            if (ModelState.IsValid)
            {
                _context.Add(gymBranch);
                await _context.SaveChangesAsync();

                // #region agent log
                System.IO.File.AppendAllText(
                    "c:\\Users\\ASUS\\Desktop\\Hafta2Web\\WebProjeGym\\.cursor\\debug.log",
                    "{\"sessionId\":\"debug-session\",\"runId\":\"gym-create\",\"hypothesisId\":\"G2\",\"location\":\"GymBranchesController.Create\",\"message\":\"GymBranch saved\",\"data\":{\"id\":" 
                    + gymBranch.Id + "},\"timestamp\":" + DateTimeOffset.Now.ToUnixTimeMilliseconds() + "}\n");
                // #endregion

                return RedirectToAction(nameof(Index));
            }

            // #region agent log
            System.IO.File.AppendAllText(
                "c:\\Users\\ASUS\\Desktop\\Hafta2Web\\WebProjeGym\\.cursor\\debug.log",
                "{\"sessionId\":\"debug-session\",\"runId\":\"gym-create\",\"hypothesisId\":\"G3\",\"location\":\"GymBranchesController.Create\",\"message\":\"ModelState invalid\",\"data\":{},\"timestamp\":" 
                + DateTimeOffset.Now.ToUnixTimeMilliseconds() + "}\n");
            // #endregion

            return View(gymBranch);
        }

        // GET: GymBranches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gymBranch = await _context.GymBranches.FindAsync(id);
            if (gymBranch == null)
            {
                return NotFound();
            }
            return View(gymBranch);
        }

        // POST: GymBranches/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GymBranch gymBranch)
        {
            if (id != gymBranch.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gymBranch);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GymBranchExists(gymBranch.Id))
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
            return View(gymBranch);
        }

        // GET: GymBranches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gymBranch = await _context.GymBranches
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gymBranch == null)
            {
                return NotFound();
            }

            return View(gymBranch);
        }

        // POST: GymBranches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gymBranch = await _context.GymBranches
                .Include(g => g.Services)
                .Include(g => g.Trainers)
                .FirstOrDefaultAsync(g => g.Id == id);
            
            if (gymBranch != null)
            {
                // 1. GymBranch'e bağlı tüm Services'leri işle
                var services = await _context.Services
                    .Where(s => s.GymBranchId == id)
                    .ToListAsync();
                
                foreach (var service in services)
                {
                    // Her Service için randevuları sil
                    var serviceAppointments = await _context.Appointments
                        .Where(a => a.ServiceId == service.Id)
                        .ToListAsync();
                    
                    if (serviceAppointments.Any())
                    {
                        _context.Appointments.RemoveRange(serviceAppointments);
                    }
                    
                    // Her Service için TrainerService ilişkilerini sil
                    var serviceTrainerServices = await _context.TrainerServices
                        .Where(ts => ts.ServiceId == service.Id)
                        .ToListAsync();
                    
                    if (serviceTrainerServices.Any())
                    {
                        _context.TrainerServices.RemoveRange(serviceTrainerServices);
                    }
                }
                
                // 2. GymBranch'e bağlı tüm Trainers'ları işle
                var trainers = await _context.Trainers
                    .Where(t => t.GymBranchId == id)
                    .ToListAsync();
                
                foreach (var trainer in trainers)
                {
                    // Her Trainer için randevuları sil
                    var trainerAppointments = await _context.Appointments
                        .Where(a => a.TrainerId == trainer.Id)
                        .ToListAsync();
                    
                    if (trainerAppointments.Any())
                    {
                        _context.Appointments.RemoveRange(trainerAppointments);
                    }
                    
                    // Her Trainer için TrainerService ilişkilerini sil
                    var trainerTrainerServices = await _context.TrainerServices
                        .Where(ts => ts.TrainerId == trainer.Id)
                        .ToListAsync();
                    
                    if (trainerTrainerServices.Any())
                    {
                        _context.TrainerServices.RemoveRange(trainerTrainerServices);
                    }
                }
                
                // 3. Services'leri sil (TrainerAvailabilities cascade ile otomatik silinir)
                if (services.Any())
                {
                    _context.Services.RemoveRange(services);
                }
                
                // 4. Trainers'ları sil (TrainerAvailabilities cascade ile otomatik silinir)
                if (trainers.Any())
                {
                    _context.Trainers.RemoveRange(trainers);
                }
                
                // 5. Son olarak GymBranch'i sil
                _context.GymBranches.Remove(gymBranch);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool GymBranchExists(int id)
        {
            return _context.GymBranches.Any(e => e.Id == id);
        }
    }
}


