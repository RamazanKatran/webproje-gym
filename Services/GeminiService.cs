using System.Net.Http.Json;
using System.Text.Json;

namespace WebProjeGym.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiService> _logger;
        private const int MaxRetries = 5;
        private const string ModelName = "gemini-flash-latest";

        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("Gemini API Key bulunamadı. appsettings.json dosyasında 'Gemini:ApiKey' ayarını yapın.");
            }

            _httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/");
        }

        public async Task<string> GetExerciseRecommendationAsync(int heightCm, float weightKg, int age, string bodyType, string goal)
        {
            var prompt = BuildExercisePrompt(heightCm, weightKg, age, bodyType, goal);
            return await CallGeminiWithRetryAsync(prompt);
        }

        public async Task<string> GetDietRecommendationAsync(int heightCm, float weightKg, int age, string bodyType, string goal)
        {
            var prompt = BuildDietPrompt(heightCm, weightKg, age, bodyType, goal);
            return await CallGeminiWithRetryAsync(prompt);
        }

        private string BuildExercisePrompt(int heightCm, float weightKg, int age, string bodyType, string goal)
        {
            return $@"Sen profesyonel bir fitness uzmanı ve diyetisyensin. Aşağıdaki bilgilere göre şöyle bir haftalık egzersiz programı hazırla:

Kullanıcı Bilgileri:
- Boy: {heightCm} cm
- Kilo: {weightKg} kg
- Yaş: {age} yaş
- Vücut Tipi: {bodyType}
- Hedef: {goal}

Lütfen şöyle bir haftalık egzersiz programı yaz:
- Program gün gün (Pazartesi'den Pazar'a) olmalı
- Her gün için set/tekrar detayları belirtilmeli
- Markdown formatında tablo veya listeler kullan
- Program kişinin hedefine uygun olmalı
- Güvenli ve etkili egzersizler öner
- Isınma ve soğuma önerileri de dahil et

Yanıtını sadece Markdown formatında ver, başka açıklama ekleme.";
        }

        private string BuildDietPrompt(int heightCm, float weightKg, int age, string bodyType, string goal)
        {
            return $@"Sen profesyonel bir fitness uzmanı ve diyetisyensin. Aşağıdaki bilgilere göre şöyle bir haftalık beslenme programı hazırla:

Kullanıcı Bilgileri:
- Boy: {heightCm} cm
- Kilo: {weightKg} kg
- Yaş: {age} yaş
- Vücut Tipi: {bodyType}
- Hedef: {goal}

Lütfen şöyle bir haftalık beslenme programı yaz:
- Program gün gün (Pazartesi'den Pazar'a) olmalı
- Her öğün için (kahvaltı, öğle yemeği, akşam yemeği, ara öğünler) detaylı menü
- Kalori ve makro besin değerleri (protein, karbonhidrat, yağ) belirtilmeli
- Markdown formatında tablo veya listeler kullan
- Program kişinin hedefine uygun olmalı
- Sağlıklı ve dengeli beslenme önerileri

Yanıtını sadece Markdown formatında ver, başka açıklama ekleme.";
        }

        private async Task<string> CallGeminiWithRetryAsync(string prompt)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = prompt
                            }
                        }
                    }
                }
            };

            var apiUrl = $"models/{ModelName}:generateContent?key={_apiKey}";

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync(apiUrl, requestBody);

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        var isQuotaExceeded = errorContent.Contains("quota", StringComparison.OrdinalIgnoreCase) || 
                                             errorContent.Contains("RESOURCE_EXHAUSTED", StringComparison.OrdinalIgnoreCase);
                        
                        if (isQuotaExceeded)
                        {
                            throw new InvalidOperationException("API kotanız dolmuş. Lütfen Google AI Studio'dan kotanızı kontrol edin veya farklı bir API anahtarı kullanın.");
                        }
                        
                        if (attempt == MaxRetries)
                        {
                            throw new InvalidOperationException("Sistem şu an çok yoğun, lütfen 1 dakika sonra tekrar deneyiniz.");
                        }

                        var delay = (int)Math.Pow(2, attempt - 1) * 1000;
                        _logger.LogWarning($"API rate limit hatası (429). {delay}ms bekleniyor. Deneme: {attempt}/{MaxRetries}");
                        await Task.Delay(delay);
                        continue;
                    }

                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    if (string.IsNullOrWhiteSpace(responseContent))
                    {
                        throw new InvalidOperationException("API'den boş yanıt alındı.");
                    }

                    var jsonDoc = JsonDocument.Parse(responseContent);
                    
                    if (!jsonDoc.RootElement.TryGetProperty("candidates", out var candidates) || 
                        candidates.GetArrayLength() == 0)
                    {
                        _logger.LogError($"API yanıtında candidates bulunamadı. Yanıt: {responseContent}");
                        throw new InvalidOperationException("API yanıtı beklenen formatta değil. Lütfen tekrar deneyiniz.");
                    }

                    var candidate = candidates[0];
                    
                    if (!candidate.TryGetProperty("content", out var content) ||
                        !content.TryGetProperty("parts", out var parts) ||
                        parts.GetArrayLength() == 0)
                    {
                        _logger.LogError($"API yanıtında content/parts bulunamadı. Yanıt: {responseContent}");
                        throw new InvalidOperationException("API yanıtı beklenen formatta değil. Lütfen tekrar deneyiniz.");
                    }

                    var part = parts[0];
                    if (!part.TryGetProperty("text", out var textElement))
                    {
                        _logger.LogError($"API yanıtında text bulunamadı. Yanıt: {responseContent}");
                        throw new InvalidOperationException("API yanıtı beklenen formatta değil. Lütfen tekrar deneyiniz.");
                    }

                    var text = textElement.GetString();
                    return text ?? "Yanıt alınamadı.";
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("429") && attempt < MaxRetries)
                {
                    var delay = (int)Math.Pow(2, attempt - 1) * 1000;
                    _logger.LogWarning($"HTTP isteği başarısız (429). {delay}ms bekleniyor. Deneme: {attempt}/{MaxRetries}");
                    await Task.Delay(delay);
                    continue;
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("kota") || ex.Message.Contains("quota"))
                {
                    // Kota hatası: Retry yapma, direkt fırlat
                    _logger.LogError(ex, "Gemini API kota hatası - Retry yapılmıyor");
                    throw;
                }
                catch (Exception ex)
                {
                    if (attempt == MaxRetries)
                    {
                        _logger.LogError(ex, $"Gemini API çağrısı {MaxRetries} deneme sonunda başarısız oldu.");
                        throw;
                    }

                    var delay = (int)Math.Pow(2, attempt - 1) * 1000;
                    _logger.LogWarning($"Hata oluştu. {delay}ms bekleniyor. Deneme: {attempt}/{MaxRetries}. Hata: {ex.Message}");
                    await Task.Delay(delay);
                }
            }

            throw new InvalidOperationException("Sistem şu an çok yoğun, lütfen 1 dakika sonra tekrar deneyiniz.");
        }
    }
}

