4. Komunikacja JSON i parser komend
Format komend
Komenda MOVE (ruch do kąta docelowego)
{
    "Type": "MOVE",
    "ManipulatedObject": "ACTUATOR",
    "ObjectIdx": 0,
    "TargetAngle": 100,
    "Speed": 200
}
Co oznacza:

Type="MOVE" - chcemy przesunąć
ManipulatedObject="ACTUATOR" - sterujemy aktorem
ObjectIdx=0 - Stepper 1 (TB6600)
TargetAngle=100 - docelowa pozycja = 100 (kroków dla steppera, stopni dla servo)
Speed=200 - szybkość (0-255 lub bardziej, zależy od driver'a)
Komenda STOP (zatrzymaj)
{
    "Type": "STOP",
    "ManipulatedObject": "ACTUATOR",
    "ObjectIdx": 0
}
lub zatrzymaj wszystko:

{
    "Type": "STOP",
    "ManipulatedObject": "ALL"
}
Komenda GET (zapytaj stan)
{
    "Type": "GET",
    "ManipulatedObject": "ACTUATOR",
    "ObjectIdx": 2
}
Odpowiedź (wysyła sterownik):

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
