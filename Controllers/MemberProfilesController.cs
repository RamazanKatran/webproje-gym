using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProjeGym.Data;
using WebProjeGym.Models;

namespace WebProjeGym.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MemberProfilesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: MemberProfiles
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var memberProfile = await _context.MemberProfiles
                .Include(m => m.ApplicationUser)
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            if (memberProfile == null)
            {
                // Profil yoksa oluştur
                memberProfile = new MemberProfile
                {
                    ApplicationUserId = user.Id,
                    ApplicationUser = user
                };
                _context.MemberProfiles.Add(memberProfile);
                await _context.SaveChangesAsync();
            }

            return View(memberProfile);
        }

        // GET: MemberProfiles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var memberProfile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.Id == id && m.ApplicationUserId == user.Id);

            if (memberProfile == null)
            {
                return NotFound();
            }

            return View(memberProfile);
        }

        // POST: MemberProfiles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MemberProfile memberProfile)
        {
            if (id != memberProfile.Id)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // ApplicationUserId'yi güvenlik için tekrar set et
            var existingProfile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.Id == id && m.ApplicationUserId == user.Id);

            if (existingProfile == null)
            {
                return NotFound();
            }

            // ModelState'den ApplicationUser hatasını kaldır (navigation property, form'dan gelmez)
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("memberProfile.ApplicationUser");

            // Sadece değiştirilebilir alanları güncelle
            existingProfile.HeightCm = memberProfile.HeightCm;
            existingProfile.WeightKg = memberProfile.WeightKg;
            existingProfile.Goal = memberProfile.Goal;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(existingProfile);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberProfileExists(existingProfile.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(existingProfile);
        }

        private bool MemberProfileExists(int id)
        {
            return _context.MemberProfiles.Any(e => e.Id == id);
        }
    }
}

