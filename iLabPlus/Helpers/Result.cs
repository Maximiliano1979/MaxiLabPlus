using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace iLabPlus
{
    [Authorize]

    public class Result<T>
    {
        public T Value { get; }
        public string Error { get; }
        public bool IsSuccess => Error == null;

        protected Result(T value, string error)
        {
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value) => new Result<T>(value, null);
        public static Result<T> Failure(string error) => new Result<T>(default(T), error);
    }


}
