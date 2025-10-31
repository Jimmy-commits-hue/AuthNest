using AuthApiBackend.DTOs.ResponseDtos;
using System.Collections.Concurrent;

namespace AuthApiBackend.Utilities
{
    public static class SendStudentNumberToClient
    {

        public static ConcurrentQueue<ForgottenLoginNumber> resendForgettedLoginNumber = new();
    };
}
