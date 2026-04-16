# jak organizować kod w klasie, aby był czytelny i łatwy do zrozumienia? Oto przykładowa struktura organizacji kodu w klasie:

// 1. Fields (private)
private double _currentAngle;

// 2. Constructors
public Actuator(...) { }

// 3. Properties (public API)
public double CurrentAngle => _currentAngle;

// 4. Public methods
public void SetTargetAngle(...) { }

// 5. Protected methods
protected void SetState(...) { }

// 6. Private methods (helpers)
private void ValidateSomething() { }

---

# Nie zawsze warto robic encje
## czasem mozna stworzyc prosty DTO, albo nawet zwykly rekord, ale w przypadku bardziej zlozonych obiektow, ktore maja swoje zachowanie, warto stworzyc encje. Wtedy mozemy umiescic w niej logike biznesowa, a nie tylko dane.

Czy Command to encja?
W tym, co opisujesz (typ operacji + cel + parametry do wysłania), to zwykle wygląda jak wiadomość / intencja — w DDD częściej modeluje się to jako:

rekord / value object (niemutowalny), albo
hierarchia małych typów (RunCommand, StopCommand, …),
a Entity wtedy, gdy komenda ma tożsamość i cykl życia w systemie (np. „komenda #123 w kolejce, status: wysłana/błąd”). Jeśli to tylko „co wyślę po serialu”, encja nie jest obowiązkowa — nie musisz jej sztucznie dodawać.