using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;


[ApiController]
[Route("api/[controller]")]
public class ClavePublicaController : ControllerBase
{
    private readonly string _clavePath = Path.Combine(Directory.GetCurrentDirectory(), "Claves", "clave_publica.pem");

    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ClaveRequest request)
    {
        var userPath = Path.Combine(Directory.GetCurrentDirectory(), "Claves", $"{request.UsuarioId}_clave_publica.pem");

        Directory.CreateDirectory(Path.GetDirectoryName(userPath)!);
        await System.IO.File.WriteAllTextAsync(userPath, request.ClavePublica);

        return Ok("Clave pública recibida y guardada.");
    }


    [HttpGet]
    public IActionResult Get()
    {
        if (!System.IO.File.Exists(_clavePath))
            return NotFound("Clave no encontrada.");

        var contenido = System.IO.File.ReadAllText(_clavePath);
        return Ok(contenido);
    }

    // 👇 Aquí agregas el nuevo método de cifrado
    [HttpPost("cifrar")]
    public IActionResult Cifrar([FromQuery] string usuarioId, [FromBody] string texto)
    {
        var clavePath = Path.Combine(_clavePath, $"{usuarioId}_clave_publica.pem");

        if (!System.IO.File.Exists(clavePath))
            return NotFound("Clave pública no encontrada para el usuario");

        var clavePublica = System.IO.File.ReadAllText(clavePath);

        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(clavePublica.ToCharArray());

            byte[] data = Encoding.UTF8.GetBytes(texto);
            byte[] encrypted = rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
            string base64Encrypted = Convert.ToBase64String(encrypted);

            return Ok(base64Encrypted);
        }
        catch (Exception ex)
        {
            return BadRequest("Error al cifrar: " + ex.Message);
        }
    }

}
public class ClaveRequest
{
    public string UsuarioId { get; set; } = string.Empty;
    public string ClavePublica { get; set; } = string.Empty;
}

