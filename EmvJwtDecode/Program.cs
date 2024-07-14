using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.MapType<Dictionary<string, object>>(() => new OpenApiSchema
    {
        Type = "object",
        AdditionalPropertiesAllowed = true,
        Example = new OpenApiObject
        {
            ["emv"] = new OpenApiObject
            {
                ["26"] = new OpenApiObject
                {
                    ["25"] = new OpenApiString("qr.iugu.com/public/payload/v2/C170077E4A7B4E248BBFAE2D769F1811"),
                    ["00"] = new OpenApiString("br.gov.bcb.pix")
                },
                ["52"] = new OpenApiString("0000"),
                ["53"] = new OpenApiString("986"),
                ["54"] = new OpenApiString("45.00"),
                ["58"] = new OpenApiString("BR"),
                ["59"] = new OpenApiString("CLOSE TECNOLOGIA LTD"),
                ["60"] = new OpenApiString("BELO HORIZONTE"),
                ["62"] = new OpenApiObject
                {
                    ["05"] = new OpenApiString("***")
                },
                ["63"] = new OpenApiString("F03B"),
                ["00"] = new OpenApiString("01"),
                ["01"] = new OpenApiString("12")
            }
        }
    });
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Decoder API",
        Version = "v1",
        Description = "A simple API to decode JWT tokens and Pix Copia-e-Cola codes, returning the header and body as JSON.\n\n" +
                                  "### EMV QR Code\n" +
                                  "EMV (Europay, Mastercard, and Visa) QR Codes are globally recognized standards for payment transactions. These QR codes are structured to hold multiple fields, each serving a specific purpose. " +
                                  "The data within an EMV QR code is formatted as `[ID do campo][Tamanho do campo com dois dígitos][Conteúdo do campo]`, making it a secure and efficient method for encoding transaction details. " +
                                  "This API allows you to decode these fields, revealing the underlying data such as payment amounts, merchant details, and other transaction-specific information.\n\n" +
                                  "### Pix Copia-e-Cola\n" +
                                  "Pix is a real-time payment system created by the Central Bank of Brazil. 'Pix Copia-e-Cola' refers to a convenient method where users can copy a Pix code from one location and paste it into their banking app to make a payment. " +
                                  "These Pix codes are built on the EMV standard and include various fields such as receiver details, transaction amount, and unique identifiers. This API decodes the Pix Copia-e-Cola strings, allowing you to view and verify the detailed contents of the Pix transaction.\n\n" +
                                  "### JWT (JSON Web Token)\n" +
                                  "JWT is a compact, URL-safe means of representing claims to be transferred between two parties. The claims in a JWT are encoded as a JSON object that is used as the payload of a JSON Web Signature (JWS) structure, enabling the claims to be digitally signed or integrity protected with a Message Authentication Code (MAC). " +
                                  "This API decodes JWT tokens, providing access to the header and payload contents in a readable JSON format. This is particularly useful for authentication and authorization processes where you need to verify the information contained within the JWT.\n\n" +
                                  "### Usage\n" +
                                  "This API provides endpoints to decode both EMV and JWT strings, offering a clear view of the encoded information for validation, processing, and troubleshooting purposes. Each endpoint is designed to handle specific decoding tasks efficiently, ensuring accurate and secure processing of the provided data.",
        Contact = new OpenApiContact
        {
            Name = "Your Name",
            Email = "your-email@example.com",
            Url = new Uri("https://your-website.com")
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.MapControllers();

app.Run();
