using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Domain.Response
{
    public class BaseResponse<T> where T : class
    {
        public T Result { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }

        private BaseResponse(T result, bool success, string message, int statusCode)
        {
            Result = result;
            Success = success;
            Message = message;
            StatusCode = statusCode;
        }

        public BaseResponse(T result) : this(result, true, "OK", 200) { }
        public BaseResponse(string message) : this(default, false, message, 400) { }
    }
}
