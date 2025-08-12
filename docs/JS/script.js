document.addEventListener('DOMContentLoaded', () => {
    document.body.classList.add('bg-gray-900', 'text-white', 'p-6', 'font-sans');

    // Cím
    const cim = document.createElement('h1');
    cim.textContent = 'UNO Kártyajáték Letöltése';
    cim.classList.add('text-3xl', 'font-bold', 'text-center', 'mb-4');
    document.body.appendChild(cim);

    // Leírás
    const leiras = document.createElement('p');
    leiras.textContent = 'Töltsd le és játssz az UNO kártyajátékkal! Egyszerű, szórakoztató és ingyenes.';
    leiras.classList.add('text-center', 'mb-6');
    document.body.appendChild(leiras);

    // Letöltési konténer
    const downloadContainer = document.createElement('div');
    downloadContainer.classList.add('bg-gray-800', 'p-6', 'rounded-lg', 'shadow-lg', 'max-w-md', 'mx-auto', 'mb-6');
    document.body.appendChild(downloadContainer);

    // Verzió információ
    const verzio = document.createElement('p');
    verzio.innerHTML = '<strong>Verzió:</strong> 1.0.0';
    verzio.classList.add('mb-2');
    downloadContainer.appendChild(verzio);

    // Kompatibilitás
    const kompatibilitas = document.createElement('p');
    kompatibilitas.innerHTML = '<strong>Kompatibilitás:</strong> Windows 10/11';
    kompatibilitas.classList.add('mb-4');
    downloadContainer.appendChild(kompatibilitas);

    // Letöltési gomb
    const downloadBtn = document.createElement('button');
    downloadBtn.textContent = 'Letöltés';
    downloadBtn.classList.add('bg-green-600', 'text-white', 'p-2', 'rounded', 'hover:bg-green-700', 'transition', 'w-full');
    downloadBtn.addEventListener('click', () => {
        alert('A letöltés jelenleg nem elérhető. A végleges verzió hamarosan!');
    });
    downloadContainer.appendChild(downloadBtn);
});