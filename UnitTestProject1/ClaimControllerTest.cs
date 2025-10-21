using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMCSGUI.Tests
{
    [TestClass]
    public class ClaimsControllerTests
    {
        // Define a simple Claim class for testing purposes
        public class Claim
        {
            public Guid Id { get; set; }
            public DateTime ClaimDate { get; set; }
            public string Status { get; set; } = "";
            public decimal HoursWorked { get; set; }
            public decimal HourlyRate { get; set; }
            public DateTime LastUpdated { get; set; }
        }

        // Simulated controller for testing
        public class TestClaimsController
        {
            private static List<Claim> _claims = new List<Claim>();
            private readonly string _webRootPath;

            public TestClaimsController(string webRootPath)
            {
                _webRootPath = webRootPath;
            }

            public List<Claim> Index()
            {
                return _claims.OrderByDescending(c => c.ClaimDate).ToList();
            }

            public List<Claim> Pending()
            {
                return _claims.Where(c => c.Status == "Submitted" || c.Status == "Verified").ToList();
            }

            public IActionResult Create(Claim claim, object file)
            {
                if (claim.HoursWorked <= 0 || claim.HourlyRate <= 0)
                {
                    return new BadRequestResult();
                }

                claim.Id = Guid.NewGuid();
                claim.ClaimDate = DateTime.UtcNow;
                claim.Status = "Submitted";
                _claims.Add(claim);
                return new RedirectResult("/");
            }

            public IActionResult Approve(Guid id)
            {
                var claim = _claims.FirstOrDefault(c => c.Id == id);
                if (claim == null)
                    return new NotFoundResult();

                claim.Status = "Approved";
                claim.LastUpdated = DateTime.UtcNow;
                return new RedirectToActionResult("Index", null, null);
            }

            public IActionResult Reject(Guid id, string reason)
            {
                var claim = _claims.FirstOrDefault(c => c.Id == id);
                if (claim == null)
                    return new NotFoundResult();

                claim.Status = "Rejected";
                claim.LastUpdated = DateTime.UtcNow;
                return new RedirectToActionResult("Index", null, null);
            }

            public static void SetClaims(List<Claim> claims)
            {
                _claims = claims;
            }
        }

        private TestClaimsController _controller;

        [TestInitialize]
        public void Setup()
        {
            _controller = new TestClaimsController("wwwroot");
        }

        [TestMethod]
        public void Index_ReturnsClaimsInDescendingOrder()
        {
            // Arrange
            var claim1 = new Claim { Id = Guid.NewGuid(), ClaimDate = DateTime.UtcNow.AddDays(-1) };
            var claim2 = new Claim { Id = Guid.NewGuid(), ClaimDate = DateTime.UtcNow };

            var claimsList = new List<Claim> { claim1, claim2 };
            TestClaimsController.SetClaims(claimsList);

            // Act
            var result = _controller.Index();

            // Assert
            Assert.AreEqual(claim2.Id, result.First().Id);
            Assert.AreEqual(claim1.Id, result.Last().Id);
        }

        [TestMethod]
        public void Pending_ReturnsOnlySubmittedOrVerifiedClaims()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim { Status = "Submitted" },
                new Claim { Status = "Verified" },
                new Claim { Status = "Approved" }
            };
            TestClaimsController.SetClaims(claims);

            // Act
            var result = _controller.Pending();

            // Assert
            Assert.IsTrue(result.All(c => c.Status == "Submitted" || c.Status == "Verified"));
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void Create_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var claim = new Claim { HoursWorked = -5, HourlyRate = 10 };

            // Act
            var result = _controller.Create(claim, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public void Create_ValidModel_AddsClaim()
        {
            // Arrange
            var claim = new Claim { HoursWorked = 40, HourlyRate = 25 };

            // Act
            var result = _controller.Create(claim, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectResult));
        }

        [TestMethod]
        public void Approve_ValidId_UpdatesStatusToApproved()
        {
            // Arrange
            var claim = new Claim { Id = Guid.NewGuid(), Status = "Submitted" };
            var claimsList = new List<Claim> { claim };
            TestClaimsController.SetClaims(claimsList);

            // Act
            var result = _controller.Approve(claim.Id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            Assert.AreEqual("Approved", claim.Status);
            Assert.IsTrue(claim.LastUpdated <= DateTime.UtcNow);
        }

        [TestMethod]
        public void Approve_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var fakeId = Guid.NewGuid();
            TestClaimsController.SetClaims(new List<Claim>());

            // Act
            var result = _controller.Approve(fakeId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void Reject_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var fakeId = Guid.NewGuid();
            TestClaimsController.SetClaims(new List<Claim>());

            // Act
            var result = _controller.Reject(fakeId, "Test reason");

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void Reject_ValidId_UpdatesStatusToRejected()
        {
            // Arrange
            var claim = new Claim { Id = Guid.NewGuid(), Status = "Submitted" };
            var claimsList = new List<Claim> { claim };
            TestClaimsController.SetClaims(claimsList);

            // Act
            var result = _controller.Reject(claim.Id, "Invalid hours");

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            Assert.AreEqual("Rejected", claim.Status);
            Assert.IsTrue(claim.LastUpdated <= DateTime.UtcNow);
        }
    }
}