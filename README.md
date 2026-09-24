# Console UNO

A klasszikus UNO kartyajatek terminalos valtozata C# es .NET 10 kornyezetben, retro ASCII grafikus felulettel, beepitett 8-bites chiptune hangokkal es magyar nyelvu kezelofelulettel.

Keszitette: **Solti Csongor Peter**.

---

## Fo Funkciok

- **ASCII Kartyak**: Szines, tobbsoros karkarteres kartyak (Piros, Kek, Zold, Sarga es Vad szinekben), egyertelmu szimbolumokkal (X = Kimaradsz, <> = Fordito, +2, +4, W = Vad).
- **Interaktiv Iranyitas**: Kartyak tallozasa a Balra / Jobbra nyilakkal ([<-] / [->]) es kijatszasa ENTER-rel, vagy gyorsbillentyukkel (1-9).
- **Lapozas Nagy Kez eseten**: 7 lapnal tobb kartya eseten dinamikus lapozas (<< es >>), igy barmennyi lap konyeden megtekintheto es kijatszhato.
- **Beepitett 8-bites Hangszintetizator**: Retro jatek effektek kartyahuzashoz, lerakashoz, akciohatasokhoz, UNO riasztashoz es gyozelmi fanfarhoz, valamint fomenui hatterzene.
- **Alapertelmezett Magyar Nyelv**: A jatek magyar nyelven indul a diakok szamara (a Beallitasokban angolra is valthato).
- **Perzisztens Beallitas-Cache**: A beallitasok (hang, zene, hangerok, nyelv) automatikusan elmentodnek a `settings.json` fajlba.
- **Jatekmodok**:
  - Egyjatekos mod: 1v1, 1v1v1, 1v1v1v1 intelligens botok ellen.
  - Helyi Tobbjatekos: 2-4 jatekos gepatados kepernyovedovel a lapok elrejtesehez.
- **Hivatalos UNO Szabalyok**:
  - Hurokartya ujrakeverese a dobopaklibol, ha a pakli elfogy.
  - Akciokartyak (+2, +4, Skip, Reverse) hatasa egyszeri, nem ismetlodik huzasnal.
  - 2 jatekos eseten a Fordito (Reverse) Kimaradaskent (Skip) mukodik.
  - UNO bemondas es riasztas, ha valakinek 1 lapja marad.

---

## Iranyitas

### Menuk
- **Szamgombok (1-5)**: Valasztas a menupontok kozul

### Jatek kozben
- **Balra / Jobbra nyilak ([<-] / [->])**: Lap kivalasztasa a kezben
- **ENTER / SZOKOZ**: Kivalasztott lap lerakasa
- **Szamgombok (1-9)**: Az adott pozicioban levo lap azonnali lerakasa
- **D**: Kartya huzasa a paklibol
- **Q**: Kilepes a fomenube

### Beallitasok
- **1**: Nyelv valtasa (Magyar / English)
- **2**: Hanghatasok (BE / KI)
- **3**: Fomenu Zene (BE / KI)
- **4 / 5**: Effektek / Zene hangero modositasa (20% - 100%)
- **6**: Visszateres a fomenube

---

## Futtatas

### 1. Helyi futtatas (.NET 10 SDK)
```bash
# Belepes a projekt mappajaba
cd uno-game/uno-game

# Forditas es inditas
dotnet run
```

### 2. Bongeszoben (GitHub Pages)
Nyisd meg a GitHub Pages weboldalt a bongeszodben, es jatszhatsz azonnal a beagyazott webes terminalon keresztul kliensoldali JavaScript motorral es 8-bites hangokkal!

### 3. Docker Compose (Web localhost:3000 es Terminal)
A jatek konnyeden futtathato Docker Compose segitsegevel Arch Linuxon vagy barmilyen Docker kornyezetben:

```bash
# 1. Webes felulet elinditasa a hatterben (http://localhost:3000):
docker compose up -d web

# 2. Interaktiv terminalos C# jatek futtatasa:
docker compose run --rm game
```

### 4. Szukseges Docker Image-ek (Letoltes / Pull eloadas elott)
Ha az eloadas helyszinen nincs megbizhato internetkapcsolat, toltsd le elore a szukseges image-eket:

```bash
# Web kiszolgalasahoz (localhost:3000):
docker pull nginx:alpine

# Konzol jatek .NET 10 epitesehez es futtatasahoz:
docker pull mcr.microsoft.com/dotnet/sdk:10.0
docker pull mcr.microsoft.com/dotnet/runtime:10.0
```

### 5. Hagyomanyos Docker CLI futtatas
```bash
# Docker kontener felepitese es futtatasa interaktiv terminalban
docker build -t console-uno .
docker run -it --rm console-uno
```

---

## Fejleszto
- **Fejleszto**: Solti Csongor Peter
- **Licenc**: MIT