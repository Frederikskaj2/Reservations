﻿namespace Frederikskaj2.Reservations.Shared
{
    public class PayOut
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int ApartmentId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public int Amount { get; set; }
    }
}