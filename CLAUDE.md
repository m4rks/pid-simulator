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
  Pid.Api.Tests/       # testy integracyjne: WebApplicationFactory + klient SignalR
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

- **Komendy** zmieniają stan i nie zwracają danych; Id nowej symulacji nadaje wywołujący (`CreateSimulationCommand.SimulationId`).
  Są: `CreateSimulation`, `SetSetpoint`, `SetAmbientTemperature`, `UpdatePidParameters`, `AdvanceSimulation`.
  Planowane: `ChangePlantModel`, `ChangeController`, `StartSimulation`, `PauseSimulation`, `ResetSimulation`.
- Upływ czasu symulacji to też komenda (`AdvanceSimulation`); runner w Infrastructure nie woła domeny bezpośrednio.
- **Zapytania** tylko czytają i zwracają DTO, nigdy encje domenowe. Przykłady: `GetSimulationState`, `GetSimulationHistory`.
- Każda komenda ma osobny walidator (FluentValidation). Uruchamia go dekorator `ValidatingCommandHandler`,
  który `AddCommandHandler<TCommand, THandler>()` w `DependencyInjection.cs` owija wokół handlera.
  Nowy handler trzeba tam zarejestrować.
- Walidatory pilnują zakresów wejścia (`InputLimits`), a domena pilnuje niezmienników (np. dodatnia pojemność cieplna).
- Brak symulacji: `NotFoundException`; błędne dane: `FluentValidation.ValidationException`.
- Struktura folderów według funkcji: `Simulations/Commands/SetSetpoint/{SetSetpointCommand, Handler, Validator}.cs`.

## Infrastructure i Api

- Na start repozytorium in-memory (`InMemorySimulationRepository`, singleton); baza danych dopiero, gdy będzie potrzebna.
- Współbieżność: wszystkie handlery są owinięte w `ExecutionGate` (jeden handler naraz), bo API i runner
  zmieniają te same obiekty w pamięci. Pilnuje tego test `Parallel_commands_and_queries_do_not_corrupt_the_simulation`.
- `SimulationRunner : BackgroundService` (folder `Runner/`): co `TickInterval` (250 ms) wysyła `AdvanceSimulation`
  o `SimulatedTimePerTick` (2 min) dla każdej symulacji i publikuje stan przez `ISimulationStatePublisher`.
  Ustawienia w sekcji `SimulationRunner` w `appsettings.json`.
- Endpointy (`Endpoints/SimulationEndpoints.cs`) są cienkie: mapują żądanie na komendę lub zapytanie i wywołują handler.
  `POST /api/simulations`, `GET /api/simulations[/{id}[/history]]`, `PUT /{id}/setpoint|ambient|pid`, `POST /{id}/advance`.
  Przykłady żądań w `src/Pid.Api/Pid.Api.http`.
- Błędy zamienia na problem details `ApiExceptionHandler`: walidacja i `DomainException` dają 400, `NotFoundException` 404.
- SignalR: hub `/hubs/simulation`; klient wywołuje `Subscribe(simulationId)` i dostaje `StateUpdated(SimulationStateDto)`.
- API działa na `http://localhost:5080`. Frontend w trybie dev łączy się przez proxy Vite, więc CORS nie jest potrzebny.

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
