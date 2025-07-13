using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace WordToPdf.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public DocumentController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("convert-to-pdf")]
        public async Task<IActionResult> ConvertToPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Create input/output folders
            var inputDir = Path.Combine(_env.ContentRootPath, "input");
            var outputDir = Path.Combine(_env.ContentRootPath, "output");
            Directory.CreateDirectory(inputDir);
            Directory.CreateDirectory(outputDir);

            // Save the uploaded file
            var inputPath = Path.Combine(inputDir, file.FileName);
            await using (var stream = new FileStream(inputPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Convert to PDF using LibreOffice
            var libreOfficeProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "libreoffice",
                    Arguments = $"--headless --convert-to pdf --outdir \"{outputDir}\" \"{inputPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            libreOfficeProcess.Start();
            string output = await libreOfficeProcess.StandardOutput.ReadToEndAsync();
            string error = await libreOfficeProcess.StandardError.ReadToEndAsync();
            await libreOfficeProcess.WaitForExitAsync();

            if (libreOfficeProcess.ExitCode != 0)
                return StatusCode(500, $"Conversion failed: {error}");

            // Return PDF file
            var pdfFileName = Path.GetFileNameWithoutExtension(file.FileName) + ".pdf";
            var pdfFilePath = Path.Combine(outputDir, pdfFileName);

            if (!System.IO.File.Exists(pdfFilePath))
                return StatusCode(500, "PDF file not found after conversion.");

            var memory = new MemoryStream();
            await using (var stream = new FileStream(pdfFilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;
            return File(memory, "application/pdf", pdfFileName);
        }
    }
}
