using EmvJwtDecode.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EmvJwtDecode.Controllers.PixController;

namespace EmvJwtDecoderApi.Tests
{
    [TestFixture]
    public class PixControllerTests
    {
        private PixController _controller;

        [SetUp]
        public void Setup()
        {
            _controller = new PixController();
        }

        [Test]
        public void DecodeBrcode_ValidBrcode_ReturnsExpectedDictionary()
        {
            string brcode = "00020126580014br.gov.bcb.pix01140123456789052040000530398654041.235802BR5913Fulano de Tal6008Brasilia62070503***63041D3D";
            var expected = new Dictionary<string, object>
            {
                { "00", "01" },
                { "01", "12" },
                { "26", new Dictionary<string, object>
                    {
                        { "25", "qr.iugu.com/public/payload/v2/C170077E4A7B4E248BBFAE2D769F1811" },
                        { "00", "br.gov.bcb.pix" }
                    }
                },
                { "52", "0000" },
                { "53", "986" },
                { "54", "45.00" },
                { "58", "BR" },
                { "59", "CLOSE FANS TECNOLOGIA LTD" },
                { "60", "BELO HORIZONTE" },
                { "62", new Dictionary<string, object>
                    {
                        { "05", "***" }
                    }
                },
                { "63", "F03B" }
            };

            var result = DecodeBrcode(brcode);

            Assert.IsNotNull(result);
            Assert.AreEqual(expected.Count, result.Count);

            foreach (var key in expected.Keys)
            {
                Assert.IsTrue(result.ContainsKey(key));
                var expectedValue = expected[key];
                var actualValue = result[key];

                if (expectedValue is Dictionary<string, object> expectedDict && actualValue is Dictionary<string, object> actualDict)
                {
                    Assert.AreEqual(expectedDict.Count, actualDict.Count);
                    foreach (var subKey in expectedDict.Keys)
                    {
                        Assert.IsTrue(actualDict.ContainsKey(subKey));
                        Assert.AreEqual(expectedDict[subKey], actualDict[subKey]);
                    }
                }
                else
                {
                    Assert.AreEqual(expectedValue, actualValue);
                }
            }
        }

        [Test]
        public void DecodeBrcode_InvalidBrcode_ReturnsNull()
        {
            string brcode = "00020126";
            var result = DecodeBrcode(brcode);
            Assert.IsNull(result);
        }

        [Test]
        public void DecodeBrcode_NullBrcode_ThrowsArgumentNullException()
        {
            string brcode = null;
            Assert.Throws<System.ArgumentNullException>(() => DecodeBrcode(brcode));
        }

        [Test]
        public void DecodeBrcode_EmptyBrcode_ReturnsEmptyDictionary()
        {
            string brcode = string.Empty;
            var result = DecodeBrcode(brcode);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void DecodeBrcode_ValidBrcodeWithoutRecursion_ReturnsExpectedDictionary()
        {
            string brcode = "00020126580014br.gov.bcb.pix01140123456789052040000530398654041.235802BR5913Fulano de Tal6008Brasilia62070503***63041D3D";
            var expected = new Dictionary<string, object>
            {
                { "00", "01" },
                { "01", "12" },
                { "26", "01140123456789052040000530398654041.235802BR5913Fulano de Tal6008Brasilia62070503***63041D3D" },
                { "52", "0000" },
                { "53", "986" },
                { "54", "45.00" },
                { "58", "BR" },
                { "59", "CLOSE FANS TECNOLOGIA LTD" },
                { "60", "BELO HORIZONTE" },
                { "62", "05***" },
                { "63", "F03B" }
            };

            var result = DecodeBrcode(brcode, false);

            Assert.IsNotNull(result);
            Assert.AreEqual(expected.Count, result.Count);

            foreach (var key in expected.Keys)
            {
                Assert.IsTrue(result.ContainsKey(key));
                Assert.AreEqual(expected[key].ToString(), result[key].ToString());
            }
        }


    }
}
