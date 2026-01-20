# Video Puller


# Projektbeschreibung:
Das Projekt ist ein Spiel fuer das Peppers Ghost System, mit dem man Kurzvideos anschauen kann.
Man muss jedoch bevor man ein Video anschauen "darf", eine Kurbel drehen und dann bei jedem Level eine immer kuriosere Aufgabe vor einer Kamera erledigen.
Das System sollte an einem mehr oder weniger oeffentlichen Ort stehen, so dass Menschen, die nichts vom Projekt wissen, es bedienen.
Das Projekt dient also als soziales Experiment, in dem man herausfindet, wie weit Menschen gehen, nur um sich "sinnlose" Videos anzusehen.


# Setup:
Zuerst muss, um alle Funktionen korrekt verwenden zu koennen, der StreamingAssets Ordner von folgendem Link heruntergeladen werden und in das Projekt unter \Video_Puller\Assets\ eingefuegt werden.

Download Link:

https://drive.google.com/drive/folders/1B5JOegBrK-KJxL1OKNKihTMmV6ttmb-v?usp=sharing

Der gesamte Ordner mit Inhalten wird dann hier eingefuegt:

<img width="657" height="540" alt="StreamingAssets" src="https://github.com/user-attachments/assets/82128762-1736-4040-a96e-d99fd9367034" />

Der Ordner und seine Inhalte muessen gleich benannt bleiben.

# Nutzungsanleitung
In der Main Scene (9:16 Hochformat) oeffBittenet man die Kiste mit der Maus mit dem Slider oben am Screen.
Um zur nächsten Kiste und somit zum nächsten Video zu gelangen -> Slider Wieder nach links um die Kiste zu schliessen und dann wieder nach rechts un die naechste zu oeffnen.

Screenshot:

<img width="1919" height="1007" alt="MainGame" src="https://github.com/user-attachments/assets/4789b165-9b61-4b3c-a6ec-38ecf5541a4f" />




In der DetectionTester Scene (9:16 Hochformat) muss man nach dem starten der Szene einem Moment warten, bis die beiden Programme (objectDetection_build_noWindow.exe und poseDetection_build_noWindow.exe) gestartet sind.
Das dauert in der Regel aber nicht laenger als 30 Sekunden. Diese Programme haben jedoch absichtlich kein Fenster, man kann also nur im Task manager ueberpruefen, ob sie laufen.

Screenshot:

<img width="781" height="236" alt="grafik" src="https://github.com/user-attachments/assets/42dfdc9f-dee8-4cb3-8031-fc3bfcb88e57" />

Danach kann man eine der beiden Detections ausprobieren:
Object Detection gibt jede Sekunde aus, welche Objekte bei der standard webcam gerade im Frame sind und schätzt die Emotionen von Personen im Frame anhand ihrer Gesichtsausdruecke.
Action Detection erkennt, wie wahrscheinlich eine Person im Frame gerade springt oder winkt anhand der Position ihrer Koerperteile. Je hoeher die Zahl, desto wahrscheinlicher.
Jedoch MUSS man beim Wechseln zwischen den beiden Programmen, dazwischen immer den roten "Close Connection" Knopf druecken, um zu vermeiden, dass beide Programme gleichzeitig versuchen auf die gleiche Webcam zuzugreifen. Das kann unvorhersehbare Probleme auslösen.

Die Kurbel Scene bedient man vorerst mit den Pfeiltasaten:
Links <-, um die Kurbel rueckwaerts zu drehen und Rechts ->, um die Kurbel vorwaerts zu drehen.
Hierbei ist ein Tastendruck als ein Zahn der Kurbel anzusehen, also eine zwoelftel Umdrehung.
Es werden drei Statistiken erfasst: Die Drehgeschwindigkeit in RPM, die Anzahl der erfassten einzelnen Steps, und die Gradanzahl, also der Winkel den die Kurbel hat.

Screenshot:

<img width="164" height="140" alt="KurbelStats" src="https://github.com/user-attachments/assets/3fe5eb41-46cd-4349-ad36-5e1b725d2b9f" />



# Gameplay:
https://github.com/user-attachments/assets/1df5f0b9-8cbd-49e4-af1e-942f758fa822


# Development Platform:
Unity Version: 6.2 (6000.2.7f2)
Programmiersprachen: C# (in Unity), Python (Detection Software)
Betriebssystem: Windows 11 Pro

# Destination Platform:
Windows Standalone (Intended use: Peppers Ghost)
