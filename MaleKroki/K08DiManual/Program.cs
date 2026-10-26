// K08DiManual/Program.cs
// Dependency Injection "ręcznie": obiekty składamy w jednym miejscu (Program.cs),
// a klasy dostają swoje zależności konstruktorem.
using K08DiManual;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// 1. Prawdziwy zegar – powitanie zależy od aktualnej godziny
IClock systemClock = new SystemClock();
var greeter = new Greeter(systemClock);
Console.WriteLine(greeter.Greet("Ola"));

// 2. Ten sam Greeter, inny zegar – bez zmiany ani jednej linii w Greeter
IClock morningClock = new FixedClock(new DateTime(2026, 1, 1, 8, 0, 0));
var morningGreeter = new Greeter(morningClock);
Console.WriteLine(morningGreeter.Greet("Ola"));

IClock eveningClock = new FixedClock(new DateTime(2026, 1, 1, 21, 0, 0));
var eveningGreeter = new Greeter(eveningClock);
Console.WriteLine(eveningGreeter.Greet("Ola"));

// Gdyby Greeter miał w środku "new SystemClock()", punkty 2 i 3 byłyby niemożliwe:
// nie dałoby się sprawdzić powitania wieczornego inaczej niż czekając do wieczora.
