using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace EmvJwtDecode.Controllers
{
    /// <summary>
    /// JwtController
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class JwtController : ControllerBase
    {
        /// <summary>
        /// Decodifica um JWT e retorna o header e o body como um objeto JSON.
        /// </summary>
        /// <param name="request">Objeto contendo o token JWT a ser decodificado.</param>
        /// <returns>Um objeto JSON contendo o header e o body do JWT.</returns>
        /// <response code="200">Retorna o header e o body do JWT.</response>
        /// <response code="400">Se o token JWT estiver faltando ou for inválido.</response>
        [HttpPost("decode")]
        public IActionResult DecodeJwt([FromBody] JwtRequest request)
        {
            if (string.IsNullOrEmpty(request.Token))
            {
                return BadRequest("JWT token is required");
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(request.Token) as JwtSecurityToken;

                if (jsonToken == null)
                {
                    return BadRequest("Invalid JWT token");
                }

                var header = jsonToken.Header.ToDictionary(k => k.Key, v => v.Value.ToString());
                var payload = jsonToken.Payload.ToDictionary(k => k.Key, v => v.Value.ToString());

                var result = new
                {
                    Header = header,
                    Body = payload
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error decoding JWT token: {ex.Message}");
            }
        }

        /// <summary>
        /// JWT request model
        /// </summary>
        public class JwtRequest
        {
            public string Token { get; set; }
        }
    }
}
