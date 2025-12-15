using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tashlih.Core.Enums
{
    public class Enums
    {
        public enum UserType { Customer, Supplier, Admin }
        public enum UserStatus { Active, Inactive, Blocked, Pending }
        public enum Language { Ar, En }
        public enum DeviceType { Ios, Android, Web, Other }
        public enum OtpPurpose { Login, Register, ResetPassword, VerifyPhone }
    }
}
