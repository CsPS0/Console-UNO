# Console UNO – Projekt Dokumentáció

## Projekt Áttekintés
Ez egy konzolos UNO kártyajáték C# (.NET 10) nyelven implementálva. A projekt a diákoknak szóló interaktív bemutatókhoz készült, látványos ASCII felülettel és beépített 8-bites hangokkal.

---

## Projekt Szerkezete és Architektúrája

A forráskód tiszta, moduláris szerkezetre épül:
- `Program.cs`: Belépési pont, UTF-8 karakterkódolás és konzolcím inicializálása.
- `Card.cs`: A kartya modell, ASCII-doboz generator (`GetAsciiLines`), szimbolumok (`X`, `<>`, `+2`, `+4`, `W`), konzolszinek es ketnyelvu (magyar/angol) lokalizacio.
- `Game.cs`: A teljes játékmotor, állapotkezelés, Fisher-Yates keverő algoritmus, körök és szabályok végrehajtása, tábla kirajzolása, interaktív kurzoros kártyaválasztó és menürendszer.
- `GameSettings.cs`: Perzisztens beállítások modellje (`settings.json`).
- `SoundManager.cs`: Beépített, aszinkron 8-bites retro chiptune hangszintetizátor (`Console.Beep` és háttérszálak segítségével). Külső fájlok és könyvtárfüggőségek nélkül működik.

---

## Főbb Funkciók

### 1. Játékmódok
- **Egyjátékos mód (vs AI)**:
  - 1v1 (Te vs 1 AI)
  - 1v1v1 (Te vs 2 AI)
  - 1v1v1v1 (Te vs 3 AI)
- **Helyi Többjátékos (2-4 játékos)**:
  - Körönkénti képernyővédő („Add át a gépet...”), így a játékosok nem látják egymás lapjait.

### 2. Grafikus Felület és Irányítás
- **Vizuális kártyamegjelenítés**: Többsoros színes ASCII kártyadobozok piros, kék, zöld, sárga és lila vad színekben.
- **Interaktív kurzor**: `←` és `→` nyilakkal tallózható kéz, `ENTER` vagy `SZÓKÖZ` a lerakáshoz.
- **Lapozás**: 7 lap feletti kéznél dinamikus lapozás (`«` és `»`), minden lap elérhető és kijátszható marad.
- **Közvetlen gombok**: `1`-`9` számgombok a lap azonnali megjátszásához, `D` a húzáshoz, `Q` a kilépéshez.

### 3. Audió Rendszer
- Külső `.wav` fájlokat nem igénylő, beépített chiptune szintetizátor.
- Külön hangok:
  - Laplerakás és laphúzás
  - Speciális akciókártyák és színválasztás
  - UNO riasztás
  - Győzelmi fanfár
  - Főmenü háttérzene (egyedi 8-bites téma, be- és kikapcsolható)

### 4. Hivatalos UNO Szabályok
- **Dobópakli újrakeverése**: Ha a húzópakli elfogy, a dobópakli automatikusan újrakeveredik.
- **Egyszeri akcióhatás**: Az akciókártyák (+2, +4, Skip, Reverse) hatása csak a kijátszás körében lép életbe, laphúzáskor nem ismétlődik.
- **2 fős Reverse**: 2 játékos esetén a Fordító kártya szabályosan Kimaradásként (Skip) funkcionál.
- **UNO bemondás**: 1 lapra csökkenéskor látványos vizuális és hangos figyelmeztetés.

---

## Beállítások és Mentés
- Kétnyelvűség: **Magyar** (alapértelmezett) és **English**
- Hangeffektek: Be / Ki
- Főmenü Zene: Be / Ki
- Effektek és Zene hangerő: 20% - 100%
- Automatikus mentés: `settings.json` (git által ignorálva)

---

## Futtatasi Lehetosegek es Docker

### 1. Helyi futtatas (.NET 10 SDK)
```bash
cd uno-game/uno-game
dotnet run
```

### 2. Bongeszoben (GitHub Pages)
A jatek a GitHub Pages oldalon kozvetlenul jatszhato barmilyen bongeszobol (kliensoldali JS motor, Web Audio szintetizator, billentyuzet- es gombvezerles).

### 3. Docker Compose (Web localhost:3000 es Terminal)
```bash
# Weboldal elerese bongeszoben (http://localhost:3000):
docker compose up -d web

# Interaktiv terminalos jatek futtatasa:
docker compose run --rm game
```

### 4. Letoltendo Docker Image-ek (Eloadas elotti pull)
```bash
docker pull nginx:alpine
docker pull mcr.microsoft.com/dotnet/sdk:10.0
docker pull mcr.microsoft.com/dotnet/runtime:10.0
```

---

## Fejlesztői és Iskolai Információ
- **Iskola**: BMSZC Neumann János Informatikai Technikum
- **Készítő**: Solti Csongor Péter
- **Technológia**: C# / .NET 10, HTML5 / JS (Web Audio)
- **Platform**: Windows Konzol, Linux / Docker, Web (GitHub Pages)