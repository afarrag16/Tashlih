using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tashlih.Application.Interfaces;
using Tashlih.Infrastructure.Models;


namespace Tashlih.Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        public string GenerateOtp()
        {
            var random = new Random();
            return random.Next(1000, 9999).ToString("D4");
        }

        public async Task<bool> SendOtpSmsAsync(string phone, string otp)
        {
            // TODO: ربط مع SMS Gateway
            Console.WriteLine($"[SMS] Sending OTP {otp} to {phone}");
            await Task.Delay(100);
            return true;
        }
    }
}
