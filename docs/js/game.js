
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
            const colMap = { Red: 'Piros', Blue: 'Kek', Green: 'Zold', Yellow: 'Sarga', Wild: 'Szinvalaszto' };
            const valMap = { Skip: 'Kimaradsz', Reverse: 'Fordito', '+2': '+2', '+4': '+4 Vad', Wild: 'Vad' };
            const c = colMap[this.color] || this.color;
            const v = valMap[this.value] || this.value;
            return this.color === 'Wild' && this.value !== '+4' ? 'Vad (Szinvalaszto)' : `${c} ${v}`;
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

    let deck = [];
    let discardPile = [];
    let playerHand = [];
    let aiHand = [];
    let currentCard = null;
    let selectedIndex = 0;
    let lastAction = 'Udv a Console UNO-ban! Nyomj egy gombot a kezdeshez.';
    let lastActionColor = 'c-cyan';
    let isGameOver = false;
    let winner = null;
    let pickingWildColor = false;
    let pendingWildCard = null;
    let isAiTurn = false;

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
                lastAction = 'A pakli elfogyott! Dobopakli ujrakeverve.';
                lastActionColor = 'c-wild';
            } else {
                return null;
            }
        }
        const card = deck.shift();
        if (hand) hand.push(card);
        return card;
    }

    function canPlay(card, target) {
        if (!target) return true;
        return card.color === 'Wild' || card.color === target.color || card.value === target.value;
    }

    function startNewGame() {
        deck = initDeck();
        discardPile = [];
        playerHand = [];
        aiHand = [];
        selectedIndex = 0;
        isGameOver = false;
        winner = null;
        pickingWildColor = false;
        pendingWildCard = null;
        isAiTurn = false;

        for (let i = 0; i < 7; i++) {
            drawCard(playerHand);
            drawCard(aiHand);
        }

        currentCard = drawCard(null);
        while (currentCard && currentCard.value === '+4') {
            deck.push(currentCard);
            shuffle(deck);
            currentCard = drawCard(null);
        }
        if (currentCard && currentCard.color === 'Wild') {
            currentCard.color = 'Red';
        }

        lastAction = 'Jatek elindult! Te vagy a soron kovetkezo.';
        lastActionColor = 'c-yellow';
        render();
    }

    function playPlayerCard(idx) {
        if (isGameOver || isAiTurn || pickingWildColor) return;
        if (idx < 0 || idx >= playerHand.length) return;

        const card = playerHand[idx];
        if (!canPlay(card, currentCard)) {
            Sound.invalid();
            lastAction = 'Ervenytelen lepes! A lap nem egyezik szinben vagy ertekben.';
            lastActionColor = 'c-red';
            render();
            return;
        }

        playerHand.splice(idx, 1);
        if (currentCard) discardPile.push(currentCard);
        currentCard = card;

        if (selectedIndex >= playerHand.length) {
            selectedIndex = Math.max(0, playerHand.length - 1);
        }

        Sound.playCard();

        if (card.color === 'Wild') {
            pickingWildColor = true;
            pendingWildCard = card;
            lastAction = 'Vad kartya lerakva! Valassz szint: [1] Piros, [2] Kek, [3] Zold, [4] Sarga';
            lastActionColor = 'c-wild';
            render();
            return;
        }

        finishPlayerTurn(card);
    }

    function chooseColor(color) {
        if (!pickingWildColor || !pendingWildCard) return;
        pendingWildCard.color = color;
        currentCard = pendingWildCard;
        pickingWildColor = false;
        pendingWildCard = null;
        Sound.beep();

        lastAction = `Valasztott szin: ${color === 'Red' ? 'Piros' : color === 'Blue' ? 'Kek' : color === 'Green' ? 'Zold' : 'Sarga'}`;
        lastActionColor = currentCard.getColorClass();

        finishPlayerTurn(currentCard);
    }

    function finishPlayerTurn(playedCard) {
        if (playerHand.length === 1) {
            Sound.uno();
            lastAction = '*** UNO! Mar csak 1 lapod maradt! ***';
            lastActionColor = 'c-yellow';
        }

        if (playerHand.length === 0) {
            winner = 'Te';
            isGameOver = true;
            Sound.victory();
            render();
            return;
        }

        let aiSkips = false;
        if (playedCard.value === 'Skip' || playedCard.value === 'Reverse') {
            Sound.special();
            aiSkips = true;
            lastAction = `[${playedCard.value === 'Skip' ? 'KIMARAD' : 'FORDITO'}] AI kimarad a korbol! Ujra te jossz.`;
            lastActionColor = 'c-yellow';
        } else if (playedCard.value === '+2') {
            Sound.special();
            drawCard(aiHand);
            drawCard(aiHand);
            aiSkips = true;
            lastAction = '[+2] AI huzott 2 lapot es kimarad! Ujra te jossz.';
            lastActionColor = 'c-red';
        } else if (playedCard.value === '+4') {
            Sound.special();
            for (let k = 0; k < 4; k++) drawCard(aiHand);
            aiSkips = true;
            lastAction = '[+4] AI huzott 4 lapot es kimarad! Ujra te jossz.';
            lastActionColor = 'c-red';
        }

        render();

        if (!aiSkips) {
            isAiTurn = true;
            setTimeout(handleAiTurn, 1000);
        }
    }

    function handleAiTurn() {
        if (isGameOver) return;

        let playIdx = -1;
        for (let i = 0; i < aiHand.length; i++) {
            if (canPlay(aiHand[i], currentCard) && ['Skip', 'Reverse', '+2', '+4', 'Wild'].includes(aiHand[i].value)) {
                playIdx = i;
                break;
            }
        }
        if (playIdx === -1) {
            for (let i = 0; i < aiHand.length; i++) {
                if (canPlay(aiHand[i], currentCard)) {
                    playIdx = i;
                    break;
                }
            }
        }

        let playerSkips = false;

        if (playIdx !== -1) {
            const played = aiHand.splice(playIdx, 1)[0];
            if (currentCard) discardPile.push(currentCard);
            currentCard = played;

            if (played.color === 'Wild') {
                const cols = ['Red', 'Blue', 'Green', 'Yellow'];
                played.color = cols[Math.floor(Math.random() * cols.length)];
            }

            Sound.playCard();
            lastAction = `AI kijatszotta: ${played.getLocalizedName()}`;
            lastActionColor = played.getColorClass();

            if (played.value === 'Skip' || played.value === 'Reverse') {
                Sound.special();
                playerSkips = true;
                lastAction += ' -> Kimaradsz a korbol!';
            } else if (played.value === '+2') {
                Sound.special();
                drawCard(playerHand);
                drawCard(playerHand);
                playerSkips = true;
                lastAction += ' -> Huzz 2 lapot es kimaradsz!';
            } else if (played.value === '+4') {
                Sound.special();
                for (let k = 0; k < 4; k++) drawCard(playerHand);
                playerSkips = true;
                lastAction += ' -> Huzz 4 lapot es kimaradsz!';
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
                lastAction = `AI huzott es azonnal lerakta: ${drawn.getLocalizedName()}`;
                lastActionColor = drawn.getColorClass();
            } else {
                lastAction = 'AI huzott egy lapot a paklibol.';
                lastActionColor = 'c-cyan';
            }
        }

        if (aiHand.length === 1) {
            Sound.uno();
            lastAction += ' *** AI: UNO! ***';
        }

        if (aiHand.length === 0) {
            winner = 'AI (Bot)';
            isGameOver = true;
            Sound.invalid();
            render();
            return;
        }

        render();

        if (playerSkips) {
            setTimeout(handleAiTurn, 1200);
        } else {
            isAiTurn = false;
        }
    }

    function handlePlayerDraw() {
        if (isGameOver || isAiTurn || pickingWildColor) return;
        const drawn = drawCard(playerHand);
        if (drawn) {
            Sound.drawCard();
            selectedIndex = playerHand.length - 1;
            lastAction = `Huztal egy lapot: ${drawn.getLocalizedName()}`;
            lastActionColor = 'c-cyan';

            isAiTurn = true;
            render();
            setTimeout(handleAiTurn, 1000);
        } else {
            Sound.invalid();
            lastAction = 'Nincs tobb lap a pakliban!';
            lastActionColor = 'c-red';
            render();
        }
    }

    function render() {
        const screen = document.getElementById('terminal-screen');
        if (!screen) return;

        if (isGameOver) {
            screen.innerHTML = renderVictoryScreen();
            return;
        }

        let html = '';
        html += '<span class="c-red">[ C O N S O L E   U N O ]</span>\n';
        html += '<span class="c-darkgray">Fejleszto: Solti Csongor Peter</span>\n';
        html += '<span class="c-darkgray">--------------------------------------------------------------------------</span>\n';

        html += `  <span class="c-white">KOVETKEZO:</span> <span class="c-yellow">${isAiTurn ? 'AI (Bot)      ' : 'Te (Jatekos 1)'}</span>`;
        html += ` | <span class="c-white">AI LAPJAI:</span> <span class="c-cyan">${aiHand.length} db</span>`;
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
            html += '  <button onclick="window.UNO.chooseColor(\'Red\')" class="ctrl-btn c-red">[1] PIROS</button> ';
            html += '  <button onclick="window.UNO.chooseColor(\'Blue\')" class="ctrl-btn c-blue">[2] KEK</button> ';
            html += '  <button onclick="window.UNO.chooseColor(\'Green\')" class="ctrl-btn c-green">[3] ZOLD</button> ';
            html += '  <button onclick="window.UNO.chooseColor(\'Yellow\')" class="ctrl-btn c-yellow">[4] SARGA</button>\n';
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
            html += '  <span class="c-white">[<- / ->]: Lap kivalasztasa  |  [ENTER]: Lerakas  |  [D]: Huzas  |  [Q]: Uj jatek</span>\n';
        }

        screen.innerHTML = html;
    }

    function renderVictoryScreen() {
        let html = '';
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
        html += '  <button onclick="window.UNO.startNewGame()" class="ctrl-btn ctrl-btn-primary">UJ JATEK INDITASA (Q)</button>\n';
        return html;
    }

    window.addEventListener('keydown', (e) => {

        if (['ArrowLeft', 'ArrowRight', 'Space'].includes(e.code)) {
            e.preventDefault();
        }

        if (e.key === 'q' || e.key === 'Q') {
            startNewGame();
            return;
        }

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
    });

    window.UNO = {
        startNewGame,
        moveLeft: () => {
            if (selectedIndex > 0) {
                selectedIndex--;
                Sound.beep();
                render();
            }
        },
        moveRight: () => {
            if (selectedIndex < playerHand.length - 1) {
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
        }
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            startNewGame();
        });
    } else {
        startNewGame();
    }
})();
