// SST-Digital RD · GPS + Firma Digital (JS Interop)

// ══════════════════════════════════════
//  GEOLOCALIZACIÓN
// ══════════════════════════════════════
window.SSTGeo = {

    getPosition: function () {
        return new Promise((resolve, reject) => {
            if (!navigator.geolocation) {
                reject(new Error("Este navegador no soporta geolocalización."));
                return;
            }

            navigator.geolocation.getCurrentPosition(
                (pos) => {
                    resolve({
                        latitude:  pos.coords.latitude,
                        longitude: pos.coords.longitude,
                        accuracy:  pos.coords.accuracy
                    });
                },
                (err) => {
                    const msgs = {
                        1: "Permiso de ubicación denegado por el usuario.",
                        2: "No se pudo determinar la posición actual.",
                        3: "La solicitud de ubicación expiró."
                    };
                    reject(new Error(msgs[err.code] || "Error de geolocalización."));
                },
                {
                    enableHighAccuracy: true,
                    timeout: 10000,
                    maximumAge: 0
                }
            );
        });
    }
};

// ══════════════════════════════════════
//  FIRMA DIGITAL (Canvas)
// ══════════════════════════════════════
window.SSTFirma = {

    _estados: {},

    init: function (canvasId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        const ctx = canvas.getContext("2d");
        ctx.strokeStyle = "#1A1A19";
        ctx.lineWidth   = 2;
        ctx.lineCap     = "round";
        ctx.lineJoin    = "round";

        let dibujando = false;
        let ultimoX = 0, ultimoY = 0;

        const getPos = (e) => {
            const r = canvas.getBoundingClientRect();
            const src = e.touches ? e.touches[0] : e;
            return {
                x: (src.clientX - r.left) * (canvas.width / r.width),
                y: (src.clientY - r.top)  * (canvas.height / r.height)
            };
        };

        const start = (e) => {
            e.preventDefault();
            dibujando = true;
            const { x, y } = getPos(e);
            ultimoX = x; ultimoY = y;
            this._estados[canvasId] = true;
        };

        const draw = (e) => {
            if (!dibujando) return;
            e.preventDefault();
            const { x, y } = getPos(e);
            ctx.beginPath();
            ctx.moveTo(ultimoX, ultimoY);
            ctx.lineTo(x, y);
            ctx.stroke();
            ultimoX = x; ultimoY = y;
        };

        const stop = () => { dibujando = false; };

        canvas.addEventListener("mousedown",  start);
        canvas.addEventListener("mousemove",  draw);
        canvas.addEventListener("mouseup",    stop);
        canvas.addEventListener("mouseleave", stop);
        canvas.addEventListener("touchstart", start, { passive: false });
        canvas.addEventListener("touchmove",  draw,  { passive: false });
        canvas.addEventListener("touchend",   stop);
    },

    getDataUrl: function (canvasId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return "";
        return canvas.toDataURL("image/png");
    },

    clear: function (canvasId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const ctx = canvas.getContext("2d");
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        this._estados[canvasId] = false;
    },

    isEmpty: function (canvasId) {
        return !this._estados[canvasId];
    }
};

window.SSTVision = {

    _stream: null,

    iniciar: async function (videoId, canvasId) {
        const video = document.getElementById(videoId);
        const canvas = document.getElementById(canvasId);
        if (!video) return;

        try {
            const stream = await navigator.mediaDevices.getUserMedia({
                video: { facingMode: 'environment', width: 640, height: 480 }
            });
            this._stream = stream;
            video.srcObject = stream;

            if (canvas) {
                canvas.width = 640;
                canvas.height = 480;
            }
        } catch (err) {
            const msgs = {
                'NotAllowedError': 'Permiso de cámara denegado.',
                'NotFoundError': 'No se encontró ninguna cámara.',
                'NotReadableError': 'La cámara está siendo usada por otra aplicación.'
            };
            throw new Error(msgs[err.name] || 'Error al acceder a la cámara.');
        }
    },

    detener: function (videoId) {
        const video = document.getElementById(videoId);
        if (video && video.srcObject) {
            video.srcObject.getTracks().forEach(t => t.stop());
            video.srcObject = null;
        }
        this._stream = null;
    },

    capturarFrame: function (videoId, canvasId) {
        const video = document.getElementById(videoId);
        const canvas = document.getElementById(canvasId);
        if (!video || !canvas) return null;

        const ctx = canvas.getContext('2d');
        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
        return canvas.toDataURL('image/jpeg', 0.8);
    }
};

//PDF
window.SSTUtil = {
    descargarArchivo: function (nombreArchivo, base64Data, tipoMime) {
        const link = document.createElement('a');
        link.href = `data:${tipoMime};base64,${base64Data}`;
        link.download = nombreArchivo;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    },

    abrirPdfParaImprimir: function (base64Data) {
        const byteChars = atob(base64Data);
        const byteNums = new Array(byteChars.length);
        for (let i = 0; i < byteChars.length; i++)
            byteNums[i] = byteChars.charCodeAt(i);
        const blob = new Blob(
            [new Uint8Array(byteNums)],
            { type: 'application/pdf' }
        );
        const url = URL.createObjectURL(blob);
        const newTab = window.open(url, '_blank');
        if (newTab) {
            newTab.addEventListener('load', () => {
                newTab.print();
                URL.revokeObjectURL(url);
            });
        }
    }
};



window.SSTGemelo = {

    _zonas: [],
    _callback: null,

    renderMapa: function (canvasId, zonas) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        this._zonas = zonas;
        const ctx = canvas.getContext('2d');
        const W = canvas.offsetWidth || 520;
        const H = canvas.offsetHeight || 320;

        canvas.width = W;
        canvas.height = H;

        // Fondo
        ctx.fillStyle = '#F8F8F7';
        ctx.fillRect(0, 0, W, H);

        // Grid de referencia
        ctx.strokeStyle = '#E8E7E2';
        ctx.lineWidth = 0.5;
        for (let x = 0; x < W; x += 40) {
            ctx.beginPath(); ctx.moveTo(x, 0); ctx.lineTo(x, H); ctx.stroke();
        }
        for (let y = 0; y < H; y += 40) {
            ctx.beginPath(); ctx.moveTo(0, y); ctx.lineTo(W, y); ctx.stroke();
        }

        // Dibujar zonas
        zonas.forEach(z => {
            const scaleX = W / 560;
            const scaleY = H / 320;
            const x = z.x * scaleX;
            const y = z.y * scaleY;
            const w = z.w * scaleX;
            const h = z.h * scaleY;

            // Color según riesgo
            let fillColor, strokeColor;
            if (z.riesgo >= 70) {
                fillColor = 'rgba(226,75,74,0.18)';
                strokeColor = 'rgba(226,75,74,0.7)';
            } else if (z.riesgo >= 50) {
                fillColor = 'rgba(239,159,39,0.18)';
                strokeColor = 'rgba(239,159,39,0.7)';
            } else {
                fillColor = 'rgba(99,153,34,0.12)';
                strokeColor = 'rgba(99,153,34,0.5)';
            }

            // Rectángulo de zona
            ctx.fillStyle = fillColor;
            ctx.strokeStyle = strokeColor;
            ctx.lineWidth = 1.5;
            ctx.beginPath();
            ctx.roundRect(x, y, w, h, 6);
            ctx.fill();
            ctx.stroke();

            // Nombre de la zona
            ctx.fillStyle = '#3d3d3a';
            ctx.font = '500 11px sans-serif';
            ctx.fillText(z.nombre, x + 8, y + 16);

            // Porcentaje de riesgo
            if (z.riesgo >= 70) ctx.fillStyle = '#A32D2D';
            else if (z.riesgo >= 50) ctx.fillStyle = '#854F0B';
            else ctx.fillStyle = '#3B6D11';

            ctx.font = '700 13px sans-serif';
            ctx.fillText(z.riesgo + '%', x + 8, y + 32);

            // Círculo de calor proporcional a incidentes
            if (z.incidentes > 0) {
                const cx = x + w / 2;
                const cy = y + h / 2;
                const r = Math.min(20 + z.incidentes * 8, Math.min(w, h) / 2 - 6);

                ctx.beginPath();
                ctx.arc(cx, cy, r, 0, Math.PI * 2);
                ctx.fillStyle = z.riesgo >= 70
                    ? 'rgba(226,75,74,0.30)'
                    : z.riesgo >= 50
                        ? 'rgba(239,159,39,0.25)'
                        : 'rgba(99,153,34,0.20)';
                ctx.fill();

                // Número de incidentes en el centro
                ctx.fillStyle = z.riesgo >= 70 ? '#A32D2D'
                    : z.riesgo >= 50 ? '#854F0B' : '#3B6D11';
                ctx.font = '600 12px sans-serif';
                ctx.textAlign = 'center';
                ctx.textBaseline = 'middle';
                ctx.fillText(z.incidentes, cx, cy);
                ctx.textAlign = 'start';
                ctx.textBaseline = 'alphabetic';
            }
        });

        // Guardar referencia para click
        canvas._zonas = zonas;
        canvas._scaleX = W / 560;
        canvas._scaleY = H / 320;
    },

    getZonaEnPunto: function (canvasId, clientX, clientY) {
        const canvas = document.getElementById(canvasId);
        if (!canvas || !canvas._zonas) return null;

        const rect = canvas.getBoundingClientRect();
        const x = clientX - rect.left;
        const y = clientY - rect.top;
        const scaleX = canvas._scaleX || 1;
        const scaleY = canvas._scaleY || 1;

        for (const z of canvas._zonas) {
            const zx = z.x * scaleX;
            const zy = z.y * scaleY;
            const zw = z.w * scaleX;
            const zh = z.h * scaleY;

            if (x >= zx && x <= zx + zw && y >= zy && y <= zy + zh)
                return z.nombre;
        }
        return null;
    }
};


window.SSTCharts = {

    renderIncidentes: function (canvasId, labels, reportados, graves) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        // Destruir instancia anterior si existe
        if (canvas._chartInstance) {
            canvas._chartInstance.destroy();
        }

        // Labels por defecto si no vienen datos
        const semanas = labels && labels.length
            ? labels
            : ['Sem 1', 'Sem 2', 'Sem 3', 'Sem 4', 'Sem 5', 'Hoy'];
        const dataRep = reportados && reportados.length
            ? reportados : [1, 2, 1, 3, 2, 2];
        const dataGraves = graves && graves.length
            ? graves : [0, 1, 0, 1, 0, 1];

        const Chart = window.Chart;
        if (!Chart) return;

        canvas._chartInstance = new Chart(canvas, {
            type: 'bar',
            data: {
                labels: semanas,
                datasets: [
                    {
                        label: 'Reportados',
                        data: dataRep,
                        backgroundColor: '#B5D4F4',
                        borderRadius: 4,
                        borderSkipped: false,
                    },
                    {
                        label: 'Graves',
                        data: dataGraves,
                        backgroundColor: '#E24B4A',
                        borderRadius: 4,
                        borderSkipped: false,
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { font: { size: 11 }, color: '#888780' }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1,
                            font: { size: 11 },
                            color: '#888780'
                        },
                        grid: {
                            color: 'rgba(0,0,0,0.05)'
                        }
                    }
                }
            }
        });
    },

    renderRiesgo: function (canvasId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        if (canvas._chartInstance)
            canvas._chartInstance.destroy();

        const Chart = window.Chart;
        if (!Chart) return;

        canvas._chartInstance = new Chart(canvas, {
            type: 'bar',
            data: {
                labels: ['Sem 1', 'Sem 2', 'Sem 3', 'Sem 4', 'Sem 5', 'Hoy'],
                datasets: [{
                    data: [22, 28, 45, 58, 71, 74],
                    backgroundColor: [
                        '#B5D4F4', '#B5D4F4',
                        '#FAC775', '#FAC775',
                        '#F09595', '#E24B4A'
                    ],
                    borderRadius: 4,
                    borderSkipped: false,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { font: { size: 10 }, color: '#888780' }
                    },
                    y: {
                        beginAtZero: true,
                        max: 100,
                        ticks: { font: { size: 10 }, color: '#888780' },
                        grid: { color: 'rgba(0,0,0,0.05)' }
                    }
                }
            }
        });
    }
};