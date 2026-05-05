document.addEventListener("DOMContentLoaded", () => {
    const canvases = Array.from(document.querySelectorAll(".question-chart"));

    if (!canvases.length || typeof Chart === "undefined") {
        return;
    }

    const chartInstances = {};
    const chartData = {};

    function parseChartData(canvas) {
        try {
            return {
                labels: JSON.parse(canvas.dataset.labels || "[]"),
                counts: JSON.parse(canvas.dataset.counts || "[]"),
                colors: JSON.parse(canvas.dataset.colors || "[]")
            };
        }
        catch {
            return null;
        }
    }

    function renderPieChart(chartId) {
        const canvas = document.getElementById(chartId);

        if (!canvas || !chartData[chartId]) {
            return;
        }

        const data = chartData[chartId];
        const ctx = canvas.getContext("2d");

        chartInstances[chartId] = new Chart(ctx, {
            type: "pie",
            data: {
                labels: data.labels,
                datasets: [{
                    data: data.counts,
                    backgroundColor: data.colors
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        displayColors: false,
                        callbacks: {
                            label(context) {
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = total > 0
                                    ? ((context.parsed / total) * 100).toFixed(1)
                                    : "0.0";

                                return "Count: " + context.parsed + " (" + percentage + "%)";
                            }
                        }
                    }
                }
            }
        });
    }

    function renderHistogram(chartId) {
        const canvas = document.getElementById(chartId);

        if (!canvas || !chartData[chartId]) {
            return;
        }

        const data = chartData[chartId];
        const ctx = canvas.getContext("2d");

        chartInstances[chartId] = new Chart(ctx, {
            type: "bar",
            data: {
                labels: data.labels,
                datasets: [{
                    label: "Responses",
                    data: data.counts,
                    backgroundColor: data.colors,
                    borderColor: data.colors,
                    borderWidth: 1
                }]
            },
            options: {
                indexAxis: "y",
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        displayColors: false,
                        callbacks: {
                            label(context) {
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = total > 0
                                    ? ((context.parsed.x / total) * 100).toFixed(1)
                                    : "0.0";

                                return "Count: " + context.parsed.x + " (" + percentage + "%)";
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        beginAtZero: true,
                        ticks: {
                            color: "#fff",
                            stepSize: 1
                        },
                        grid: { color: "rgba(255, 255, 255, 0.1)" }
                    },
                    y: {
                        ticks: { color: "#fff" },
                        grid: { color: "rgba(255, 255, 255, 0.1)" }
                    }
                }
            }
        });
    }

    canvases.forEach((canvas) => {
        const data = parseChartData(canvas);

        if (!data) {
            return;
        }

        chartData[canvas.id] = data;
        chartInstances[canvas.id] = null;
        renderPieChart(canvas.id);
    });

    document.querySelectorAll('input[name="chartType"]').forEach((radio) => {
        radio.addEventListener("change", function () {
            const chartType = this.value;

            canvases.forEach((canvas) => {
                const chartId = canvas.id;

                if (chartInstances[chartId]) {
                    chartInstances[chartId].destroy();
                    chartInstances[chartId] = null;
                }

                if (chartType === "chart") {
                    renderPieChart(chartId);
                }
                else {
                    renderHistogram(chartId);
                }
            });
        });
    });
});
