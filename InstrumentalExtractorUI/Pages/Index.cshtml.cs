using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace InstrumentalExtractorUI.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public IFormFile? AudioFile { get; set; }

        public string? Message { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (AudioFile == null || AudioFile.Length == 0)
            {
                Message = "Please upload a valid audio file.";
                return Page();
            }

            // Save file to a temporary location
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            Directory.CreateDirectory(uploadsFolder);
            var filePath = Path.Combine(uploadsFolder, AudioFile.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await AudioFile.CopyToAsync(stream);
            }

            // Run Python script
            string output = RunPythonScript(filePath);

            Message = $"File processed! Output: {output}";
            return Page();
        }
        private string RunPythonScript(string audioFilePath)
        {
            string venvActivate = @"C:\Users\zakar\Desktop\Repo\InstrumentalExtractorUI\back_end\instrument_extraction\Audio_venv\Scripts\activate.bat";
            string pythonScript = @"C:\Users\zakar\Desktop\Repo\InstrumentalExtractorUI\back_end\instrument_extraction\generate_instrumental.py";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{venvActivate} && python \"{pythonScript}\" \"{audioFilePath}\"\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = @"C:\Users\zakar\Desktop\Repo\InstrumentalExtractorUI\back_end\instrument_extraction"
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process == null) return "Error: Process failed to start.";

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                return string.IsNullOrEmpty(error) ? output : $"Error: {error}";
            }
        }


    }
}
