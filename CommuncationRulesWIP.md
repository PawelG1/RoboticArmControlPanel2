4. Komunikacja JSON i parser komend

Mapowanie ACTUATOR -> ObjectIdx:
- 0 = Stepper 1 (TB6600)
- 1 = Stepper 2 (DM556)
- 2 = Servo 1
- 3 = Servo 2

Wszystkie kąty podajemy w stopniach.

Komendy wejściowe (PC -> sterownik)

1) MOVE (ruch do kąta docelowego)

{
    "Type": "MOVE",
    "ManipulatedObject": "ACTUATOR",
    "ObjectIdx": 0,
    "TargetAngle": 100,
    "Speed": 200
}

Znaczenie:
- TargetAngle: docelowy kąt
- Speed: wartość komendowa 1..255 (w firmware mapowana na Hz)

Uwagi:
- Dla stepperów TargetAngle jest normalizowany do zakresu 0..360.
- Dla serw firmware ogranicza TargetAngle do 0..180.

2) STOP (zatrzymaj konkretny aktuator)

{
    "Type": "STOP",
    "ManipulatedObject": "ACTUATOR",
    "ObjectIdx": 1
}

3) STOP ALL (zatrzymaj wszystko)

{
    "Type": "STOP",
    "ManipulatedObject": "ALL"
}

4) GET (zapytaj o stan aktuatora)

{
    "Type": "GET",
    "ManipulatedObject": "ACTUATOR",
    "ObjectIdx": 2
}

5) SET (ustaw dozwolony wycinek ruchu steppera)

Rekomendowany format:
{
    "Type": "SET",
    "ManipulatedObject": "ACTUATOR",
    "ObjectIdx": 0,
    "RangeStart": 30,
    "RangeEnd": 204
}

Kompatybilny (stary) format:
{
    "Type": "SET",
    "ManipulatedObject": "ACTUATOR",
    "ObjectIdx": 0,
    "MinAngleLimit": 30,
    "MaxAngleLimit": 204
}

Znaczenie:
- ObjectIdx: tylko 0 lub 1 (tylko steppery)
- RangeStart: początek dozwolonej strefy
- RangeEnd: koniec dozwolonej strefy

Interpretacja zakresu:
- Gdy RangeStart <= RangeEnd: dozwolony zakres [RangeStart, RangeEnd]
- Gdy RangeStart > RangeEnd: zakres przechodzi przez 0 deg
  (np. 225.2 -> 20.0 oznacza: 225.2..360 oraz 0..20.0)

Odpowiedzi / telemetria (sterownik -> PC)

1) INFO (odpowiedź na GET)

{
    "Type": "INFO",
    "ManipulatedObject": "ACTUATOR",
    "ObjectIdx": 2,
    "Values": {
        "CurrentAngle": 90,
        "TargetAngle": 120,
        "Status": "MOVING"
    }
}

Pola Values:
- CurrentAngle
- TargetAngle
- Status: "MOVING" albo "IDLE"

2) HEARTBEAT (cyklicznie co ok. 2 s)

{
    "Type": "HEARTBEAT",
    "ManipulatedObject": "SYSTEM",
    "Values": {
        "Uptime": 123456,
        "EStop": 0,
        "Enc1Angle": 10.5,
        "Enc2Angle": 250.2,
        "SteppersEnabled": 0,
        "Status": "OK"
    }
}

Pola HEARTBEAT:
- Uptime: ms od startu
- EStop: 0/1
- Enc1Angle, Enc2Angle: aktualne kąty enkoderów
- SteppersEnabled: bitmaska (bit0=idx0, bit1=idx1)
- Status: "OK" albo "ESTOP"

Walidacja parsera (ważne):
- Wymagane pola globalnie: Type
- Dla ACTUATOR wymagane: ManipulatedObject="ACTUATOR" oraz ObjectIdx 0..3
- Dla MOVE wymagane: TargetAngle i Speed
- Dla SET wymagane: (RangeStart i RangeEnd) lub (MinAngleLimit i MaxAngleLimit)
- Dla STOP ALL: ManipulatedObject="ALL", bez ObjectIdx
