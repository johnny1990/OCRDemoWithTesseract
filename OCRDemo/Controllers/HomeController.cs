using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OCRDemo.Models;
using Tesseract;

namespace OCRDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        private readonly List<string> _knownRecipients = new()
        {
            "Test 1",
            "Test 2",
            "Test 3",
            "Test 4"
        };

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessImage([FromForm] OCRModel model)
        {
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var filePath = Path.GetTempFileName();
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                var ocrResult = ExtractTextFromImage(filePath);

                // Match recipients
                var suggestions = _knownRecipients
                    .Where(name => ocrResult.Contains(name, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return Json(new { ocrResult, suggestions });
            }

            _logger.LogError("Error. No image uploaded.");
            return BadRequest("No image uploaded.");
        }

        #region Methods
        private string ExtractTextFromImage(string imagePath)
        {
            try
            {
                using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
                using var img = Pix.LoadFromFile(imagePath);
                using var page = engine.Process(img);
                return page.GetText();
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
