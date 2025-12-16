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
            var gyms = await _context.GymBranches
                .Include(g => g.Services)
                .Include(g => g.Trainers)
                .ToListAsync();

            // #region agent log
            System.IO.File.AppendAllText(
                "c:\\Users\\ASUS\\Desktop\\Hafta2Web\\WebProjeGym\\.cursor\\debug.log",
                "{\"sessionId\":\"debug-session\",\"runId\":\"gym-index\",\"hypothesisId\":\"G4\",\"location\":\"GymBranchesController.Index\",\"message\":\"Index loaded\",\"data\":{\"count\":" 
                + gyms.Count + "},\"timestamp\":" + DateTimeOffset.Now.ToUnixTimeMilliseconds() + "}\n");
            // #endregion

            return View(gyms);
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
            var gymBranch = await _context.GymBranches.FindAsync(id);
            if (gymBranch != null)
            {
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


