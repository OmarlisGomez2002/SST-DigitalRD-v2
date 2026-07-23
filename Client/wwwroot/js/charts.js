window.SSTCharts = {

    renderIncidentes: function (canvasId) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;
        if (ctx._chartInstance) ctx._chartInstance.destroy();

        ctx._chartInstance = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['Sem 1', 'Sem 2', 'Sem 3', 'Sem 4', 'Sem 5', 'Sem 6'],
                datasets: [
                    {
                        label: 'Reportados',
                        data: [8, 12, 6, 10, 5, 9],
                        backgroundColor: '#B5D4F4',
                        borderRadius: 4,
                        borderSkipped: false,
                    },
                    {
                        label: 'Graves',
                        data: [3, 6, 2, 4, 2, 3],
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
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: '#1A1A19',
                        titleFont: { family: 'DM Sans', size: 12 },
                        bodyFont: { family: 'DM Sans', size: 11 },
                        padding: 10,
                        cornerRadius: 8,
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        border: { display: false },
                        ticks: { color: '#888780', font: { family: 'DM Sans', size: 11 } }
                    },
                    y: {
                        grid: { color: 'rgba(0,0,0,0.05)' },
                        border: { display: false, dash: [4, 4] },
                        ticks: {
                            color: '#888780',
                            font: { family: 'DM Sans', size: 11 },
                            stepSize: 4,
                            maxTicksLimit: 5
                        }
                    }
                },
                animation: { duration: 600, easing: 'easeOutQuart' }
            }
        });
    },

    renderRiesgo: function (canvasId, labels, datos) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;
        if (ctx._chartInstance) ctx._chartInstance.destroy();

        const semanas = labels && labels.length
            ? labels
            : ['Sem -5', 'Sem -4', 'Sem -3', 'Sem -2', 'Sem -1', 'Hoy'];

        const valores = datos && datos.length
            ? datos
            : [0, 0, 0, 0, 0, 0];

        const maxVal = Math.max(...valores, 1);
        const colors = valores.map(v => {
            const pct = (v / maxVal) * 100;
            return pct >= 70 ? '#E24B4A'
                : pct >= 40 ? '#EF9F27'
                    : '#B5D4F4';
        });

        const Chart = window.Chart;
        if (!Chart) return;

        ctx._chartInstance = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: semanas,
                datasets: [{
                    data: valores,
                    backgroundColor: colors,
                    borderRadius: 4,
                    borderSkipped: false,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: '#1A1A19',
                        callbacks: {
                            label: c => ` ${c.raw} incidente(s)`
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        border: { display: false },
                        ticks: {
                            color: '#888780',
                            font: { family: 'DM Sans', size: 11 }
                        }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1,
                            color: '#888780',
                            font: { family: 'DM Sans', size: 11 }
                        },
                        grid: { color: 'rgba(0,0,0,0.05)' }
                    }
                },
                animation: { duration: 600, easing: 'easeOutQuart' }
            }
        });
    },

    //renderRiesgo: function (canvasId) {
    //    const ctx = document.getElementById(canvasId);
    //    if (!ctx) return;
    //    if (ctx._chartInstance) ctx._chartInstance.destroy();

    //    const colors = [
    //        '#B5D4F4', '#B5D4F4',
    //        '#EF9F27', '#EF9F27',
    //        '#E24B4A', '#E24B4A'
    //    ];

    //    ctx._chartInstance = new Chart(ctx, {
    //        type: 'bar',
    //        data: {
    //            labels: ['Sem 1', 'Sem 2', 'Sem 3', 'Sem 4', 'Sem 5', 'Hoy'],
    //            datasets: [{
    //                data: [50, 65, 45, 80, 70, 91],
    //                backgroundColor: colors,
    //                borderRadius: 4,
    //                borderSkipped: false,
    //            }]
    //        },
    //        options: {
    //            responsive: true,
    //            maintainAspectRatio: false,
    //            plugins: {
    //                legend: { display: false },
    //                tooltip: {
    //                    backgroundColor: '#1A1A19',
    //                    titleFont: { family: 'DM Sans', size: 12 },
    //                    bodyFont: { family: 'DM Sans', size: 11 },
    //                    padding: 10,
    //                    cornerRadius: 8,
    //                    callbacks: {
    //                        label: ctx => ` Riesgo: ${Math.round(ctx.raw)}%`
    //                    }
    //                }
    //            },
    //            scales: {
    //                x: {
    //                    grid: { display: false },
    //                    border: { display: false },
    //                    ticks: { color: '#888780', font: { family: 'DM Sans', size: 11 } }
    //                },
    //                y: {
    //                    min: 0, max: 100,
    //                    grid: { color: 'rgba(0,0,0,0.05)' },
    //                    border: { display: false },
    //                    ticks: {
    //                        color: '#888780',
    //                        font: { family: 'DM Sans', size: 11 },
    //                        stepSize: 25,
    //                        callback: v => v + '%'
    //                    }
    //                }
    //            },
    //            animation: { duration: 600, easing: 'easeOutQuart' }
    //        }
    //    });
    //},

    destroyChart: function (canvasId) {
        const ctx = document.getElementById(canvasId);
        if (ctx && ctx._chartInstance) {
            ctx._chartInstance.destroy();
            delete ctx._chartInstance;
        }
    }
};

window.SSTGemelo = {

    renderMapa: function (canvasId, zonas) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        const parent = canvas.parentElement;
        const W = (parent && parent.offsetWidth > 10) ? parent.offsetWidth : 560;
        const H = (parent && parent.offsetHeight > 10) ? parent.offsetHeight : 340;

        if (W < 10 || H < 10) {
            setTimeout(() => window.SSTGemelo.renderMapa(canvasId, zonas), 200);
            return;
        }

        canvas.width = W;
        canvas.height = H;

        const ctx = canvas.getContext('2d');
        const scaleX = W / 560;
        const scaleY = H / 340;

        ctx.clearRect(0, 0, W, H);

        // Fondo del plano
        ctx.fillStyle = '#F0EFEC';
        ctx.strokeStyle = '#D3D1C7';
        ctx.lineWidth = 0.5;
        window.SSTGemelo._roundRect(ctx,
            20 * scaleX, 20 * scaleY, 520 * scaleX, 300 * scaleY, 4);
        ctx.fill();
        ctx.stroke();

        // Dibujar cada zona
        zonas.forEach(function (z) {
            const x = z.x * scaleX;
            const y = z.y * scaleY;
            const w = z.w * scaleX;
            const h = z.h * scaleY;
            const cx = x + w / 2;
            const cy = y + h / 2;
            const radio = Math.min(12 + z.incidentes * 6, 32) *
                Math.min(scaleX, scaleY);

            const r = z.riesgo >= 70 ? 226 : z.riesgo >= 50 ? 239 : 99;
            const g = z.riesgo >= 70 ? 75 : z.riesgo >= 50 ? 159 : 153;
            const b = z.riesgo >= 70 ? 74 : z.riesgo >= 50 ? 39 : 34;

            // Fondo de la zona
            ctx.fillStyle = `rgba(${r},${g},${b},0.15)`;
            ctx.strokeStyle = `rgba(${r},${g},${b},0.8)`;
            ctx.lineWidth = 1;
            window.SSTGemelo._roundRect(ctx, x, y, w, h, 3);
            ctx.fill();
            ctx.stroke();

            // Círculo exterior (densidad)
            ctx.beginPath();
            ctx.arc(cx, cy, radio * 1.6, 0, Math.PI * 2);
            ctx.fillStyle = `rgba(${r},${g},${b},0.2)`;
            ctx.fill();

            // Círculo interior
            ctx.beginPath();
            ctx.arc(cx, cy, radio, 0, Math.PI * 2);
            ctx.fillStyle = `rgba(${r},${g},${b},0.45)`;
            ctx.fill();

            // Porcentaje
            ctx.fillStyle = z.riesgo >= 70 ? '#791F1F'
                : z.riesgo >= 50 ? '#633806' : '#27500A';
            ctx.font = `500 ${Math.round(11 * Math.min(scaleX, scaleY))}px DM Sans, sans-serif`;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillText(z.riesgo + '%', cx, cy);

            // Nombre de la zona
            ctx.fillStyle = '#5F5E5A';
            ctx.font = `400 ${Math.round(9 * Math.min(scaleX, scaleY))}px DM Sans, sans-serif`;
            const nombre = z.nombre.length > 18
                ? z.nombre.substring(0, 16) + '...' : z.nombre;
            ctx.fillText(nombre, cx, y + h - 12 * scaleY);
        });
    },

    _roundRect: function (ctx, x, y, w, h, r) {
        ctx.beginPath();
        ctx.moveTo(x + r, y);
        ctx.lineTo(x + w - r, y);
        ctx.quadraticCurveTo(x + w, y, x + w, y + r);
        ctx.lineTo(x + w, y + h - r);
        ctx.quadraticCurveTo(x + w, y + h, x + w - r, y + h);
        ctx.lineTo(x + r, y + h);
        ctx.quadraticCurveTo(x, y + h, x, y + h - r);
        ctx.lineTo(x, y + r);
        ctx.quadraticCurveTo(x, y, x + r, y);
        ctx.closePath();
    }
};