using Microsoft.Extensions.Configuration;
using Tashlih.Application.Interfaces;
using Twilio;
using Twilio.Rest.Verify.V2.Service;

namespace Tashlih.Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _verifyServiceSid;

        public OtpService(IConfiguration configuration)
        {
            _accountSid = configuration["Twilio:AccountSid"]!;
            _authToken = configuration["Twilio:AuthToken"]!;
            _verifyServiceSid = configuration["Twilio:VerifyServiceSid"]!;
        }

        public async Task<bool> SendOtpSmsAsync(string phone)
        {
            try
            {
                TwilioClient.Init(_accountSid, _authToken);

                var verification = await VerificationResource.CreateAsync(
                    to: phone,
                    channel: "sms",
                    pathServiceSid: _verifyServiceSid
                );

                return verification.Status == "pending";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SMS Error] {ex.Message}");
                return false;
            }
        }

        public async Task<bool> VerifyOtpAsync(string phone, string code)
        {
            try
            {
                TwilioClient.Init(_accountSid, _authToken);

                var check = await VerificationCheckResource.CreateAsync(
                    to: phone,
                    code: code,
                    pathServiceSid: _verifyServiceSid
                );

                return check.Status == "approved";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Verify Error] {ex.Message}");
                return false;
            }
        }
    }
    }
