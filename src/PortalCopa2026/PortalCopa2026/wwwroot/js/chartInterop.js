// Módulo de interop reutilizável para gráficos Chart.js (carregado via IJSObjectReference).
// Depende do global `Chart` fornecido pelo script UMD (wwwroot/lib/chartjs/chart.umd.min.js),
// registrado em App.razor.

const charts = new Map();

export function initChart(elementId, config) {
    const canvas = document.getElementById(elementId);
    if (!canvas) {
        return;
    }

    destroyChart(elementId);

    const chart = new Chart(canvas, config);
    charts.set(elementId, chart);
}

export function updateChart(elementId, config) {
    const chart = charts.get(elementId);
    if (!chart) {
        initChart(elementId, config);
        return;
    }

    chart.data = config.data;
    chart.options = config.options;
    chart.update();
}

export function destroyChart(elementId) {
    const chart = charts.get(elementId);
    if (chart) {
        chart.destroy();
        charts.delete(elementId);
    }
}
