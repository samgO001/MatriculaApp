using Microsoft.AspNetCore.Mvc.RazorPages;
using MatriculaApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class MatriculasModel : PageModel
{
    public List<Matricula> Matriculas { get; set; } = new List<Matricula>();

    public void OnGet()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "matriculas.json");
        if (System.IO.File.Exists(path))
        {
            var json = System.IO.File.ReadAllText(path);
            Matriculas = JsonSerializer.Deserialize<List<Matricula>>(json) ?? new List<Matricula>();
        }
    }
}
