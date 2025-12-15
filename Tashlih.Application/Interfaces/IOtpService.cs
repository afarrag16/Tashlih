using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tashlih.Application.Interfaces
{
    public interface IOtpService
    {
        string GenerateOtp();
        Task<bool> SendOtpSmsAsync(string phone, string otp);
    }
}
