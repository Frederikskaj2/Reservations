using System;

namespace Frederikskaj2.Reservations.Shared
{
    public class User
    {
        public User(
            int id, string email, string fullName, string phone, bool isEmailConfirmed, bool isAdministrator,
            bool isPendingDelete, Apartment? apartment)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Value cannot be null or empty.", nameof(email));
            if (string.IsNullOrEmpty(fullName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(fullName));
            if (string.IsNullOrEmpty(phone))
                throw new ArgumentException("Value cannot be null or empty.", nameof(phone));

            Id = id;
            Email = email;
            FullName = fullName;
            Phone = phone;
            IsEmailConfirmed = isEmailConfirmed;
            IsAdministrator = isAdministrator;
            IsPendingDelete = isPendingDelete;
            Apartment = apartment;
        }

        public int Id { get; }
        public string Email { get; }
        public string FullName { get; }
        public string Phone { get; }
        public bool IsEmailConfirmed { get; }
        public bool IsAdministrator { get; }
        public bool IsPendingDelete { get; }
        public Apartment? Apartment { get; }
    }
}