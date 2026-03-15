using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class AnalysisModel : PageModel
{
    public string CodigoCita { get; set; }
    public string NombrePaciente { get; set; }
    public string Fecha { get; set; }

    public void OnGet(string cita)
    {
        CodigoCita = cita ?? "C-001";
        NombrePaciente = "María López";
        Fecha = "2026/02/17";
    }

    public IActionResult OnPostUpload()
    {
        var file = Request.Form.Files[0];

        if (file != null)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", file.FileName);

            using var stream = new FileStream(path, FileMode.Create);
            file.CopyTo(stream);
        }

        return new JsonResult(new { success = true });
    }
}