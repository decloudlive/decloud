using System;

namespace DCLCore.DCLws.Models
{
    internal class RpcException : Exception
    {
        public int Code = 0;

        public RpcException(string message, ErrorCode code)
            : base(message)
        {
            Code = (int)code;
        }
    }
}
