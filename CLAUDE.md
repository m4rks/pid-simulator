# PID Simulator

Aplikacja do nauki: symulacja obiektu cieplnego (dom, później zbiornik z wodą) z piecem i regulatorem PID.
Backend w C# (.NET 9) według Clean Architecture + CQRS, frontend w React, który rysuje obiekt i wykresy.

Autor uczy się programowania i architektury. Pracuj małymi krokami, przy nowych wzorcach krótko
wyjaśniaj, *dlaczego* coś jest zrobione w dany sposób, i nie generuj całej aplikacji naraz.

## Struktura

```
src/
  Pid.Domain/          # model fizyczny, regulatory, symulacja; czysty C#, zero zależności
  Pid.Application/     # CQRS: komendy, zapytania, handlery, DTO, interfejsy (np. repozytorium)
  Pid.Infrastructure/  # repozytorium in-memory, pętla symulacji (BackgroundService)
  Pid.Api/             # ASP.NET Core Minimal API + SignalR hub, Program.cs, DI
web/                   # React + TypeScript + Vite
tests/
  Pid.Domain.Tests/
  Pid.Application.Tests/
```

Kierunek zależności: `Api -> Infrastructure -> Application -> Domain`. Domain nie zna żadnej innej warstwy.

## Domain: model

Kluczowe abstrakcje. Dzięki nim można wymieniać obiekt i regulator bez zmian w reszcie kodu:

- `IThermalPlant`: obiekt sterowany. `double Step(double heatPowerW, double ambientC, TimeSpan dt)` zwraca nową temperaturę.
  - `HouseModel`: model 1. rzędu: `C * dT/dt = P - (T - T_amb) / R` (C: pojemność cieplna, R: opór cieplny).
  - później: `WaterTankModel`.
- `IController`: regulator. `double Compute(double setpoint, double measured, TimeSpan dt)` zwraca sygnał sterujący 0..1.
  - `PidController`: parametry `Kp`, `Ki`, `Kd`, ograniczenie wyjścia i anti-windup.
  - później np. `OnOffController`.
- `Heater` (piec): moc maksymalna; zamienia sygnał 0..1 na moc w W.
- `Simulation`: aggregate root. Trzyma obiekt, regulator, piec, temperaturę zadaną i temperaturę otoczenia,
  czas oraz historię próbek. Metoda `Tick(TimeSpan dt)` wykonuje jeden krok: pomiar -> regulator -> piec -> obiekt.
- Value objects z walidacją w konstruktorze: `PidParameters`, `HouseParameters`. Temperatury to `double` z sufiksem `C` w nazwie.

Zasady domeny:
- Brak `DateTime.Now`, wątków i I/O. Czas `dt` jest zawsze przekazywany z zewnątrz, więc wszystko da się testować deterministycznie.
- Zmiana stanu tylko przez metody agregatu (`ChangeSetpoint`, `ChangeAmbient`, `ReplaceController`, ...), bez publicznych setterów.
- Błędne dane, np. ujemne `Kp` lub ujemna pojemność cieplna, rzucają wyjątek domenowy.

## Application: CQRS

Własne proste interfejsy zamiast MediatR (lepiej do nauki i bez problemów licencyjnych):
`ICommand`, `ICommandHandler<TCommand>`, `IQuery<TResult>`, `IQueryHandler<TQuery, TResult>`.

- **Komendy** zmieniają stan i nie zwracają danych (najwyżej Id). Przykłady:
  `CreateSimulation`, `SetSetpoint`, `SetAmbientTemperature`, `UpdatePidParameters`,
  `ChangePlantModel`, `ChangeController`, `StartSimulation`, `PauseSimulation`, `ResetSimulation`.
- **Zapytania** tylko czytają i zwracają DTO, nigdy encje domenowe. Przykłady: `GetSimulationState`, `GetSimulationHistory`.
- Każda komenda ma osobny walidator (FluentValidation) uruchamiany przed handlerem.
- Struktura folderów według funkcji: `Simulations/Commands/SetSetpoint/{SetSetpointCommand, Handler, Validator}.cs`.

## Infrastructure i Api

- Na start repozytorium in-memory (`ISimulationRepository`); baza danych dopiero, gdy będzie potrzebna.
- `SimulationRunner : BackgroundService` co stały interwał wywołuje `Tick` i publikuje stan przez SignalR.
- Endpointy są cienkie: mapują żądanie na komendę lub zapytanie i wywołują handler, bez logiki biznesowej.
  REST dla komend i zapytań, SignalR (`/hubs/simulation`) do strumienia stanu na żywo.

## Frontend (web/)

- React + TypeScript + Vite. TanStack Query dla zapytań REST, `@microsoft/signalr` dla danych na żywo.
- Wykresy (temperatura, wartość zadana, moc pieca): Recharts.
- Wizualizacja obiektu jako komponent SVG wybierany według typu modelu (`HouseView`, później `TankView`).
- Podział: `api/` (klienci HTTP i SignalR), `features/simulation/` (komponenty i hooki), `components/` (wspólne).
- Typy DTO odpowiadają DTO z backendu.

## Konwencje

- Kod, nazwy i komentarze po angielsku; rozmowa po polsku.
- Nullable reference types włączone, `record` dla DTO, komend i zapytań.
- Jednostki w nazwach, gdy nie są oczywiste: `PowerW`, `ResistanceKPerW`, `DtSeconds`.
- Testy xUnit: każdy handler oraz domena, np. odpowiedź skokowa PID, bilans energii modelu domu,
  brak windup przy nasyceniu pieca.

## Komendy

```
dotnet build
dotnet test
dotnet run --project src/Pid.Api
cd web && npm install && npm run dev
```
