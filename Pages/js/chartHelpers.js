window.chartHelpers = {
    charts: {},

    createChart: function(chartId, config) {
        const ctx = document.getElementById(chartId).getContext('2d');
        if (this.charts[chartId]) {
            this.charts[chartId].destroy();
        }
        this.charts[chartId] = new Chart(ctx, config);
    },

    updateChart: function(chartId, data) {
        if (this.charts[chartId]) {
            this.charts[chartId].data = { ...this.charts[chartId].data, ...data };
            this.charts[chartId].update();
        }
    },

    destroyChart: function(chartId) {
        if (this.charts[chartId]) {
            this.charts[chartId].destroy();
            delete this.charts[chartId];
        }
    },

    addCenterText: function(chartId, mainText, subText) {
        const chart = this.charts[chartId];
        if (!chart) return;
        chart.options.plugins.centerText = { mainText: mainText, subText: subText };
        Chart.register({
            id: 'centerText',
            afterDraw: function(chart) {
                if (chart.config.type !== 'doughnut') return;
                const ctx = chart.ctx;
                const centerX = (chart.chartArea.left + chart.chartArea.right) / 2;
                const centerY = (chart.chartArea.top + chart.chartArea.bottom) / 2;
                ctx.save();
                ctx.font = 'bold 24px sans-serif';
                ctx.fillStyle = '#1e293b';
                ctx.textAlign = 'center';
                ctx.textBaseline = 'middle';
                ctx.fillText(mainText, centerX, centerY - 10);
                if (subText) {
                    ctx.font = '14px sans-serif';
                    ctx.fillStyle = '#64748b';
                    ctx.fillText(subText, centerX, centerY + 15);
                }
                ctx.restore();
            }
        });
        chart.update();
    },

    updateCenterText: function(chartId, mainText) {
        const chart = this.charts[chartId];
        if (chart && chart.options.plugins.centerText) {
            chart.options.plugins.centerText.mainText = mainText;
            chart.update();
        }
    }
};
