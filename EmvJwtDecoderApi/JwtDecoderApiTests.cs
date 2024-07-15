using EmvJwtDecode.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EmvJwtDecode.Controllers.JwtController;

namespace EmvJwtDecoderApi.Tests
{
    [TestFixture]
    public class JwtDecoderApiTests
    {
        private JwtController _controller;

        [SetUp]
        public void Setup()
        {
            _controller = new JwtController();
        }

        [Test]
        public void DecodeJwt_ValidToken_ReturnsOkResult()
        {
            var request = new JwtRequest
            {
                Token = "eyJhbGciOiJQUzI1NiIsImtpZCI6IjdkOTZiMTU3NDczZDVmOGFlYTBmNTAwNWJmNjllN2JmNGVhZDZhNzQ0OTIxN2Q0NTIxOWZiN2MzZDAzOGFhNTIiLCJ4NXQiOiJ4RVE0YlVocmEzSmZ1MHhMRlNEeGVQN0tKQU0iLCJqa3UiOiJodHRwczovL3FyLml1Z3UuY29tL3B1YmxpYy9qd3QvandrcyJ9.eyJyZXZpc2FvIjoxLCJjYWxlbmRhcmlvIjp7ImNyaWFjYW8iOiIyMDI0LTA2LTI5VDAxOjMzOjM2WiIsImV4cGlyYWNhbyI6OTU1NTgzLCJhcHJlc2VudGFjYW8iOiIyMDI0LTA2LTI5VDAxOjMzOjM2WiJ9LCJ2YWxvciI6eyJvcmlnaW5hbCI6IjQ1LjAwIn0sImNoYXZlIjoiNWNmOTYyOWMtMTE0ZC00ZGIyLThhYTQtNzMzNjRmMjUyMWJkIiwidHhpZCI6IkMxNzAwNzdFNEE3QjRFMjQ4QkJGQUUyRDc2OUYxODExIiwic3RhdHVzIjoiQVRJVkEifQ.lxjkGh_p8ztTeL-V7ui7bFll6aqXiKxfRG39CG03bHJsgyHG5TQ-usWLb8u2Kjvcla92RwQFbr_KL1AihS9VT3cIcBZ-xf3Z2QFoqYfIiAbssPxUbaI4OxOL-MRk1HG7PVxsze9Z3IcyxttTAcY5XM5e1KZ9LhJ_rSkuYpb3QbX1RNm4TwjQVhAM64ZJGhQGhvj1i3y2rcvGSKWhwwuL4PHg9eb8SF3AF8-M7T2wA6el5Dt6EapQ-GrfyTU9pRp-rzlEs2w3oaBSk2z7XvyrtvChh2ZASD7vntOavrbSWaFctxjiTbdcOkpCW_tPandLzx0PdQpm2wElIjSPtIUbDA" // Use a valid JWT for testing
            };

            var result = _controller.DecodeJwt(request) as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public void DecodeJwt_InvalidToken_ReturnsBadRequest()
        {
            var request = new JwtRequest
            {
                Token = "invalid_token"
            };

            var result = _controller.DecodeJwt(request) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test]
        public void DecodeJwt_NullToken_ReturnsBadRequest()
        {
            var request = new JwtRequest
            {
                Token = null
            };

            var result = _controller.DecodeJwt(request) as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

    }
}
