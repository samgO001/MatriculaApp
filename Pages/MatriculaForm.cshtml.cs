using Microsoft.AspNetCore.Mvc.RazorPages;
using MatriculaApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MatriculaApp.Pages
{
    public class MatriculaFormModel : PageModel
    {
        public List<Matricula> Matriculas { get; set; } = new();

        public void OnGet()
        {
            CargarMatriculas();
        }

        private void CargarMatriculas()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "matriculas.json");
            if (System.IO.File.Exists(path))
            {
                var json = System.IO.File.ReadAllText(path);
                Matriculas = JsonSerializer.Deserialize<List<Matricula>>(json) ?? new();
            }
        }
    }
}
