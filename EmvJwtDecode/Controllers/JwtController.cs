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
        /// Decode a JWT token and return the header and payload
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
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
