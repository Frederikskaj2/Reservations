﻿using System;

namespace Frederikskaj2.Reservations.Server.ErrorHandling
{
    public class Problem
    {
        public Problem(string type, string title, int status)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Status = status;
        }

        public string Type { get; }
        public string Title { get; }
        public int Status { get; }

        public void Throw(string detail) => ThrowException(detail, null);
        public void Throw(Exception exception) => ThrowException(null, exception);
        public void Throw(string detail, Exception exception) => ThrowException(detail, exception);

        private void ThrowException(string? detail, Exception? innerException)
            => throw new ProblemException(Type, Title, Status, detail, innerException);
    }
}
