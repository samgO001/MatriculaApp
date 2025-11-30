using Microsoft.AspNetCore.Mvc;
using MatriculaApp.Models;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.IO;

namespace MatriculaApp.Controllers
{
	[Route("Matricula")]
	public class MatriculaController : Controller
	{
		private const string StorageFile = "matriculas.json";

		[HttpPost("Guardar")]
		public IActionResult Guardar(Matricula datos)
		{
			// Guardar en el archivo como array de matrículas
			List<Matricula> lista = new List<Matricula>();
			if (System.IO.File.Exists(StorageFile))
			{
				var existing = System.IO.File.ReadAllText(StorageFile);
				lista = JsonSerializer.Deserialize<List<Matricula>>(existing) ?? new List<Matricula>();
			}

			lista.Add(datos);
			var options = new JsonSerializerOptions { WriteIndented = true };
			System.IO.File.WriteAllText(StorageFile, JsonSerializer.Serialize(lista, options));

			// Generar PDF para la matrícula recién registrada y devolverlo directamente
			var pdfBytes = GeneratePDF(datos);
			var filename = $"matricula_{(string.IsNullOrWhiteSpace(datos.Documento) ? System.Guid.NewGuid().ToString() : datos.Documento)}.pdf";
			return File(pdfBytes, "application/pdf", filename);
		}

		[HttpGet("Download/{index}")]
		public IActionResult Download(int index)
		{
			if (!System.IO.File.Exists(StorageFile))
				return NotFound("No hay matrículas guardadas.");

			var json = System.IO.File.ReadAllText(StorageFile);
			var lista = JsonSerializer.Deserialize<List<Matricula>>(json) ?? new List<Matricula>();

			if (index < 0 || index >= lista.Count)
				return NotFound();

			var pdfBytes = GeneratePDF(lista[index]);
			var filename = $"matricula_{(string.IsNullOrWhiteSpace(lista[index].Documento) ? index.ToString() : lista[index].Documento)}.pdf";
			return File(pdfBytes, "application/pdf", filename);
		}

		private byte[] GeneratePDF(Matricula m)
		{
			var doc = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Size(PageSizes.A4);
					page.Margin(40);
					page.DefaultTextStyle(x => x.FontSize(11));

					page.Header().AlignCenter().Column(col =>
					{
						col.Item().Text("CONSTANCIA DE MATRÍCULA").FontSize(18).Bold();
						col.Item().Text($"Fecha de emisión: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
					});

					page.Content().Column(col =>
					{
						col.Item().Padding(20).Column(content =>
						{
							content.Item().Text("INFORMACIÓN DEL ESTUDIANTE").FontSize(14).Bold();
							content.Item().PaddingVertical(10);
							content.Item().Text($"Nombre: {m.Nombre}");
							content.Item().Text($"Documento: {m.Documento}");
							content.Item().Text($"Edad: {m.Edad}");
							content.Item().Text($"Email: {m.Email}");

							content.Item().PaddingVertical(15);
							content.Item().Text("INFORMACIÓN ACADÉMICA").FontSize(14).Bold();
							content.Item().PaddingVertical(10);
							content.Item().Text($"Curso: {m.Curso}");
						});
					});

					page.Footer().AlignCenter().Text("Este documento es una constancia oficial de matrícula.").FontSize(9);
				});
			});

			using var ms = new MemoryStream();
			doc.GeneratePdf(ms);
			return ms.ToArray();
		}
	}
}
