using Microsoft.IdentityModel.Tokens;
using SSTDigitalRD.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace SSTDigitalRD.Server.Services
{
    public interface IAuthService
    {
        void CrearPasswordHash(string password, out byte[] hash, out byte[] salt);
        bool VerificarPassword(string password, byte[] hash, byte[] salt);
        string GenerarToken(UsuarioSistema usuario);
        Task EnviarCodigo2FA(string correo, string nombre, string codigo);
    }

    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;

        public AuthService(IConfiguration config)
            => _config = config;

        public void CrearPasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        public bool VerificarPassword(string password, byte[] hash, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computed.SequenceEqual(hash);
        }

        public string GenerarToken(UsuarioSistema usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier,usuario.Id.ToString()),
                new(ClaimTypes.Email,usuario.Correo),
                new(ClaimTypes.Name,usuario.Nombre),
                new(ClaimTypes.Role,usuario.Rol)
            };

            var token = new JwtSecurityToken(issuer: _config["Jwt:Issuer"],
                                            audience: _config["Jwt:Audience"],
                                            claims: claims,
                                            expires: DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:ExpirationHours"]!)),
                                            signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task EnviarCodigo2FA(string correo, string nombre, string codigo)
        {
            // Implementación con SmtpClient — configura en appsettings.json
            var smtpHost = _config["Smtp:Host"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_config["Smtp:Port"] ?? "587");
            var smtpUser = _config["Smtp:Usuario"] ?? "";
            var smtpPass = _config["Smtp:Password"] ?? "";

            using var client = new System.Net.Mail.SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass),
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                
            };

            var mensaje = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(smtpUser, "SST-Digital RD"),
                Subject = "Código de verificación — SST-Digital RD",
                Body = $"Hola {nombre},\n\n" +
                          $"Tu código de verificación es: {codigo}\n\n" +
                          "Este código expira en 10 minutos.\n\n" +
                          "Si no iniciaste sesión, ignora este mensaje.\n\n" +
                          "SST-Digital RD",
                IsBodyHtml = false
            };
            mensaje.To.Add(correo);

            try { 
                await client.SendMailAsync(mensaje); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando 2FA: {ex.Message}");
            }
        }
    }
}
