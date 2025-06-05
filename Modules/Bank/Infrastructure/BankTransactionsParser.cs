using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using nietras.SeparatedValues;
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;

namespace Frederikskaj2.Reservations.Bank;

class BankTransactionsParser : IBankTransactionsParser
{
    static readonly LocalDatePattern datePattern = LocalDatePattern.CreateWithInvariantCulture("dd.MM.yyyy");

    static readonly SepReaderOptions sepReaderOptions =
        Sep.Reader(options => options with { CultureInfo = CultureInfo.GetCultureInfo("da-DK"), Unescape = true });

    [SuppressMessage("Design", "CA1031", Justification = "Parsing can fail in many ways and it's hard to know the full set of possible exceptions.")]
    public Either<Failure<ImportError>, Seq<ImportBankTransaction>> ParseBankTransactions(string transactions)
    {
        try
        {
            // Seq tries to be lazy so to be able to catch exceptions it's
            // necessary to create an array.
            return ParseCsv(transactions).ToArray().ToSeq();
        }
        catch (Exception exception)
        {
            return Failure.New(HttpStatusCode.UnprocessableEntity, ImportError.InvalidRequest, exception.Message);
        }

        static IEnumerable<ImportBankTransaction> ParseCsv(string csv)
        {
            using var reader = sepReaderOptions.FromText(csv);
            foreach (var row in reader)
            {
                // ReSharper disable StringLiteralTypo
                var date = datePattern.Parse(row["Bogført dato"].ToString()).Value;
                var text = row["Tekst"].ToString();
                var amount = row["Beløb i DKK"].Parse<decimal>();
                var balance = row["Bogført saldo i DKK"].Parse<decimal>();
                // ReSharper restore StringLiteralTypo
                yield return new(date, text, Amount.FromDecimal(amount), Amount.FromDecimal(balance));
            }
        }
    }
}
