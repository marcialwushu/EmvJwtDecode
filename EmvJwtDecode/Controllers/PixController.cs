﻿using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;

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
        /// Decodifica uma linha Pix Copia-e-Cola e retorna os campos em formato JSON.
        /// </summary>
        /// <param name="request">Objeto contendo a linha Pix Copia-e-Cola a ser decodificada.
        /// <br>Esta rotina desmembra uma linha Pix Copia-e-cola</br>
        /// <br>Todas as linhas são compostas por [ID do campo][Tamanho do campo com dois dígitos][Conteúdo do campo] conforme o padrão EMV®1 QRCPS Merchant Presented</br>
        /// </param>
        /// <returns>Um objeto JSON contendo os campos decodificados.</returns>
        /// <response code="200">Retorna os campos decodificados.</response>
        /// <response code="400">Se a linha Pix estiver faltando ou for inválida.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> DecodePix([FromBody] PixRequest request)
        {
            //var emv = DecodeEmv(request.PixCopiaCola);
            string getUri = null;
            
            var emv = DecodeBrcode(request.PixCopiaCola);




            if (emv.TryGetValue("26", out object subField26) && subField26 is Dictionary<string, object> subField26Dict)
            {
                if (subField26Dict.TryGetValue("25", out object url))
                {
                    Console.WriteLine("URL: " + url);
                    getUri = url.ToString();
                }
                else
                {
                    Console.WriteLine("URL não encontrada.");
                }
            }
            else
            {
                Console.WriteLine("Campo '26' não encontrado.");
            }


            var urlPayload = "https://" + getUri;

            //// Get the URL from the EMV data to fetch the JWS payload
            /// A ser verificado se é necessário
            //if (string.IsNullOrEmpty(urlPayload))
            //{
            //    return BadRequest("Invalid EMV data: URL not found.");
            //}

            //var client = new RestClient(urlPayload);
            //var response = await client.ExecuteAsync(new RestRequest());

            //if (!response.IsSuccessful)
            //{
                
            //    return NotFound("Failed to get JWS payload.");
            //}

            //var jwt = response.Content;
            //var handler = new JwtSecurityTokenHandler();
            //var jsonToken = handler.ReadJwtToken(jwt);
            //var claims = jsonToken.Claims.Select(c => new { c.Type, c.Value });

            return Ok(new
            {
                Emv = emv
                //Jwt = claims
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

            var rsult = DecodeBrcode(pixCopiaCola);

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
            try
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
            catch (Exception ex)
            {
                throw ex;
            }
            
        }


        private static string ExtractTransactionPayloadUrl(string brCode)
        {
            // Extrair o valor do campo TransactionAmount, que deve conter a URL
            string transactionAmountPattern = @"(?<=\|54..\|)[\w:/.\-]+(?=\|)";
            Match transactionAmountMatch = Regex.Match(brCode, transactionAmountPattern);
            if (transactionAmountMatch.Success)
            {
                return transactionAmountMatch.Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Decodifica uma linha Pix Copia-e-Cola.
        /// </summary>
        /// <param name="brcode">A linha Pix Copia-e-Cola a ser decodificada.</param>
        /// <param name="recursivamente">Se deve decodificar recursivamente os campos.</param>
        /// <returns>Um dicionário contendo os campos decodificados.</returns>
        public static Dictionary<string, object> DecodeBrcode(string brcode, bool recursivamente = true)
        {
            //if (string.IsNullOrEmpty(brcode)) throw new ArgumentNullException(nameof(brcode));
            if (brcode == string.Empty) return new Dictionary<string, object>();

            int n = 0;
            var retorno = new Dictionary<string, object>();

            try
            {
                if (!recursivamente)
                {
                    Console.WriteLine("Decode nao recursivo " + brcode);
                }

                while (n < brcode.Length)
                {
                    if (n + 4 > brcode.Length) return null; // Verificar se há pelo menos 4 caracteres restantes
                    string codigo = brcode.Substring(n, 2);
                    n += 2;
                    if (!int.TryParse(brcode.Substring(n, 2), out int tamanho))
                    {
                        return null;
                    }
                    n += 2;
                    if (n + tamanho > brcode.Length) return null; // Verificar se o comprimento especificado está dentro dos limites
                    string valor = brcode.Substring(n, tamanho);
                    Console.WriteLine($"Cod: {codigo} T: {tamanho} Data: {valor}");
                    n += tamanho;

                    if (codigo == "26")
                    {
                        retorno["26"] = DecodeBrcode(valor, false);
                    }
                    else if (recursivamente && Regex.IsMatch(valor, @"^[0-9]{4}.+$") && codigo != "54")
                    {
                        retorno[codigo] = DecodeBrcode(valor);
                    }
                    else
                    {
                        retorno[codigo] = valor;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return retorno;
        }


        /// <summary>
        /// A linha Pix Copia-e-Cola a ser decodificada.
        /// </summary>
        /// <example>00020126580014br.gov.bcb.pix01140123456789052040000530398654041.235802BR5913Fulano de Tal6008Brasilia62070503***63041D3D</example>
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
