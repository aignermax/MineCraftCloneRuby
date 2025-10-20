# Minecraft Clone - Godot 4.5

using Godot;
using MinecraftClone;
using System.Reflection;
using System.Runtime.InteropServices;
using static Godot.OpenXRInterface;
using static System.Reflection.Metadata.BlobBuilder;

Ein 3D Minecraft-Klon entwickelt mit Godot 4.5 und C#.

## Features

### Implementiert:
- **Voxel-basierte Welt**: Prozedural generierte Chunks mit verschiedenen Blocktypen
- **First-Person Controller**: WASD - Bewegung, Maussteuerung, Springen
- **Block-System**: Wiese, Holz, Metall, Eisen, Wasser, Lava, Feuer, Stein
- **Tiere**: Panda, Katze, Hund, Papagei, Kuh, Fuchs, Fische, Creeper
- **Werkzeuge**: Hand, Schwert, Spitzhacke, Bogen
- **Inventory-System**: 36 Slots + Hotbar mit 9 Slots
- **Kisten**: Interaktive Lagerungsmöglichkeiten
- **Rüstungssystem**: Leder - und Eisenrüstung
- **Hauptmenü * *: Start, Einstellungen, Beenden
- **HUD**: Gesundheitsanzeige, Werkzeuganzeige, Hotbar, Fadenkreuz

## Setup-Anleitung

### 1. Projektstruktur erstellen:
```
MinecraftClone/
├── Scripts/
│   ├── GameManager.cs
│   ├── VoxelWorld.cs
│   ├── Chunk.cs
│   ├── FirstPersonController.cs
│   ├── Animal.cs
│   ├── Inventory.cs
│   ├── Chest.cs
│   └── MainMenu.cs
├── Scenes/
│   ├── Player.tscn
│   ├── World.tscn
│   ├── Chunk.tscn
│   └── UI/
│       └── HUD.tscn
└── Game.tscn
```

### 2. Input Map konfigurieren:
Füge die Einträge aus `InputMap.gdcfg` in deine `project.godot` Datei ein:
-**WASD * *: Bewegung
- **Leertaste * *: Springen
- **Maus * *: Umsehen
- **E * *: Interagieren
- **Linksklick * *: Primäre Aktion(abbauen)
-**Rechtsklick * *: Sekundäre Aktion(platzieren)
-**1 - 9 * *: Hotbar - Auswahl
- **ESC * *: Maus ein/ausblenden

### 3. C# Support aktivieren:
1. Öffne Projekt-Einstellungen
2. Gehe zu "Dotnet" → "Project"
3. Aktiviere "Use .NET"
4. Stelle sicher, dass .NET SDK installiert ist

### 4. Hauptmenü als Startszene:
1. Erstelle eine neue Szene `MainMenu.tscn`
2. Füge ein Control-Node hinzu
3. Weise das `MainMenu.cs` Script zu
4. Setze als Hauptszene in den Projekteinstellungen

## Steuerung

- **WASD**: Bewegen
- **Maus * *: Umsehen
- **Leertaste * *: Springen
- **E * *: Mit Kisten interagieren
- **Linke Maustaste**: Block abbauen(mit Spitzhacke) / Angreifen(mit Schwert)
- **Rechte Maustaste * *: Block platzieren
- **1-4**: Werkzeug wechseln
- **1-9**: Hotbar - Slot auswählen
- **Mausrad * *: Durch Hotbar scrollen
- **ESC**: Mauszeiger ein-/ausblenden

## Erweiterungsmöglichkeiten

### Nächste Features:
1. **Crafting-System**: Rezepte für neue Items
2. **Tag/Nacht-Zyklus**: Dynamische Beleuchtung
3. **Mehr Blocktypen**: Erde, Sand, Glas, etc.
4. **Bessere Terrain-Generierung**: Biome, Höhlen, Strukturen
5. **Multiplayer**: Netzwerk - Support
6. * *Sound - Effekte * *: Schritte, Block-Sounds, Tier-Geräusche
7. **Partikel-Effekte**: Block - Bruch, Explosionen
8. **Speicher-System**: Welt speichern/laden
9. **Mehr Tiere und Gegner**: Skelette, Zombies, etc.
10. **Achievements**: Fortschrittssystem

### Performance-Optimierungen:
- Chunk - LOD(Level of Detail)
- Frustum Culling
- Greedy Meshing für Chunks
- Objekt-Pooling für Entities
- Texture-Atlas für Blöcke

## Bekannte Limitierungen

- Chunks werden momentan nicht gespeichert
- Keine Kollisionserkennung zwischen Tieren
- Wasser/Lava haben noch keine Fließ-Physik
- Beleuchtung ist noch sehr einfach
- Keine Sounds implementiert

## Tipps für die Entwicklung

1. **Performance**: Halte die Render-Distanz niedrig für bessere FPS
2. **Debugging**: Nutze Godots Profiler für Performance-Analyse
3. **Erweiterungen**: Das System ist modular aufgebaut - neue Features lassen sich leicht hinzufügen
4. **Assets**: Erstelle Texturen im 16x16 oder 32x32 Format für authentischen Minecraft-Look

Viel Spaß beim Entwickeln deines Minecraft-Klons!