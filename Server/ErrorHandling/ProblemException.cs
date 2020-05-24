﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;

namespace Frederikskaj2.Reservations.Server.ErrorHandling
{
    [Serializable]
    public class ProblemException : Exception
    {
        public ProblemException()
        {
        }

        public ProblemException(string message) : base(message)
        {
        }

        public ProblemException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ProblemException(string? type, string? title, int? status, string? detail)
            : this(type, title, status, detail, null)
        {
        }

        public ProblemException(string? type, string? title, int? status, string? detail, Exception? innerException)
            : base(GetMessage(type, title, detail), innerException)
        {
            Type = type ?? "about:blank";
            Title = title ?? "Internal Server Error";
            Status = status ?? 500;
            Detail = detail;
        }

        protected ProblemException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info is null)
                throw new ArgumentNullException(nameof(info));

            Type = info.GetString(nameof(Type));
            Title = info.GetString(nameof(Title));
            Status = info.GetInt32(nameof(Status));
            Detail = info.GetString(nameof(Detail));
        }

        [SuppressMessage("Naming", "CA1721:Property names should not match get methods", Justification = "The term 'type' is used in RFC 7807 and there is no confusion about this property and the GetType() method.")]
        public string? Type { get; }
        public string? Title { get; }
        public int Status { get; }
        public string? Detail { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info is null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(Type), Type);
            info.AddValue(nameof(Title), Title);
            info.AddValue(nameof(Status), Status);
            info.AddValue(nameof(Detail), Detail);
            base.GetObjectData(info, context);
        }

        private static string GetMessage(string? type, string? title, string? detail)
        {
            var stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(type))
                stringBuilder.Append(type);
            if (!string.IsNullOrEmpty(title))
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append(' ');
                stringBuilder.Append(title);
            }
            if (!string.IsNullOrEmpty(detail))
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append(": ");
                stringBuilder.Append(detail);
            }
            return stringBuilder.ToString();
        }
    }
}
