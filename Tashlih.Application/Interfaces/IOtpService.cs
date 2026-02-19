using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tashlih.Application.Interfaces
{
    public interface IOtpService
    {
        Task<bool> SendOtpSmsAsync(string phone);
        Task<bool> VerifyOtpAsync(string phone, string code);

    }

}
