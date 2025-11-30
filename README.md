# MatriculaApp

## Portada

Matriculas


Samuel Guarin Osorio y Darly zambrano zambrano

**Aplicación Web de Gestión de Matrículas**

*Una solución integral para registrar y gestionar información de estudiantes con generación automática de PDF*

Desarrollado con: **ASP.NET Core 10.0** | **C# 10** | **QuestPDF 2025.7.4**

---

## Introducción

MatriculaApp es una aplicación web moderna desarrollada con **ASP.NET Core** que permite gestionar el registro de matrículas estudiantiles. 

### Propósito Principal
La aplicación proporciona una interfaz intuitiva para:
- Registrar nuevos estudiantes con sus datos personales y académicos
- Guardar automáticamente la información en una base de datos JSON
- Generar documentos PDF de constancia de matrícula al momento del registro
- Visualizar un listado completo de todos los estudiantes matriculados
- Descargar PDFs de matrículas previas
### Características Destacadas
**Interfaz Moderna**: Diseño responsivo con CSS Grid y estilos profesionales  
**Generación de PDF**: Constancias automáticas con encabezado, información del estudiante y firma  
**Persistencia de Datos**: Almacenamiento en formato JSON legible  
**Experiencia Fluida**: Fetch API para descarga de PDFs sin recarga de página  
**Tabla Dinámica**: Listado actualizado en tiempo real de todos los registros 
📱 **Tabla Dinámica**: Listado actualizado en tiempo real de todos los registros  

---

## Índice

1. [Introducción](#introducción)
2. [Requisitos Previos](#requisitos-previos)
3. [Instalación y Configuración](#instalación-y-configuración)
4. [Estructura del Proyecto](#estructura-del-proyecto)
5. [Componentes Principales](#componentes-principales)
   - [Modelo de Datos](#modelo-de-datos)
   - [Controlador](#controlador)
   - [Vistas Razor](#vistas-razor)
   - [Generación de PDF](#generación-de-pdf)
6. [Uso de la Aplicación](#uso-de-la-aplicación)
7. [Código Fuente Destacado](#código-fuente-destacado)
8. [Conclusiones](#conclusiones)


---

## Requisitos Previos

Para ejecutar esta aplicación, necesitas tener instalado:

- **.NET 10.0 SDK** o superior
- **Visual Studio Code** o **Visual Studio 2022+** (opcional)
- **Git** para clonar el repositorio
- **Navegador web moderno** (Chrome, Firefox, Edge, Safari)

### Verificar Instalación

```powershell
dotnet --version
git --version
```

---

## Instalación y Configuración

### 1. Clonar el Repositorio

```powershell
git clone https://github.com/samgO001/MatriculaApp.git
cd MatriculaApp
```

### 2. Restaurar Dependencias

```powershell
dotnet restore
```

### 3. Compilar el Proyecto

```powershell
dotnet build
```

### 4. Ejecutar la Aplicación

```powershell
dotnet run --urls http://localhost:5138
```

La aplicación estará disponible en: **http://localhost:5138/MatriculaForm**

### 5. Acceder a la Aplicación

Abre tu navegador favorito y ve a:
```
http://localhost:5138/MatriculaForm
```

---

## Estructura del Proyecto

```
MatriculaApp/
├── Controllers/
│   └── MatriculaController.cs          # Lógica de negocio
├── Models/
│   └── Matricula.cs                    # Modelo de datos
├── Pages/
│   ├── MatriculaForm.cshtml            # Interfaz principal
│   ├── MatriculaForm.cshtml.cs         # PageModel
│   ├── Matriculas.cshtml               # Página de listado (supercedida)
│   ├── Error.cshtml
│   ├── Index.cshtml
│   ├── Privacy.cshtml
│   └── Shared/
│       └── _Layout.cshtml              # Layout maestro
├── wwwroot/
│   ├── css/
│   │   └── site.css
│   ├── js/
│   │   └── site.js
│   └── lib/                            # Bootstrap, jQuery
├── Program.cs                          # Configuración de startup
├── appsettings.json                    # Configuración
├── MatriculaApp.csproj                 # Archivo del proyecto
├── matriculas.json                     # Base de datos (generada)
└── README.md                           # Este archivo
```

---

## Componentes Principales

### Modelo de Datos

**Archivo**: `Models/Matricula.cs`

```csharp
namespace MatriculaApp.Models
{
    public class Matricula
    {
        public string Nombre { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Curso { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
```

**Descripción**: Clase POCO (Plain Old CLR Object) que representa un estudiante matriculado con sus propiedades básicas inicializadas para evitar warnings de nulabilidad.

---

### Controlador

**Archivo**: `Controllers/MatriculaController.cs`

#### Método Guardar (POST)

```csharp
[HttpPost("Guardar")]
public IActionResult Guardar([FromForm] Matricula datos)
{
    // 1. Leer matrículas existentes
    var matriculas = new List<Matricula>();
    if (System.IO.File.Exists("matriculas.json"))
    {
        var json = System.IO.File.ReadAllText("matriculas.json");
        matriculas = System.Text.Json.JsonSerializer.Deserialize<List<Matricula>>(json) ?? new();
    }

    // 2. Agregar nueva matrícula
    matriculas.Add(datos);

    // 3. Guardar en JSON
    var jsonActualizado = System.Text.Json.JsonSerializer.Serialize(
        matriculas, 
        new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
    );
    System.IO.File.WriteAllText("matriculas.json", jsonActualizado);

    // 4. Generar y retornar PDF
    var pdfBytes = GeneratePDF(datos);
    return File(pdfBytes, "application/pdf", $"matricula_{datos.Documento}.pdf");
}
```

#### Método Download (GET)

```csharp
[HttpGet("Download/{index}")]
public IActionResult Download(int index)
{
    var json = System.IO.File.ReadAllText("matriculas.json");
    var matriculas = System.Text.Json.JsonSerializer.Deserialize<List<Matricula>>(json);
    
    if (index < 0 || index >= matriculas.Count)
        return NotFound();

    var pdfBytes = GeneratePDF(matriculas[index]);
    return File(pdfBytes, "application/pdf", $"matricula_{matriculas[index].Documento}.pdf");
}
```

---

### Vistas Razor

**Archivo**: `Pages/MatriculaForm.cshtml`

#### Estructura HTML (Secciones)

```html
<!-- FORMULARIO -->
<div class="form-section">
    <h2>Registrar Nueva Matrícula</h2>
    <form id="matriculaForm">
        <div class="form-grid">
            <input type="text" name="Nombre" placeholder="Nombre Completo" required>
            <input type="text" name="Documento" placeholder="Documento de Identidad" required>
            <input type="number" name="Edad" placeholder="Edad" required>
            <input type="email" name="Email" placeholder="Correo Electrónico" required>
            <input type="text" name="Curso" placeholder="Curso o Programa" required>
        </div>
        <button type="submit" class="btn-submit">Registrar y Descargar PDF</button>
    </form>
</div>

<!-- TABLA DE MATRÍCULAS -->
<div class="list-section">
    <h2>Matrículas Registradas</h2>
    @if (Model?.Matriculas?.Count == 0)
    {
        <p class="empty-message">No hay matrículas registradas aún.</p>
    }
    else
    {
        <table class="matriculas-table">
            <thead>
                <tr>
                    <th>#</th>
                    <th>Nombre</th>
                    <th>Documento</th>
                    <th>Curso</th>
                    <th>Edad</th>
                    <th>Email</th>
                    <th>Acción</th>
                </tr>
            </thead>
            <tbody>
                @for (int i = 0; i < Model.Matriculas.Count; i++)
                {
                    <tr>
                        <td>@(i + 1)</td>
                        <td>@Model.Matriculas[i].Nombre</td>
                        <td>@Model.Matriculas[i].Documento</td>
                        <td>@Model.Matriculas[i].Curso</td>
                        <td>@Model.Matriculas[i].Edad</td>
                        <td>@Model.Matriculas[i].Email</td>
                        <td>
                            <a href="/Matricula/Download/@i" class="btn-download">
                                Descargar PDF
                            </a>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    }
</div>
```

---

### Generación de PDF

**Archivo**: `Controllers/MatriculaController.cs` - Método `GeneratePDF`

```csharp
private byte[] GeneratePDF(Matricula m)
{
    var document = Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(40);

            page.Header().Column(column =>
            {
                column.Item().Text("CONSTANCIA DE MATRÍCULA")
                    .FontSize(20)
                    .Bold()
                    .AlignCenter();

                column.Item().Text($"Emitida: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(10)
                    .AlignRight();
            });

            page.Content().Column(column =>
            {
                // Sección 1: Información del Estudiante
                column.Item().PaddingVertical(10).Text("INFORMACIÓN DEL ESTUDIANTE")
                    .Bold().FontSize(12);

                column.Item().Column(inner =>
                {
                    inner.Item().Text($"Nombre: {m.Nombre}");
                    inner.Item().Text($"Documento: {m.Documento}");
                    inner.Item().Text($"Edad: {m.Edad} años");
                    inner.Item().Text($"Email: {m.Email}");
                });

                // Sección 2: Información Académica
                column.Item().PaddingVertical(10).Text("INFORMACIÓN ACADÉMICA")
                    .Bold().FontSize(12);

                column.Item().Text($"Curso/Programa: {m.Curso}");
            });

            page.Footer().AlignCenter().Text(
                "Este documento certifica la inscripción del estudiante en nuestro programa académico."
            ).FontSize(8);
        });
    });

    return document.GeneratePdf();
}
```

---

## Uso de la Aplicación

### Paso 1: Acceder a la Aplicación

```
http://localhost:5138/MatriculaForm
```

### Paso 2: Completar el Formulario

- **Nombre**: Nombre completo del estudiante
- **Documento**: Cédula o ID de identificación
- **Edad**: Edad en años
- **Email**: Correo electrónico
- **Curso**: Programa académico (ej: "Programación Web", "ASP.NET Core")

### Paso 3: Registrar y Descargar

1. Haz clic en **"Registrar y Descargar PDF"**
2. Se guardará automáticamente en `matriculas.json`
3. Se descargará el PDF de la constancia
4. La página se recargará mostrando la nueva matrícula en la tabla

### Paso 4: Descargar PDFs Previos

En la tabla de matrículas, haz clic en **"Descargar PDF"** para cualquier fila.

---

## Código Fuente Destacado

### JavaScript - Manejo de Formulario

```javascript
document.getElementById('matriculaForm').addEventListener('submit', function(e) {
    e.preventDefault();
    
    const formData = new FormData(this);
    
    fetch('/Matricula/Guardar', {
        method: 'POST',
        body: formData
    })
    .then(response => response.blob())
    .then(blob => {
        // Crear descarga
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'matricula.pdf';
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        a.remove();
        
        // Recargar página después de 1 segundo
        setTimeout(() => { 
            window.location.reload(); 
        }, 1000);
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Error al guardar la matrícula');
    });
});
```

### CSS - Estilos Principales

```css
.form-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 20px;
}

.matriculas-table {
    width: 100%;
    border-collapse: collapse;
    margin-top: 20px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.matriculas-table tbody tr:hover {
    background-color: #f0f7ff;
    transition: all 0.3s ease;
}

.btn-download {
    background-color: #2196F3;
    color: white;
    padding: 8px 12px;
    border-radius: 4px;
    text-decoration: none;
    font-size: 12px;
    transition: background-color 0.3s;
}

.btn-download:hover {
    background-color: #1976D2;
}
```

---

## Conclusiones

### Logros Alcanzados

**Aplicación Completamente Funcional**: MatriculaApp cumple con todos los requisitos de gestión de matrículas.

**Tecnología Moderna**: Utiliza ASP.NET Core 10.0 con C# 10, garantizando rendimiento y seguridad.

**Generación Automática de Documentos**: QuestPDF proporciona una forma elegante y eficiente de crear PDFs profesionales.

**Interfaz Intuitiva**: Diseño responsive que funciona en todos los dispositivos.

**Persistencia de Datos**: Sistema JSON simple pero efectivo para almacenar registros.





