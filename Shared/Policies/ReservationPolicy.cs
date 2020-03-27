using System;
using System.Linq;
using System.Threading.Tasks;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    internal abstract class ReservationPolicy
    {
        private readonly LocalDate reservationsAreNotAllowedAfter;
        private readonly LocalDate reservationsAreNotAllowedBefore;

        protected ReservationPolicy(
            IDataProvider dataProvider, ReservationsOptions options, PriceOptions priceOptions,
            IDateProvider dateProvider)
        {
            if (dateProvider is null)
                throw new ArgumentNullException(nameof(dateProvider));

            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            PriceOptions = priceOptions ?? throw new ArgumentNullException(nameof(options));

            var today = dateProvider.Today;
            reservationsAreNotAllowedBefore = today.PlusDays(Options.ReservationIsNotAllowedBeforeDaysFromNow);
            reservationsAreNotAllowedAfter = today.PlusDays(Options.ReservationIsNotAllowedAfterDaysFromNow);
        }

        protected IDataProvider DataProvider { get; }
        protected ReservationsOptions Options { get; }
        protected PriceOptions PriceOptions { get; }

        public virtual async Task<(int MinimumDays, int MaximumDays)> GetReservationAllowedNumberOfDays(
            int resourceId, LocalDate date)
        {
            var fromDate = date;
            var toDate = date.PlusDays(Options.MaximumAllowedReservationDays);
            var reservations = await DataProvider.GetReservedDays(resourceId, fromDate, toDate);
            var nextReservation = reservations.FirstOrDefault();
            var maximumDays = nextReservation == null
                ? Options.MaximumAllowedReservationDays
                : (nextReservation.Date - date).Days;
            return (1, maximumDays);
        }

        public virtual Task<bool> IsResourceAvailable(LocalDate date, int resourceId)
            => IsResourceAvailable(date, 1, resourceId);

        public virtual async Task<bool> IsResourceAvailable(LocalDate date, int durationInDays, int resourceId)
        {
            if (date < reservationsAreNotAllowedBefore || date > reservationsAreNotAllowedAfter)
                return false;

            var fromDate = date.PlusDays(-Options.MaximumAllowedReservationDays);
            var toDate = date.PlusDays(durationInDays);
            var reservations = (await DataProvider.GetReservedDays(resourceId, fromDate, toDate)).ToDictionary(reservation => reservation.Date);
            return Enumerable.Range(0, durationInDays).All(i => !reservations.ContainsKey(date.PlusDays(i)));
        }

        public virtual async Task<Price> GetPrice(LocalDate date, int durationInDays)
        {
            var numberOfHighPriceDays = await DataProvider.GetNumberOfHighPriceDays(date, durationInDays);
            var rent = numberOfHighPriceDays*PriceOptions.HighRentPerDay
                       + (durationInDays - numberOfHighPriceDays)*PriceOptions.LowRentPerDay;
            var deposit = numberOfHighPriceDays > 0 ? PriceOptions.HighDeposit : PriceOptions.LowDeposit;
            return new Price
            {
                Rent = rent,
                CleaningFee = PriceOptions.CleaningFee,
                Deposit = deposit,
                CancellationFee = decimal.Zero
            };
        }
    }
}