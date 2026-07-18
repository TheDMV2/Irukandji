(function() {
    'use strict';

    const apiBaseUrl = '/Plugin/ImageCache';
    let statsPollingInterval = null;

    function initializeForm() {
        const elements = {
            enableCache: document.getElementById('enableCache'),
            maxCacheSizeMb: document.getElementById('maxCacheSizeMb'),
            targetQuality: document.getElementById('targetQuality'),
            enablePreWarm: document.getElementById('enablePreWarm'),
            saveBtn: document.getElementById('saveBtn')
        };

        // Load saved config
        loadConfig(elements);

        // Save handler
        elements.saveBtn.addEventListener('click', () => saveConfig(elements));

        // Start polling stats when page loads
        startStatsPolling();
    }

    function loadConfig(elements) {
        // TODO: Fetch config from Jellyfin API and populate form
        // For now, read from localStorage or use defaults
        console.log('Loading config...');
    }

    function saveConfig(elements) {
        const config = {
            EnableCache: elements.enableCache.checked,
            MaxCacheSizeMb: parseInt(elements.maxCacheSizeMb.value, 10),
            TargetQuality: parseInt(elements.targetQuality.value, 10),
            EnablePreWarm: elements.enablePreWarm.checked
        };

        // TODO: POST config to Jellyfin API
        console.log('Saving config:', config);
        alert('Configuration saved (TODO: implement API call)');
    }

    function startStatsPolling() {
        // Poll stats endpoint every few seconds while page is open
        statsPollingInterval = setInterval(() => {
            updateStats();
        }, 3000);

        // Update immediately on load
        updateStats();

        // Stop polling when page unloads
        window.addEventListener('beforeunload', () => {
            if (statsPollingInterval) {
                clearInterval(statsPollingInterval);
            }
        });
    }

    function updateStats() {
        fetch(apiBaseUrl + '/Stats')
            .then(response => {
                if (response.status === 401 || response.status === 403) {
                    console.warn('Not authorized to view stats');
                    return null;
                }
                return response.json();
            })
            .then(data => {
                if (data) {
                    renderStats(data);
                }
            })
            .catch(err => console.error('Error fetching stats:', err));
    }

    function renderStats(stats) {
        const totalSizeMbElement = document.getElementById('totalSizeMb');
        const usageFillElement = document.getElementById('usageFill');
        const ratioElement = document.getElementById('ratio');
        const fileCountElement = document.getElementById('fileCount');
        const hitsElement = document.getElementById('hits');
        const missesElement = document.getElementById('misses');

        totalSizeMbElement.textContent = stats.totalSizeMb.toFixed(2) + ' MB';
        ratioElement.textContent = (stats.ratio * 100).toFixed(1) + '%';
        fileCountElement.textContent = stats.fileCount;
        hitsElement.textContent = stats.hits;
        missesElement.textContent = stats.misses;

        // Update usage bar
        const usagePercent = (stats.totalSizeMb / stats.maxSizeMb) * 100;
        usageFillElement.style.width = Math.min(usagePercent, 100) + '%';
    }

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', initializeForm);
})();