(function() {
    let audioCtx = null;

    function getAudioContext() {
        if (!audioCtx) {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            if (AudioContext) {
                audioCtx = new AudioContext();
            }
        }
        if (audioCtx && audioCtx.state === 'suspended') {
            audioCtx.resume();
        }
        return audioCtx;
    }

    let soundEnabled = true;
    let isHungarian = true;

    function playTone(freq, duration, type = 'square', gainVal = 0.08) {
        if (!soundEnabled) return;
        try {
            const ctx = getAudioContext();
            if (!ctx) return;
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();

            osc.type = type;
            osc.frequency.setValueAtTime(freq, ctx.currentTime);

            gain.gain.setValueAtTime(gainVal, ctx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + duration / 1000);

            osc.connect(gain);
            gain.connect(ctx.destination);

            osc.start();
            osc.stop(ctx.currentTime + duration / 1000);
        } catch (e) { }
    }

    const Sound = {
        beep: () => playTone(880, 50),
        playCard: () => {
            playTone(700, 40);
            setTimeout(() => playTone(1100, 60), 45);
        },
        drawCard: () => {
            playTone(450, 40);
            setTimeout(() => playTone(650, 50), 45);
        },
        special: () => {
            playTone(523, 60);
            setTimeout(() => playTone(659, 60), 60);
            setTimeout(() => playTone(784, 80), 120);
            setTimeout(() => playTone(1046, 110), 190);
        },
        uno: () => {
            playTone(880, 100);
            setTimeout(() => playTone(1320, 120), 110);
            setTimeout(() => playTone(880, 100), 250);
            setTimeout(() => playTone(1320, 120), 360);
        },
        invalid: () => {
            playTone(220, 90);
            setTimeout(() => playTone(180, 120), 95);
        },
        victory: () => {
            const notes = [523, 523, 523, 659, 784, 1046];
            const delays = [0, 110, 220, 340, 500, 700];
            notes.forEach((n, i) => {
                setTimeout(() => playTone(n, i === 5 ? 400 : 100), delays[i]);
            });
        }
    };

    class Card {
        constructor(color, value) {
            this.color = color;
            this.value = value;
        }

        getSymbol() {
            switch (this.value) {
                case 'Skip': return 'X';
                case 'Reverse': return '<>';
                case '+2': return '+2';
                case '+4': return '+4';
                case 'Wild': return 'W';
                default: return this.value;
            }
        }

        getColorClass() {
            switch (this.color) {
                case 'Red': return 'c-red';
                case 'Blue': return 'c-blue';
                case 'Green': return 'c-green';
                case 'Yellow': return 'c-yellow';
                case 'Wild': return 'c-wild';
                default: return 'c-white';
            }
        }

        getColorAbbr() {
            switch (this.color) {
                case 'Red': return 'RED';
                case 'Blue': return 'BLU';
                case 'Green': return 'GRN';
                case 'Yellow': return 'YEL';
                case 'Wild': return 'WLD';
                default: return '---';
            }
        }

        getLocalizedName() {
            const colMap = isHungarian
                ? { Red: 'Piros', Blue: 'Kek', Green: 'Zold', Yellow: 'Sarga', Wild: 'Szinvalaszto' }
                : { Red: 'Red', Blue: 'Blue', Green: 'Green', Yellow: 'Yellow', Wild: 'Wild' };
            const valMap = isHungarian
                ? { Skip: 'Kimaradsz', Reverse: 'Fordito', '+2': '+2', '+4': '+4 Vad', Wild: 'Vad' }
                : { Skip: 'Skip', Reverse: 'Reverse', '+2': '+2', '+4': '+4 Wild', Wild: 'Wild' };
            const c = colMap[this.color] || this.color;
            const v = valMap[this.value] || this.value;
            return this.color === 'Wild' && this.value !== '+4'
                ? (isHungarian ? 'Vad (Szinvalaszto)' : 'Wild (Color Choice)')
                : `${c} ${v}`;
        }

        getAsciiLines(selected = false) {
            let sym = this.getSymbol();
            let topSym = sym.padEnd(3).substring(0, 3);
            let botSym = sym.padStart(3).substring(sym.length > 3 ? sym.length - 3 : 0, 3);
            let mid = this.getColorAbbr().padEnd(3).substring(0, 3);

            if (selected) {
                return [
                    '╔═════╗',
                    `║${topSym}  ║`,
                    `║ ${mid} ║`,
                    `║  ${botSym}║`,
                    '╚═════╝'
                ];
            }
            return [
                '┌─────┐',
                `│${topSym}  │`,
                `│ ${mid} │`,
                `│  ${botSym}│`,
                '└─────┘'
            ];
        }
    }

    let currentScreen = 'MAIN_MENU';
    let deck = [];
    let discardPile = [];
    let playerHand = [];
    let aiHands = [];
    let numBots = 1;
    let currentCard = null;
    let selectedIndex = 0;
    let lastAction = '';
    let lastActionColor = 'c-cyan';
    let isGameOver = false;
    let winner = null;
    let pickingWildColor = false;
    let isAiTurn = false;

    function setScreen(screen) {
        currentScreen = screen;
        Sound.beep();
        render();
    }

    function initDeck() {
        const d = [];
        const colors = ['Red', 'Blue', 'Green', 'Yellow'];
        const values = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'Skip', 'Reverse', '+2'];

        colors.forEach(c => {
            values.forEach(v => {
                d.push(new Card(c, v));
                if (v !== '0') d.push(new Card(c, v));
            });
        });

        for (let i = 0; i < 4; i++) {
            d.push(new Card('Wild', 'Wild'));
            d.push(new Card('Wild', '+4'));
        }

        shuffle(d);
        return d;
    }

    function shuffle(arr) {
        for (let i = arr.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [arr[i], arr[j]] = [arr[j], arr[i]];
        }
    }

    function drawCard(hand) {
        if (deck.length === 0) {
            if (discardPile.length > 0) {
                deck = [...discardPile];
                discardPile = [];
                shuffle(deck);
                lastAction = isHungarian ? 'A pakli elfogyott! Dobopakli ujrakeverve.' : 'Deck reshuffled!';
                lastActionColor = 'c-wild';
            } else {
                return null;
            }
        }
        const card = deck.pop();
        if (card) hand.push(card);
        return card;
    }

    function startMatch(bots = 1) {
        numBots = bots;
        deck = initDeck();
        discardPile = [];
        playerHand = [];
        aiHands = Array.from({ length: numBots }, () => []);

        for (let i = 0; i < 7; i++) {
            drawCard(playerHand);
            for (let b = 0; b < numBots; b++) {
                drawCard(aiHands[b]);
            }
        }

        do {
            currentCard = deck.pop();
        } while (currentCard.color === 'Wild');

        discardPile.push(currentCard);
        selectedIndex = 0;
        isGameOver = false;
        winner = null;
        pickingWildColor = false;
        isAiTurn = false;
        lastAction = isHungarian ? 'Uj jatek indult! Te kezdesz.' : 'Game started! Your turn.';
        lastActionColor = 'c-cyan';

        setScreen('GAME');
    }

    function canPlay(card, topCard) {
        if (!topCard) return true;
        if (card.color === 'Wild') return true;
        if (card.color === topCard.color) return true;
        if (card.value === topCard.value) return true;
        return false;
    }

    function playPlayerCard(index) {
        if (currentScreen !== 'GAME' || isGameOver || isAiTurn || pickingWildColor) return;
        if (index < 0 || index >= playerHand.length) return;

        const card = playerHand[index];
        if (!canPlay(card, currentCard)) {
            Sound.invalid();
            lastAction = isHungarian ? 'Ervenytelen lepes! Válassz azonos szinu vagy szamukartyat!' : 'Invalid move! Match color or value.';
            lastActionColor = 'c-red';
            render();
            return;
        }

        if (card.color === 'Wild') {
            pickingWildColor = true;
            playerHand.splice(index, 1);
            if (currentCard) discardPile.push(currentCard);
            currentCard = card;
            Sound.special();
            lastAction = isHungarian ? 'Vad kartya! Valassz szint!' : 'Wild card! Choose a color!';
            lastActionColor = 'c-wild';
            render();
            return;
        }

        playerHand.splice(index, 1);
        if (currentCard) discardPile.push(currentCard);
        currentCard = card;
        Sound.playCard();
        lastAction = `${isHungarian ? 'Leraktad' : 'Played'}: ${card.getLocalizedName()}`;
        lastActionColor = card.getColorClass();

        if (playerHand.length === 1) {
            Sound.uno();
            lastAction += ' *** UNO! ***';
        }

        if (playerHand.length === 0) {
            winner = isHungarian ? 'Te (Jatekos 1)' : 'You (Player 1)';
            isGameOver = true;
            Sound.victory();
            setScreen('VICTORY');
            return;
        }

        if (selectedIndex >= playerHand.length) {
            selectedIndex = Math.max(0, playerHand.length - 1);
        }

        let aiSkips = false;
        if (card.value === 'Skip' || card.value === 'Reverse') {
            Sound.special();
            aiSkips = true;
            lastAction += isHungarian ? ' -> AI kimarad a korbol!' : ' -> AI skips turn!';
        } else if (card.value === '+2') {
            Sound.special();
            for (let b = 0; b < numBots; b++) {
                drawCard(aiHands[b]);
                drawCard(aiHands[b]);
            }
            aiSkips = true;
            lastAction += isHungarian ? ' -> AI huzz 2 lapot es kimarad!' : ' -> AI draws 2 cards & skips!';
        }

        render();

        if (!aiSkips) {
            isAiTurn = true;
            setTimeout(handleAiTurn, 1000);
        }
    }

    function chooseColor(color) {
        if (!pickingWildColor || !currentCard) return;
        currentCard.color = color;
        pickingWildColor = false;
        Sound.beep();

        const colName = isHungarian
            ? (color === 'Red' ? 'Piros' : color === 'Blue' ? 'Kek' : color === 'Green' ? 'Zold' : 'Sarga')
            : color;
        lastAction = `${isHungarian ? 'Uj aktiv szin' : 'New active color'}: ${colName}`;
        lastActionColor = currentCard.getColorClass();

        if (currentCard.value === '+4') {
            for (let b = 0; b < numBots; b++) {
                for (let k = 0; k < 4; k++) drawCard(aiHands[b]);
            }
            lastAction += isHungarian ? ' -> AI huzz 4 lapot es kimarad!' : ' -> AI draws 4 & skips!';
            render();
        } else {
            isAiTurn = true;
            render();
            setTimeout(handleAiTurn, 1000);
        }
    }

    function handleAiTurn() {
        if (currentScreen !== 'GAME' || isGameOver) return;

        for (let b = 0; b < numBots; b++) {
            const aiHand = aiHands[b];
            let playIdx = -1;

            for (let i = 0; i < aiHand.length; i++) {
                if (canPlay(aiHand[i], currentCard)) {
                    playIdx = i;
                    break;
                }
            }

            if (playIdx !== -1) {
                const played = aiHand.splice(playIdx, 1)[0];
                if (currentCard) discardPile.push(currentCard);
                currentCard = played;

                if (played.color === 'Wild') {
                    const cols = ['Red', 'Blue', 'Green', 'Yellow'];
                    played.color = cols[Math.floor(Math.random() * cols.length)];
                }

                Sound.playCard();
                lastAction = `AI ${b + 1} ${isHungarian ? 'kijatszotta' : 'played'}: ${played.getLocalizedName()}`;
                lastActionColor = played.getColorClass();

                if (played.value === 'Skip' || played.value === 'Reverse') {
                    Sound.special();
                    lastAction += isHungarian ? ' -> Kimaradsz a korbol!' : ' -> You miss your turn!';
                } else if (played.value === '+2') {
                    Sound.special();
                    drawCard(playerHand);
                    drawCard(playerHand);
                    lastAction += isHungarian ? ' -> Huzz 2 lapot es kimaradsz!' : ' -> Draw 2 cards & miss turn!';
                } else if (played.value === '+4') {
                    Sound.special();
                    for (let k = 0; k < 4; k++) drawCard(playerHand);
                    lastAction += isHungarian ? ' -> Huzz 4 lapot es kimaradsz!' : ' -> Draw 4 cards & miss turn!';
                }
            } else {
                const drawn = drawCard(aiHand);
                Sound.drawCard();
                if (drawn && canPlay(drawn, currentCard)) {
                    aiHand.pop();
                    if (currentCard) discardPile.push(currentCard);
                    currentCard = drawn;
                    if (drawn.color === 'Wild') {
                        drawn.color = ['Red', 'Blue', 'Green', 'Yellow'][Math.floor(Math.random() * 4)];
                    }
                    Sound.playCard();
                    lastAction = `AI ${b + 1} ${isHungarian ? 'huzott es lerakta' : 'drew & played'}: ${drawn.getLocalizedName()}`;
                    lastActionColor = drawn.getColorClass();
                } else {
                    lastAction = `AI ${b + 1} ${isHungarian ? 'huzott egy lapot.' : 'drew a card.'}`;
                    lastActionColor = 'c-cyan';
                }
            }

            if (aiHand.length === 1) {
                Sound.uno();
                lastAction += ` *** AI ${b + 1}: UNO! ***`;
            }

            if (aiHand.length === 0) {
                winner = `AI (Bot ${b + 1})`;
                isGameOver = true;
                Sound.invalid();
                setScreen('VICTORY');
                return;
            }
        }

        isAiTurn = false;
        render();
    }

    function handlePlayerDraw() {
        if (currentScreen !== 'GAME' || isGameOver || isAiTurn || pickingWildColor) return;
        const drawn = drawCard(playerHand);
        if (drawn) {
            Sound.drawCard();
            selectedIndex = playerHand.length - 1;
            lastAction = `${isHungarian ? 'Huztal egy lapot' : 'Drew card'}: ${drawn.getLocalizedName()}`;
            lastActionColor = 'c-cyan';

            isAiTurn = true;
            render();
            setTimeout(handleAiTurn, 1000);
        } else {
            Sound.invalid();
            lastAction = isHungarian ? 'Nincs tobb lap a pakliban!' : 'No more cards in draw deck!';
            lastActionColor = 'c-red';
            render();
        }
    }

    function render() {
        const screen = document.getElementById('terminal-screen');
        if (!screen) return;

        let html = '';

        if (currentScreen === 'MAIN_MENU') {
            html += '<span class="c-red">[ C O N S O L E   U N O ]</span>\n';
            html += '<span class="c-darkgray">Fejleszto: Solti Csongor Peter</span>\n';
            html += '<span class="c-darkgray">--------------------------------------------------------------------------</span>\n\n';

            html += '┌─────────────────────────────────────────────────────┐\n';
            html += '│  <span class="tui-menu-link" onclick="window.UNO.selectMenu(\'1\')">1. Egyjatekos Mod (vs AI)</span>                          │\n';
            html += '│  <span class="tui-menu-link" onclick="window.UNO.selectMenu(\'2\')">2. Helyi Tobbjatekos (2-4 jatekos)</span>                 │\n';
            html += '│  <span class="tui-menu-link" onclick="window.UNO.selectMenu(\'3\')">3. Beallitasok (Settings)</span>                          │\n';
            html += '│  <span class="tui-menu-link" onclick="window.UNO.selectMenu(\'4\')">4. Jatekszabalyok & Utmutato</span>                       │\n';
            html += '│  <span class="tui-menu-link" onclick="window.UNO.selectMenu(\'5\')">5. Kilepes / Reset</span>                                 │\n';
            html += '└─────────────────────────────────────────────────────┘\n\n';

            html += `  <span class="c-yellow font-bold">${isHungarian ? 'Valassz a szamgombokkal (1-5) vagy kattints a fenti opciokra!' : 'Select using keys (1-5) or click options above!'}</span>\n`;
            if (lastAction) {
                html += `\n  <span class="${lastActionColor}">>> ${lastAction}</span>\n`;
            }
        }
        else if (currentScreen === 'SINGLE_PLAYER_MENU') {
            html += '<span class="c-yellow">=== EGYJATEKOS MOD (VS AI) ===</span>\n';
            html += '<span class="c-darkgray">--------------------------------------------------------------------------</span>\n\n';

            html += '┌───────────────────────────────────┐\n';
            html += '│  <span class="tui-menu-link" onclick="window.UNO.selectMenu(\'1\')">1. 1v1 (Te vs 1 AI)</span>             │\n';
            html += '│  <span class="tui-menu-link" onclick="window.UNO.selectMenu(\'2\')">2. 1v1v1 (Te vs 2 AI)</span>           │\n';
            html += '│  <span class="tui-menu-link" onclick="window.UNO.selectMenu(\'3\')">3. 1v1v1v1 (Te vs 3 AI)</span>         │\n';
            html += '│  <span class="tui-menu-link" onclick="window.UNO.selectMenu(\'4\')">4. Vissza a Fomenube</span>            │\n';
            html += '└───────────────────────────────────┘\n\n';

            html += `  <span class="c-cyan">${isHungarian ? 'Nyomj 1-4 gombot a valasztashoz.' : 'Press keys 1-4 to select.'}</span>\n`;
        }
        else if (currentScreen === 'MULTIPLAYER_MENU') {
            html += '<span class="c-yellow">=== HELYI TOBBJATEKOS (2-4 JATEKOS) ===</span>\n';
            html += '<span class="c-darkgray">--------------------------------------------------------------------------</span>\n\n';

            html += '┌───────────────────────────────────┐\n';
            html += '│  <span class="tui-menu-link" onclick="window.UNO.selectMenu(\'1\')">1. 2 Jatekos Helyi Jatek</span>        │\n';
            html += '│  <span class="tui-menu-link" onclick="window.UNO.selectMenu(\'2\')">2. 3 Jatekos Helyi Jatek</span>        │\n';
            html += '│  <span class="tui-menu-link" onclick="window.UNO.selectMenu(\'3\')">3. 4 Jatekos Helyi Jatek</span>        │\n';
            html += '│  <span class="tui-menu-link" onclick="window.UNO.selectMenu(\'4\')">4. Vissza a Fomenube</span>            │\n';
            html += '└───────────────────────────────────┘\n\n';

            html += `  <span class="c-cyan">${isHungarian ? 'Nyomj 1-4 gombot a jatek inditasahoz.' : 'Press keys 1-4 to select.'}</span>\n`;
        }
        else if (currentScreen === 'SETTINGS') {
            html += '<span class="c-yellow">=== BEALLITASOK (SETTINGS) ===</span>\n';
            html += '<span class="c-darkgray">--------------------------------------------------------------------------</span>\n\n';

            const langStr = isHungarian ? 'Magyar' : 'English';
            const soundStr = soundEnabled ? (isHungarian ? 'BE' : 'ON') : (isHungarian ? 'KI' : 'OFF');

            html += '┌───────────────────────────────────────────────┐\n';
            html += `│  <span class="tui-menu-link" onclick="window.UNO.selectMenu('1')">1. Nyelv (Language): ${langStr.padEnd(23)}</span> │\n`;
            html += `│  <span class="tui-menu-link" onclick="window.UNO.selectMenu('2')">2. Hanghatasok (Sound): ${soundStr.padEnd(21)}</span> │\n`;
            html += '│  <span class="tui-menu-link" onclick="window.UNO.selectMenu(\'3\')">3. Vissza a Fomenube</span>                         │\n';
            html += '└───────────────────────────────────────────────┘\n\n';
        }
        else if (currentScreen === 'TUTORIAL') {
            html += '<span class="c-yellow">=== JATEKSZABALYOK & UTMUTATO ===</span>\n';
            html += '<span class="c-darkgray">--------------------------------------------------------------------------</span>\n';
            html += '  <span class="c-white">Alapveto Szabalyok:</span>\n';
            html += '  - Egyezes: Rakj le azonos szinu vagy azonos erteku kartyat!\n';
            html += '  - Laphuzas: Ha nem tudsz rakni, nyomj [D]-t egy lap huzasahoz!\n';
            html += '  - UNO Bemondas: Ha 1 lapod marad, a rendszer automatikusan jelzi!\n\n';
            html += '  <span class="c-white">Specialis Akciokartyak:</span>\n';
            html += '  - [SKIP]: A kovetkezo jatekos kimarad a korbol\n';
            html += '  - [REVERSE]: Megforditja a jatek haladasi iranyat\n';
            html += '  - [+2]: A kovetkezo jatekos 2 lapot huz es kimarad\n';
            html += '  - [WILD]: Barmikor lerakhato, uj szint valaszthatsz\n';
            html += '  - [+4 WILD]: Uj szint valaszthatsz + a kovetkezo jatekos 4 lapot huz\n\n';
            html += '  <button onclick="window.UNO.setScreen(\'MAIN_MENU\')" class="tui-btn tui-btn-primary">VISSZA A FOMENUBE (ENTER / 1)</button>\n';
        }
        else if (currentScreen === 'GAME') {
            html += '<span class="c-red">[ C O N S O L E   U N O ]</span>\n';
            html += '<span class="c-darkgray">--------------------------------------------------------------------------</span>\n';

            html += `  <span class="c-white">KOVETKEZO:</span> <span class="c-yellow">${isAiTurn ? 'AI (Bot)      ' : 'Te (Jatekos 1)'}</span>`;
            html += ` | <span class="c-white">AI LAPJAI:</span> <span class="c-cyan">${aiHands.map(h => h.length + 'db').join(', ')}</span>`;
            html += ` | <span class="c-white">PAKLIK:</span> <span class="c-green">${deck.length} lap</span>\n`;
            html += '<span class="c-darkgray">--------------------------------------------------------------------------</span>\n\n';

            const drawBox = [
                '┌─────┐',
                '│░░░░░│',
                '│ UNO │',
                '│░░░░░│',
                '└─────┘'
            ];
            const discardBox = currentCard ? currentCard.getAsciiLines(false) : [
                '┌─────┐',
                '│     │',
                '│ --- │',
                '│     │',
                '└─────┘'
            ];
            const disClass = currentCard ? currentCard.getColorClass() : 'c-white';

            html += '          <span class="c-darkgray">[ HUZOPAKLI ]</span>           <span class="c-darkgray">[ DOBOPAKLI ]</span>\n';
            for (let r = 0; r < 5; r++) {
                html += `            <span class="c-red">${drawBox[r]}</span>`;
                html += r === 2 ? '     --->     ' : '              ';
                html += `<span class="${disClass}">${discardBox[r]}</span>\n`;
            }
            const colName = currentCard ? (currentCard.color === 'Red' ? 'Piros' : currentCard.color === 'Blue' ? 'Kek' : currentCard.color === 'Green' ? 'Zold' : 'Sarga') : 'None';
            html += `                                  <span class="${disClass}">Aktiv szin: [ ${colName} ]</span>\n\n`;

            html += `  <span class="${lastActionColor} font-bold">>> ${lastAction}</span>\n`;
            html += '<span class="c-darkgray">--------------------------------------------------------------------------</span>\n';

            if (pickingWildColor) {
                html += '\n  <span class="c-yellow font-bold">=== VALASSZ SZINT: ===</span>\n';
                html += '  <button onclick="window.UNO.chooseColor(\'Red\')" class="tui-btn c-red">[1] PIROS</button> ';
                html += '  <button onclick="window.UNO.chooseColor(\'Blue\')" class="tui-btn c-blue">[2] KEK</button> ';
                html += '  <button onclick="window.UNO.chooseColor(\'Green\')" class="tui-btn c-green">[3] ZOLD</button> ';
                html += '  <button onclick="window.UNO.chooseColor(\'Yellow\')" class="tui-btn c-yellow">[4] SARGA</button>\n';
                html += '<span class="c-darkgray">--------------------------------------------------------------------------</span>\n';
            } else {
                const pageSize = 7;
                const pageStart = Math.floor(selectedIndex / pageSize) * pageSize;
                const pageEnd = Math.min(pageStart + pageSize, playerHand.length);

                html += `  <span class="c-white">LAPJAID (${playerHand.length} db) - [${selectedIndex + 1}. kivalasztva]:</span>\n`;

                html += '    ';
                html += pageStart > 0 ? '<span class="c-cyan">&lt;&lt; </span>' : '   ';
                for (let i = pageStart; i < pageEnd; i++) {
                    if (i === selectedIndex) {
                        html += `<span class="c-yellow font-bold"> &gt;[${i + 1}]&lt; </span>`;
                    } else {
                        html += `<span class="c-gray">   [${i + 1}]  </span>`;
                    }
                }
                if (pageEnd < playerHand.length) {
                    html += '<span class="c-cyan"> &gt;&gt;</span>';
                }
                html += '\n';

                const rendered = [];
                for (let i = pageStart; i < pageEnd; i++) {
                    rendered.push(playerHand[i].getAsciiLines(i === selectedIndex));
                }

                for (let r = 0; r < 5; r++) {
                    html += '    ';
                    html += (pageStart > 0 && r === 2) ? '<span class="c-cyan">&lt;- </span>' : '   ';
                    for (let c = 0; c < rendered.length; c++) {
                        const cardIdx = pageStart + c;
                        const cClass = playerHand[cardIdx].getColorClass();
                        html += `<span class="${cClass}">${rendered[c][r]}</span> `;
                    }
                    if (pageEnd < playerHand.length && r === 2) {
                        html += '<span class="c-cyan">-&gt;</span>';
                    }
                    html += '\n';
                }

                html += '<span class="c-darkgray">--------------------------------------------------------------------------</span>\n';
                html += '  <span class="c-white">[<- / ->]: Lap kivalasztasa  |  [ENTER]: Lerakas  |  [D]: Huzas  |  [Q]: Menü</span>\n';
            }
        }
        else if (currentScreen === 'VICTORY') {
            html += '<span class="c-yellow font-bold">';
            html += '             ___________             \n';
            html += '            \'.__==_==_==_.\'            \n';
            html += '            .-\\:      /-.            \n';
            html += '           | (|:.     |) |           \n';
            html += '            \'-|:.     |-\'            \n';
            html += '              \\::.    /              \n';
            html += '               \'::. .\'               \n';
            html += '                 ) (                 \n';
            html += '               _.\' \'._               \n';
            html += '              `"""""""`              \n';
            html += '</span>\n';
            html += `  <span class="c-green font-bold text-lg">*** GYOZELEM! ${winner} megnyerte a jatekot! ***</span>\n\n`;
            html += '  <button onclick="window.UNO.setScreen(\'MAIN_MENU\')" class="tui-btn tui-btn-primary">VISSZA A FOMENUBE (Q)</button>\n';
        }

        screen.innerHTML = html;
    }

    function selectMenu(keyStr) {
        Sound.beep();
        if (currentScreen === 'MAIN_MENU') {
            if (keyStr === '1') setScreen('SINGLE_PLAYER_MENU');
            else if (keyStr === '2') setScreen('MULTIPLAYER_MENU');
            else if (keyStr === '3') setScreen('SETTINGS');
            else if (keyStr === '4') setScreen('TUTORIAL');
            else if (keyStr === '5') {
                lastAction = 'Kileptel a menubol. Nyomj 1-es gombot a jatek inditasahoz!';
                render();
            }
        }
        else if (currentScreen === 'SINGLE_PLAYER_MENU') {
            if (keyStr === '1') startMatch(1);
            else if (keyStr === '2') startMatch(2);
            else if (keyStr === '3') startMatch(3);
            else if (keyStr === '4') setScreen('MAIN_MENU');
        }
        else if (currentScreen === 'MULTIPLAYER_MENU') {
            if (keyStr === '1' || keyStr === '2' || keyStr === '3') startMatch(parseInt(keyStr));
            else if (keyStr === '4') setScreen('MAIN_MENU');
        }
        else if (currentScreen === 'SETTINGS') {
            if (keyStr === '1') {
                isHungarian = !isHungarian;
                render();
            } else if (keyStr === '2') {
                window.UNO.toggleSound();
                render();
            } else if (keyStr === '3') {
                setScreen('MAIN_MENU');
            }
        }
    }

    window.addEventListener('keydown', (e) => {
        if (['ArrowLeft', 'ArrowRight', 'Space'].includes(e.code)) {
            e.preventDefault();
        }

        if (currentScreen === 'MAIN_MENU' || currentScreen === 'SINGLE_PLAYER_MENU' || currentScreen === 'MULTIPLAYER_MENU' || currentScreen === 'SETTINGS') {
            if (['1', '2', '3', '4', '5'].includes(e.key)) {
                selectMenu(e.key);
                return;
            }
        }

        if (currentScreen === 'TUTORIAL') {
            setScreen('MAIN_MENU');
            return;
        }

        if (e.key === 'q' || e.key === 'Q') {
            setScreen('MAIN_MENU');
            return;
        }

        if (currentScreen === 'GAME') {
            if (pickingWildColor) {
                if (e.key === '1') chooseColor('Red');
                if (e.key === '2') chooseColor('Blue');
                if (e.key === '3') chooseColor('Green');
                if (e.key === '4') chooseColor('Yellow');
                return;
            }

            if (e.key === 'ArrowLeft') {
                if (selectedIndex > 0) {
                    selectedIndex--;
                    Sound.beep();
                    render();
                }
            } else if (e.key === 'ArrowRight') {
                if (selectedIndex < playerHand.length - 1) {
                    selectedIndex++;
                    Sound.beep();
                    render();
                }
            } else if (e.key === 'Enter' || e.code === 'Space') {
                playPlayerCard(selectedIndex);
            } else if (e.key === 'd' || e.key === 'D') {
                handlePlayerDraw();
            } else if (!isNaN(parseInt(e.key)) && parseInt(e.key) >= 1 && parseInt(e.key) <= 9) {
                const pageSize = 7;
                const pageStart = Math.floor(selectedIndex / pageSize) * pageSize;
                const targetIdx = pageStart + (parseInt(e.key) - 1);
                if (targetIdx < playerHand.length) {
                    selectedIndex = targetIdx;
                    playPlayerCard(selectedIndex);
                }
            }
        }
    });

    window.UNO = {
        setScreen,
        selectMenu,
        moveLeft: () => {
            if (currentScreen === 'GAME' && selectedIndex > 0) {
                selectedIndex--;
                Sound.beep();
                render();
            }
        },
        moveRight: () => {
            if (currentScreen === 'GAME' && selectedIndex < playerHand.length - 1) {
                selectedIndex++;
                Sound.beep();
                render();
            }
        },
        playCurrent: () => playPlayerCard(selectedIndex),
        draw: handlePlayerDraw,
        chooseColor,
        toggleSound: () => {
            soundEnabled = !soundEnabled;
            const btn = document.getElementById('sound-btn');
            if (btn) {
                btn.textContent = soundEnabled ? 'Hang: BE' : 'Hang: KI';
            }
            if (soundEnabled) Sound.beep();
        },
        toggleTheme: () => {
            document.body.classList.toggle('light-mode');
            const btn = document.getElementById('theme-btn');
            if (btn) {
                const isLight = document.body.classList.contains('light-mode');
                btn.textContent = isLight ? 'Light Mode' : 'Dark Mode';
            }
        }
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => render());
    } else {
        render();
    }
})();
