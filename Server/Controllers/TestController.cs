using System;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Email;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Frederikskaj2.Reservations.Server.Controllers
{
    [Route("test")]
    public class TestController
    {
        private readonly EmailService emailService;

        public TestController(EmailService emailService)
        {
            this.emailService = emailService;
        }

        [HttpGet("email")]
        public async Task Email()
        {
            var user = new User
            {
                Email = "martin@liversage.com",
                FullName = "Martin Liversage"
            };
            await emailService.SendConfirmEmail(user);
        }
    }
}
