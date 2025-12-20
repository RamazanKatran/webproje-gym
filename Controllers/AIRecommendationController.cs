using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Net.Http;
using WebProjeGym.Data;
using WebProjeGym.Models;
using WebProjeGym.Services;

namespace WebProjeGym.Controllers
{
    [Authorize]
    public class AIRecommendationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GeminiService _geminiService;

        public AIRecommendationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            GeminiService geminiService)
        {
            _context = context;
            _userManager = userManager;
            _geminiService = geminiService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var memberProfile = await _context.MemberProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            var model = new AIRecommendationRequest();

            // Profil bilgilerini formu önceden doldurmak için kullan
            if (memberProfile != null)
            {
                if (memberProfile.HeightCm.HasValue)
                    model.HeightCm = memberProfile.HeightCm.Value;
                
                if (memberProfile.WeightKg.HasValue)
                    model.WeightKg = memberProfile.WeightKg.Value;
                
                if (!string.IsNullOrEmpty(memberProfile.Goal))
                    model.Goal = memberProfile.Goal;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetRecommendation(AIRecommendationRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                TempData["Error"] = "Lütfen tüm alanları doğru şekilde doldurun. " + string.Join(" ", errors);
                return RedirectToAction(nameof(Index));
            }

            try
            {
                string recommendation;

                if (request.RecommendationType == RecommendationType.Exercise)
                {
                    recommendation = await _geminiService.GetExerciseRecommendationAsync(
                        request.HeightCm,
                        request.WeightKg,
                        request.Age,
                        request.BodyType,
                        request.Goal);
                }
                else
                {
                    recommendation = await _geminiService.GetDietRecommendationAsync(
                        request.HeightCm,
                        request.WeightKg,
                        request.Age,
                        request.BodyType,
                        request.Goal);
                }

                // Öneriyi veritabanına kaydet
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    try
                    {
                        var aiRecommendation = new AIRecommendation
                        {
                            ApplicationUserId = user.Id,
                            RecommendationType = request.RecommendationType,
                            Content = recommendation,
                            HeightCm = request.HeightCm,
                            WeightKg = request.WeightKg,
                            Age = request.Age,
                            BodyType = request.BodyType,
                            Goal = request.Goal,
                            CreatedAt = DateTime.Now
                        };

                        _context.AIRecommendations.Add(aiRecommendation);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception dbEx)
                    {
                        // Veritabanı hatası - öneriyi göster ama kaydetme
                        // Inner exception detaylarını logla
                        var innerMessage = dbEx.InnerException != null ? dbEx.InnerException.Message : "Inner exception yok";
                        System.Diagnostics.Debug.WriteLine($"AI önerisi kayıt hatası: {dbEx.Message}, Inner: {innerMessage}");
                        // Hata mesajını kullanıcıya gösterme, sadece öneriyi göster
                    }
                }

                ViewBag.Recommendation = recommendation;
                ViewBag.RecommendationType = request.RecommendationType == RecommendationType.Exercise 
                    ? "Egzersiz Programı" 
                    : "Beslenme Programı";

                return View("Result");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = $"API bağlantı hatası: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
            catch (JsonException)
            {
                TempData["Error"] = "API yanıtı işlenirken bir hata oluştu. Lütfen tekrar deneyiniz.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var recommendations = await _context.AIRecommendations
                .Where(r => r.ApplicationUserId == user.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(recommendations);
        }

        public async Task<IActionResult> ViewRecommendation(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var recommendation = await _context.AIRecommendations
                .FirstOrDefaultAsync(r => r.Id == id && r.ApplicationUserId == user.Id);

            if (recommendation == null)
            {
                return NotFound();
            }

            ViewBag.Recommendation = recommendation.Content;
            ViewBag.RecommendationType = recommendation.RecommendationType == RecommendationType.Exercise 
                ? "Egzersiz Programı" 
                : "Beslenme Programı";

            return View("Result");
        }
    }
}

