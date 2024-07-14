using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System.IdentityModel.Tokens.Jwt;

namespace EmvJwtDecode.Controllers
{
    /// <summary>
    /// API controller for decoding PIX codes
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class PixController : ControllerBase
    {
        /// <summary>
        /// Decode a PIX code and return the EMV and JWT data
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DecodePix([FromBody] PixRequest request)
        {
            var emv = DecodeEmv(request.PixCopiaCola);
            var url = emv.Url;

            if (string.IsNullOrEmpty(url))
            {
                return BadRequest("Invalid EMV data: URL not found.");
            }

            var client = new RestClient(url);
            var response = await client.ExecuteAsync(new RestRequest());

            if (!response.IsSuccessful)
            {
                return BadRequest("Failed to get JWS payload.");
            }

            var jwt = response.Content;
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(jwt);
            var claims = jsonToken.Claims.Select(c => new { c.Type, c.Value });

            return Ok(new
            {
                Emv = emv,
                Jwt = claims
            });
        }

        /// <summary>
        /// Decode the EMV data from a PIX code
        /// </summary>
        /// <param name="pixCopiaCola"></param>
        /// <returns></returns>
        private Emv DecodeEmv(string pixCopiaCola)
        {
            var emv = new Emv();
            int index = 0;

            while (index < pixCopiaCola.Length)
            {
                var tag = pixCopiaCola.Substring(index, 2);
                index += 2;
                var length = int.Parse(pixCopiaCola.Substring(index, 2));
                index += 2;
                var value = pixCopiaCola.Substring(index, length);
                index += length;

                emv.Fields[tag] = value;

                if (tag == "26")
                {
                    DecodeSubFields(value, emv);
                }
            }

            if (emv.SubFields.ContainsKey("2564"))
            {
                emv.Url = emv.SubFields["2564"];
            }

            return emv;
        }

        /// <summary>
        /// Decode subfields from a tag 26 value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="emv"></param>
        private void DecodeSubFields(string value, Emv emv)
        {
            int index = 0;
            while (index < value.Length)
            {
                if (index + 18 > value.Length) break; // Ensure there is enough length for the tag
                var tag = value.Substring(index, 18);
                index += 4;

                if (index + 4 > value.Length) break; // Ensure there is enough length for the length field
                var lengthString = value.Substring(index, 14);
                if (!int.TryParse(lengthString, out int length)) break;
                index += 2;

                if (index + length > value.Length) break; // Ensure there is enough length for the value field
                var subValue = value.Substring(index, length);
                index += length;

                emv.SubFields[tag] = subValue;
            }
        }

        /// <summary>
        /// Pix request model
        /// </summary>
        public class PixRequest
        {
            public string PixCopiaCola { get; set; }
        }

        /// <summary>
        /// EMV data model
        /// </summary>
        public class Emv
        {
            public string Url { get; set; }
            public Dictionary<string, string> Fields { get; set; }
            public Dictionary<string, string> SubFields { get; set; }

            public Emv()
            {
                Fields = new Dictionary<string, string>();
                SubFields = new Dictionary<string, string>();
            }
        }
    }
}
