# Frederikskaj 2 lokalereservation

[![Build](https://github.com/Frederikskaj2/Reservations/workflows/Build/badge.svg)](https://github.com/Frederikskaj2/Reservations/actions?query=workflow%3ABuild)
[![Backup](https://github.com/Frederikskaj2/Reservations/workflows/Backup/badge.svg)](https://github.com/Frederikskaj2/Reservations/actions?query=workflow%3ABackup)

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

En almindelig bruger kan oprette bestillinger med en eller flere reservationer af lokaler og kan aflyse sine reservationer såfremt reglerne tillader det.

En bruger kan tildeles en eller flere af følgende administrative roller:

| Rolle | Adgang |
| - | - | 
| Bestillinger | Kan se bestillinger af afbestille reservationer. Kan oprette og afbestille reservationer på vegne af grundejerforeningen. |
| Betaling | Kan registrere ind- og udbetalinger. |
| Opgørelse | Kan lave en opgørelse når en reservation er slut. |
| Bogføring | Kan se kassekladden med posteringer. |
| Rengøring | Kan se rengøringsplanen. |
| Nøglebokskoder | Kan se nøglebokskoderne. |
| Brugeradminitration | Kan filføje og fjerne roller fra brugere og slette brugere. |
