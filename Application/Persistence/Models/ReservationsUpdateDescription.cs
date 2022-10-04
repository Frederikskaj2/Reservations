﻿using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;

namespace Frederikskaj2.Reservations.Application;

record ReservationsUpdateDescription(Seq<ReservedDay> UpdatedReservations);
