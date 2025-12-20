using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebProjeGym.Data;
using WebProjeGym.Models;

namespace WebProjeGym.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Services
        public async Task<IActionResult> Index()
        {
            var services = _context.Services
                .Include(s => s.GymBranch);
            var list = await services.ToListAsync();
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "service-index", hypothesisId = "S4", location = "ServicesController.Index", message = "Index loaded", data = new { count = list.Count }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            return View(list);
        }

        // GET: Services/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.GymBranch)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // GET: Services/Create
        public IActionResult Create()
        {
            ViewData["GymBranchId"] = new SelectList(_context.GymBranches, "Id", "Name");
            return View();
        }

        // POST: Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "service-create", hypothesisId = "S1", location = "ServicesController.Create", message = "Create POST called", data = new { name = service?.Name, gymBranchId = service?.GymBranchId }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            if (ModelState.IsValid)
            {
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "service-create", hypothesisId = "S2", location = "ServicesController.Create", message = "ModelState valid, saving", data = new { serviceId = service.Id, name = service.Name, gymBranchId = service.GymBranchId, databaseName = _context.Database.GetDbConnection().Database }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                _context.Add(service);
                
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "service-create", hypothesisId = "S2", location = "ServicesController.Create:74", message = "Before SaveChangesAsync", data = new { serviceId = service.Id }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                
                var rowsAffected = await _context.SaveChangesAsync();
                
                // #region agent log
                try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "service-create", hypothesisId = "S2", location = "ServicesController.Create:79", message = "SaveChangesAsync completed", data = new { rowsAffected = rowsAffected, serviceId = service.Id, databaseName = _context.Database.GetDbConnection().Database }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                return RedirectToAction(nameof(Index));
            }
            // #region agent log
            try { await System.IO.File.AppendAllTextAsync(@"c:\Users\ASUS\Desktop\Hafta2Web\WebProjeGym\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "service-create", hypothesisId = "S3", location = "ServicesController.Create", message = "ModelState invalid", data = new { errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { field = x.Key, errors = x.Value.Errors.Select(e => e.ErrorMessage) }) }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            ViewData["GymBranchId"] = new SelectList(_context.GymBranches, "Id", "Name", service.GymBranchId);
            return View(service);
        }

        // GET: Services/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            ViewData["GymBranchId"] = new SelectList(_context.GymBranches, "Id", "Name", service.GymBranchId);
            return View(service);
        }

        // POST: Services/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service service)
        {
            if (id != service.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.Id))
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
            ViewData["GymBranchId"] = new SelectList(_context.GymBranches, "Id", "Name", service.GymBranchId);
            return View(service);
        }

        // GET: Services/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.GymBranch)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // POST: Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                // Hizmetin randevularını manuel olarak sil
                var appointments = await _context.Appointments
                    .Where(a => a.ServiceId == id)
                    .ToListAsync();
                
                if (appointments.Any())
                {
                    _context.Appointments.RemoveRange(appointments);
                }
                
                // Hizmetin TrainerService ilişkilerini manuel olarak sil
                var trainerServices = await _context.TrainerServices
                    .Where(ts => ts.ServiceId == id)
                    .ToListAsync();
                
                if (trainerServices.Any())
                {
                    _context.TrainerServices.RemoveRange(trainerServices);
                }
                
                // Hizmeti sil
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}


