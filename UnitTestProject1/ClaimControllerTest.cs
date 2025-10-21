using Microsoft.VisualStudio.TestTools.UnitTesting;
using CMCSGUI.Controllers;
using CMCSGUI.Models;
using Microsoft.AspNetCore.Hosting;
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
        private ClaimsController _controller;
        private Mock<IWebHostEnvironment> _mockEnv;

        [TestInitialize]
        public void Setup()
        {
            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(e => e.WebRootPath).Returns("wwwroot");
            _controller = new ClaimsController(_mockEnv.Object);
        }

        [TestMethod]
        public void Index_ReturnsClaimsInDescendingOrder()
        {
            var claim1 = new Claim { Id = Guid.NewGuid(), ClaimDate = DateTime.UtcNow.AddDays(-1) };
            var claim2 = new Claim { Id = Guid.NewGuid(), ClaimDate = DateTime.UtcNow };

            typeof(ClaimsController)
                .GetField("_claims", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.SetValue(null, new List<Claim> { claim1, claim2 });

            var result = _controller.Index() as ViewResult;
            Assert.IsNotNull(result);

            var model = result.Model as List<Claim>;
            Assert.IsNotNull(model);
            Assert.AreEqual(claim2.Id, model.First().Id);
        }

        [TestMethod]
        public void Pending_ReturnsOnlySubmittedOrVerifiedClaims()
        {
            var claims = new List<Claim>
            {
                new Claim { Status = "Submitted" },
                new Claim { Status = "Verified" },
                new Claim { Status = "Approved" }
            };

            typeof(ClaimsController)
                .GetField("_claims", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.SetValue(null, claims);

            var result = _controller.Pending() as ViewResult;
            Assert.IsNotNull(result);

            var model = result.Model as List<Claim>;
            Assert.IsNotNull(model);
            Assert.IsTrue(model.All(c => c.Status == "Submitted" || c.Status == "Verified"));
            Assert.AreEqual(2, model.Count);
        }

        [TestMethod]
        public void Create_InvalidModel_ReturnsViewWithSameClaim()
        {
            var claim = new Claim { HoursWorked = -5, HourlyRate = 10 };
            _controller.ModelState.AddModelError("Error", "Invalid");

            var result = _controller.Create(claim, null).Result as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreSame(claim, result.Model);
        }

        [TestMethod]
        public void Approve_ValidId_UpdatesStatusToApproved()
        {
            var claim = new Claim { Id = Guid.NewGuid(), Status = "Submitted" };

            typeof(ClaimsController)
                .GetField("_claims", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.SetValue(null, new List<Claim> { claim });

            var result = _controller.Approve(claim.Id) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Approved", claim.Status);
            Assert.IsTrue(claim.LastUpdated <= DateTime.UtcNow);
        }

        [TestMethod]
        public void Reject_InvalidId_ReturnsNotFound()
        {
            var fakeId = Guid.NewGuid();
            typeof(ClaimsController)
                .GetField("_claims", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.SetValue(null, new List<Claim>());

            var result = _controller.Reject(fakeId, "Reason");
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}