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
        void CrearPasswordHash(string password,
            out byte[] hash, out byte[] salt);
        bool VerificarPassword(string password,
            byte[] hash, byte[] salt);
        string GenerarToken(UsuarioSistema usuario);
    }

    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;

        public AuthService(IConfiguration config)
            => _config = config;

        public void CrearPasswordHash(string password,
            out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(
                Encoding.UTF8.GetBytes(password));
        }

        public bool VerificarPassword(string password,
            byte[] hash, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);
            var computed = hmac.ComputeHash(
                Encoding.UTF8.GetBytes(password));
            return computed.SequenceEqual(hash);
        }

        public string GenerarToken(UsuarioSistema usuario)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _config["Jwt:Key"]!));

            var creds = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha512Signature);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier,
                    usuario.Id.ToString()),
                new(ClaimTypes.Email,
                    usuario.Correo),
                new(ClaimTypes.Name,
                    usuario.Nombre),
                new(ClaimTypes.Role,
                    usuario.Rol)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(
                    double.Parse(_config["Jwt:ExpirationHours"]!)),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}
