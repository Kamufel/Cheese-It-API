# Cheese-it-API

![CI](https://github.com/Kamufel/Cheese-it-API/actions/workflows/ci.yml/badge.svg)

REST-API zur Verwaltung einer Hofkäserei: Rezepte, Produktionschargen und deren Weg
vom Ansatz bis zur Verkaufsreife. Entstanden als Portfolio-Projekt, inspiriert von meinem
Hobby, eigene Käseprodukte herzustellen.

## Tech-Stack

- C# / .NET 8, ASP.NET Core Web API
- Entity Framework Core mit MS SQL Server
- Swagger / OpenAPI
- xUnit (Unit- und Integrationstests mit `WebApplicationFactory`, SQLite in-memory)
- Docker Compose für die lokale Datenbank
- GitHub Actions (Build + Tests bei jedem Push)

## Schnellstart

```bash
docker compose up -d
dotnet run --project src/CheeseIt.Api
```

Swagger-UI: die in der Konsole angezeigte URL öffnen, Pfad `/swagger`.

Tests:

```bash
dotnet test
```

## Domain model

- **Recipe**: Name, Milchart, Mindestreifezeit in Tagen
- **CheeseBatch**: Chargennummer, Produktionsdatum, Milchmenge, Status

Eine Charge durchläuft feste Stufen:
`Curdling → Pressing → Brining → Maturing → ReadyForSale`.
Rückschritte oder Überspringen sind nicht möglich, die Regel steckt in der Domänenklasse
(`CheeseBatch.Advance()`) und ist per Unit-Test abgesichert.

## Endpoints

| Methode | Pfad | Beschreibung |
|---|---|---|
| GET / POST | `/api/recipes` | Rezepte auflisten / anlegen |
| GET / PUT / DELETE | `/api/recipes/{id}` | Rezept lesen / ändern / löschen |
| GET / POST | `/api/batches` | Chargen auflisten (Filter `?status=`) / anlegen |
| GET / DELETE | `/api/batches/{id}` | Charge lesen / löschen |
| POST | `/api/batches/{id}/advance` | Charge in die nächste Stufe überführen |

## Hinweise

- **Fehlerfälle als saubere HTTP-Codes**: 400 bei ungültigen Daten, 404 bei unbekannter ID,
  409 bei Konflikten (doppelter Name, Rezept mit Chargen löschen, Charge bereits fertig).
- **DTOs statt Entities** in der API, damit das Datenmodell nicht nach außen durchschlägt.
- **Löschschutz** per `DeleteBehavior.Restrict`, damit Rezepte mit Chargen nicht verschwinden.

## Roadmap

- [ ] EF-Core-Migrations statt `EnsureCreated`
- [ ] Service-Schicht zwischen Controllern und DbContext
- [ ] Paging und Sortierung für Listen
- [ ] Authentifizierung (JWT)
- [ ] Dockerfile für die API selbst
