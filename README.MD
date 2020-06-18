# Frederikskaj 2 lokalereservation

[![Build](https://github.com/Frederikskaj2/Reservations/workflows/Build/badge.svg)](https://github.com/Frederikskaj2/Reservations/actions?query=workflow%3ABuild)
[![Backup](https://github.com/Frederikskaj2/Reservations/workflows/Backup/badge.svg)](https://github.com/Frederikskaj2/Reservations/actions?query=workflow%3ABackup)
[![Actions Panel](https://img.shields.io/badge/actionspanel-enabled-brightgreen)](https://www.actionspanel.app/app/Frederikskaj2/Reservations)

Dette project er en website som anvendes af Grundejerforeningen Frederikskaj 2 til at håndtere reservation af lokaler i fælleshuset.

## Forretningsgang

Sådan forløber en normal reservation:

- En bruger foretager en reservation.
- Brugerens reservation godkendes når betalingen er registreret. Hvis dette ikke sker annulleres reservationen i stedet.
- Efter brug af lokalet laves en opgørelse. Hvis der er skader fratrækkes disse fra depositummet. Hvis skaderne overstiger størrelsen af depositummet håndteres dette ekstra krav udenfor dette system.
- Det resterende depositum udbetales til brugeren.

Sådan forløber en afbestilling:

- En bruger foretager en reservation.
- Brugeren fortryder og annullerer sin reservation.
- Hvis brugeren havde indbetalt depositum og reservationen var godkendt tilbageholdes et afbestillingsgebyr i forbindelse med opgørelsen.

Roller:

1. Almindelig bruger: Kan oprette bestillinger med en eller flere reservationer af lokaler. Kan aflyse reservationer såfremt reglerne tillader det.
2. Bogholder: Kan registrere indbetalinger og udbetalinger.
3. Lokaleadministrator: Kan foretage opgørelser og annullere ubetalte reservationer.

For at forenkle systemet skelnes mellem to typer brugere: dem som ikke er administratorer og dem som er. Den første type bruger har rolle 1 ovenfor. Den anden type bruger har alle rollerne ovenfor.
