# UNO Kártyajáték Dokumentáció

## Projekt Áttekintés
Ez egy konzolos UNO kártyajáték C#-ban implementálva, többféle játékmóddal, hangeffektekkel és interaktív menükkel.

## Projekt Szerkezete

### Fő Komponensek
- `Program.cs`: Minden itt van, mivel nem tudtam, hogy lehet-e könyvtárakat használni....

## Főbb Funkciók

### Játékmódok
1. Egyjátékos
   - 1v1
   - 1v1v1
   - 1v1v1v1
2. Helyi Többjátékos (2-4 játékos)
3. Tervezett: Online Többjátékos (Nem implementált)

### Beállítások
- Hang Be/Ki
- Hangeffekt hangerő
- Zene hangerő
- Animációk Be/Ki

## Osztályok Részletesen

### 1. Program Osztály
- Alkalmazás belépési pontja
- Konzol címének beállítása
- Játék inicializálása és indítása

### 2. Kártya Osztály
- Egy UNO kártyát reprezentál
- Tulajdonságok:
  - `Szín`: Kártya színe (Piros, Kék, Zöld, Sárga, Vad)
  - `Érték`: Kártya értéke (0-9, Kihagy, Fordít, +2, +4, Vad)

### 3. Hangkezelő Osztály
- Játék audió kezelése
- Funkciók:
  - Windows konzol sípoló hangok
  - Hangerő szabályozás
  - Zene és hangeffekt kezelés

### 4. Játék Osztály
#### Fő Játéklogika
- Pakli inicializálás
- Kártya húzás
- Játékos kör kezelés
- Nyerési feltételek ellenőrzése

#### Menürendszerek
- Főmenü
- Egyjátékos menü
- Többjátékos menü
- Beállítások menü

## Játékszabályok Implementálása

### Kártya Kijátszási Szabályok
- Kártyák kijátszhatók, ha:
  - Szín megegyezik a jelenlegi kártyáéval
  - Érték megegyezik a jelenlegi kártyáéval
  - Vad kártya kijátszása

### Speciális Kártyák
- Kihagy: Következő játékos kimarad
- Fordít: Megváltoztatja a játék irányát
- +2: Következő játékos húz 2 kártyát
- +4: Következő játékos húz 4 kártyát és színt választ
- Vad: Játékos választ új színt

## Technikai Részletek

### Véletlenszerűség
- Pakli keverése Fisher-Yates algoritmussal
- `Random` osztály játékmechanikákhoz

### Hangrendszer
- `Console.Beep()` hangeffektekhez
- Csak Windows kompatibilitás
- Hangerő és némítás beállítások

### Animáció
- Szöveg karakterenként jelenik meg
- Kártya húzásnak vizuális és hanghatása
- Kikapcsolható a beállításokban

## Lehetséges Fejlesztések
- Hálózati többjátékos mód
- Összetettebb mesterséges intelligencia
- Grafikus felhasználói felület
- Állandó pontrendszer

## Fejlesztési Környezet
- Nyelv: C# (.NET)
- Platform: Windows Konzol Alkalmazás

## Licenc
[Adja hozzá a kívánt licencet]

## Közreműködők
[Közreműködők vagy saját neve]

## Verzió
1.0.0

## Ismert Problémák
- Hang csak Windows-on működik
- Korlátozott mesterséges intelligencia
- Csak konzol felület