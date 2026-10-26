// K09DiContainer/Program.cs
// Dependency Injection z kontenerem: zamiast ręcznie pisać "new", rejestrujemy
// typy w ServiceCollection, a kontener sam składa obiekty wraz z ich zależnościami.
using K09DiContainer;
using Microsoft.Extensions.DependencyInjection;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// 1. Rejestracja: "gdy ktoś poprosi o IClock, daj SystemClock"
var services = new ServiceCollection();
services.AddSingleton<IClock, SystemClock>();   // jeden zegar na całą aplikację
services.AddTransient<Greeter>();               // nowy Greeter przy każdym żądaniu

// Cykle życia do porównania:
services.AddSingleton<Counter>();                       // ten sam obiekt zawsze
services.AddTransient<TransientCounter>();              // nowy obiekt za każdym razem

// 2. Budujemy kontener
ServiceProvider provider = services.BuildServiceProvider();

// 3. Prosimy o Greeter. Kontener widzi, że konstruktor wymaga IClock,
//    znajduje rejestrację IClock → SystemClock i sam go tworzy i przekazuje.
Greeter greeter = provider.GetRequiredService<Greeter>();
Console.WriteLine(greeter.Greet("Ola"));

// 4. Singleton: dwa żądania, ten sam obiekt (ten sam Id)
Counter s1 = provider.GetRequiredService<Counter>();
Counter s2 = provider.GetRequiredService<Counter>();
s1.Value = 10;
Console.WriteLine($"Singleton: {s1.Id} / {s2.Id}  → ten sam obiekt: {ReferenceEquals(s1, s2)}, s2.Value = {s2.Value}");

// 5. Transient: dwa żądania, dwa różne obiekty (różne Id)
TransientCounter t1 = provider.GetRequiredService<TransientCounter>();
TransientCounter t2 = provider.GetRequiredService<TransientCounter>();
t1.Value = 10;
Console.WriteLine($"Transient: {t1.Id} / {t2.Id}  → ten sam obiekt: {ReferenceEquals(t1, t2)}, t2.Value = {t2.Value}");

// 6. Podmiana implementacji = jedna linia w rejestracji. Odkomentuj i uruchom ponownie:
// services.AddSingleton<IClock>(new FixedClock(new DateTime(2026, 1, 1, 21, 0, 0)));
// (musi być PRZED BuildServiceProvider; przy dwóch rejestracjach IClock wygrywa ostatnia)

// 7. Eksperyment: żądanie typu, którego nie zarejestrowano, kończy się wyjątkiem
//    "No service for type ... has been registered". Odkomentuj:
// provider.GetRequiredService<FixedClock>();
