/**
 * Country & Currency Management System
 * Handles country selection, currency conversion, and price updates
 */

class CountryManager {
    constructor() {
        this.countriesData = null;
        this.selectedCountry = null;
        this.baseCurrency = 'BHD'; // Bahrain is the base
        this.injectModalStyles();
        this.init();
    }

    /**
     * Inject CSS styles for the country modal
     */
    injectModalStyles() {
        const styleId = 'country-manager-styles';
        
        // Check if styles already exist
        if (document.getElementById(styleId)) {
            return;
        }
        
        const styles = `
            /* Country Selection Modal Styles */
            #countrySelectionModal {
                z-index: 9999 !important;
            }
            
            #countrySelectionModal .modal-dialog {
                animation: slideInDown 0.4s ease-out;
                max-width: 1000px;
            }
            
            @keyframes slideInDown {
                from {
                    opacity: 0;
                    transform: translateY(-50px);
                }
                to {
                    opacity: 1;
                    transform: translateY(0);
                }
            }
            
            /* Enhanced backdrop styling when modal is open */
            .modal-open .modal-backdrop {
                background: rgba(28, 61, 90, 0.6);
                backdrop-filter: blur(4px);
                -webkit-backdrop-filter: blur(4px);
            }
            
            #countrySelectionModal .modal-content {
                border: none;
                border-radius: 20px;
                box-shadow: 0 8px 32px rgba(28, 61, 90, 0.25);
                overflow: hidden;
                background: rgba(255, 255, 255, 0.95);
                backdrop-filter: blur(10px);
            }
            
            #countrySelectionModal .modal-header {
                background: linear-gradient(135deg, #1C3D5A 0%, #0f2a3f 100%);
                border-bottom: none !important;
                padding: 28px 32px;
            }
            
            #countrySelectionModal .modal-title {
                color: white !important;
                font-size: 24px !important;
                font-weight: 700 !important;
                letter-spacing: 0.5px;
            }
            
            #countrySelectionModal .modal-body {
                padding: 40px 32px !important;
                background: #fff;
            }
            
            #countrySelectionModal .modal-body p {
                color: #666;
                font-size: 16px;
                line-height: 1.6;
                margin-bottom: 32px;
                text-align: center;
            }
            
            /* Country Cards Grid */
            .countries-grid {
                display: grid;
                grid-template-columns: repeat(3, 1fr);
                gap: 20px;
                margin-bottom: 24px;
            }
            
            .country-card-wrapper {
                display: flex;
                justify-content: center;
            }
            
            .country-radio {
                display: none;
            }
            
            .country-card-label {
                display: flex;
                flex-direction: column;
                align-items: center;
                justify-content: center;
                cursor: pointer;
                padding: 20px 15px;
                border: 2px solid #e0e0e0;
                border-radius: 12px;
                background: white;
                transition: all 0.3s ease;
                width: 100%;
                height: 160px;
                text-align: center;
                position: relative;
            }
            
            .country-radio:checked + .country-card-label {
                background: linear-gradient(135deg, #1C3D5A 0%, #0f2a3f 100%);
                border-color: #1C3D5A;
                color: white;
                box-shadow: 0 8px 20px rgba(28, 61, 90, 0.3);
                transform: scale(1.05);
            }
            
            .country-card-label:hover {
                border-color: #1C3D5A;
                box-shadow: 0 4px 12px rgba(28, 61, 90, 0.15);
                background-color: #f8f9fa;
            }
            
            .country-row-title {
                font-size: 14px;
                font-weight: 700;
                color: #333;
                margin-top: 8px;
                display: block;
            }
            
            .country-radio:checked + .country-card-label .country-row-title {
                color: white;
            }
            
            .country-flag {
                font-size: 32px;
                line-height: 1;
                display: block;
                margin-bottom: 8px;
            }
            
            .country-icon-container {
                display: flex;
                align-items: center;
                justify-content: center;
                width: 100%;
                height: 50px;
                margin-bottom: 8px;
                background: linear-gradient(135deg, rgba(28, 61, 90, 0.08) 0%, rgba(28, 61, 90, 0.05) 100%);
                border-radius: 8px;
                transition: all 0.3s ease;
            }
            
            .country-icon {
                max-width: 100%;
                max-height: 100%;
                width: auto;
                height: auto;
                object-fit: contain;
                filter: drop-shadow(0 2px 4px rgba(0, 0, 0, 0.1)) brightness(0.9) saturate(1.1);
                transition: filter 0.3s ease, transform 0.3s ease;
                color: #1C3D5A;
            }
            
            .country-radio:checked + .country-card-label .country-icon-container {
                background: linear-gradient(135deg, rgba(28, 61, 90, 0.15) 0%, rgba(28, 61, 90, 0.1) 100%);
            }
            
            .country-radio:checked + .country-card-label .country-icon {
                filter: drop-shadow(0 2px 8px rgba(28, 61, 90, 0.4)) brightness(1) saturate(1.3);
                transform: scale(1.15);
            }
            
            #countrySelectionModal .modal-footer {
                background: #f8f9fa;
                border-top: 1px solid #e0e0e0;
                padding: 20px 32px;
                display: flex;
                justify-content: center;
            }
            
            #confirmCountryBtn {
                background: linear-gradient(135deg, #1C3D5A 0%, #0f2a3f 100%) !important;
                border: none !important;
                padding: 12px 40px !important;
                font-weight: 600 !important;
                border-radius: 8px !important;
                transition: all 0.3s ease !important;
                text-transform: uppercase;
                font-size: 14px;
                letter-spacing: 0.5px;
                color: white;
            }
            
            #confirmCountryBtn:hover {
                box-shadow: 0 8px 24px rgba(28, 61, 90, 0.35) !important;
                transform: translateY(-2px);
            }
            
            #confirmCountryBtn:active {
                transform: translateY(0);
            }
            
            #confirmCountryBtn:disabled {
                opacity: 0.6;
                cursor: not-allowed;
            }
            
            @media (max-width: 768px) {
                .countries-grid {
                    grid-template-columns: repeat(2, 1fr);
                    gap: 12px;
                }
                
                .country-card-label {
                    padding: 15px 10px;
                    height: 140px;
                    font-size: 12px;
                }
                
                .country-flag {
                    font-size: 28px;
                }
                
                #countrySelectionModal .modal-body {
                    padding: 24px 20px !important;
                }
            }
        `;
        
        const styleElement = document.createElement('style');
        styleElement.id = styleId;
        styleElement.textContent = styles;
        document.head.appendChild(styleElement);
    }

    /**
     * Initialize the country manager
     */
    async init() {
        try {
            // Load countries data
            await this.loadCountriesData();
            
            // Check if user has selected a country
            const savedCountry = localStorage.getItem('selectedCountry');
            const countrySelectionInitiated = localStorage.getItem('countrySelectionInitiated');
            
            if (savedCountry) {
                // Use saved country
                this.setCountry(savedCountry);
                
                // Update currency display on existing pages
                if (typeof updateCurrencyDisplay === 'function') {
                    // Wait a bit for jQuery and custom.js to be ready
                    setTimeout(() => {
                        updateCurrencyDisplay();
                    }, 100);
                }
            } else if (!countrySelectionInitiated) {
                // Show country selection modal only if user has never selected a country before
                this.showCountrySelectionModal();
            }
        } catch (error) {
            console.error('Error initializing CountryManager:', error);
        }
    }

    /**
     * Load countries data from JSON file
     */
    async loadCountriesData() {
        try {
            const response = await fetch('/data/countries.json');
            this.countriesData = await response.json();
            console.log('Countries data loaded successfully', this.countriesData);
        } catch (error) {
            console.error('Error loading countries data:', error);
            throw error;
        }
    }

    /**
     * Show country selection modal
     */
    showCountrySelectionModal() {
        if (!this.countriesData) return;

        const modalHtml = this.generateCountryModalHTML();
        
        // Remove existing modal if present
        const existingModal = document.getElementById('countrySelectionModal');
        if (existingModal) {
            existingModal.remove();
        }

        // Add modal to body
        document.body.insertAdjacentHTML('beforeend', modalHtml);

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('countrySelectionModal'), {
            backdrop: 'static',
            keyboard: false
        });
        modal.show();

        // Attach event listeners
        this.attachCountrySelectionListeners();
    }

    /**
     * Generate HTML for country selection modal
     */
    generateCountryModalHTML() {
        const countriesGrid = this.countriesData.countries
            .map(country => {
                // Use icon if available, otherwise fall back to flag emoji
                const iconHtml = country.icon 
                    ? `<img src="${country.icon}" alt="${country.name}" class="country-icon" loading="lazy">` 
                    : `<span class="country-flag">${country.flag}</span>`;
                
                return `
                    <div class="country-card-wrapper">
                        <input type="radio" name="countrySelection" id="country-${country.id}" class="country-radio" value="${country.id}">
                        <label for="country-${country.id}" class="country-card-label">
                            <div class="country-icon-container">
                                ${iconHtml}
                            </div>
                            <span class="country-row-title">${country.name}</span>
                        </label>
                    </div>
                `;
            })
            .join('');

        return `
            <div class="modal fade" id="countrySelectionModal" tabindex="-1" role="dialog" 
                 aria-labelledby="countrySelectionLabel" aria-hidden="true">
                <div class="modal-dialog" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="countrySelectionLabel">
                                <i class="ion-location"></i> Select Your Country
                            </h5>
                        </div>
                        <div class="modal-body">
                            <p>Please select your country to see prices in your local currency.</p>
                            <div class="countries-grid">
                                ${countriesGrid}
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-primary" id="confirmCountryBtn">
                                Confirm Selection
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    /**
     * Attach event listeners for country selection
     */
    attachCountrySelectionListeners() {
        const countryRadios = document.querySelectorAll('.country-radio');
        const confirmBtn = document.getElementById('confirmCountryBtn');
        
        // Enable/disable confirm button based on selection
        countryRadios.forEach(radio => {
            radio.addEventListener('change', () => {
                confirmBtn.disabled = false;
            });
        });
        
        // Handle confirm button click
        confirmBtn.addEventListener('click', () => {
            const selectedRadio = document.querySelector('.country-radio:checked');
            
            if (!selectedRadio) {
                alert('Please select a country');
                return;
            }
            
            const selectedCountryId = selectedRadio.value;
            this.selectCountry(selectedCountryId);
        });
        
        // Allow Enter key to confirm selection when a card is focused
        countryRadios.forEach(radio => {
            radio.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    radio.checked = true;
                    confirmBtn.disabled = false;
                    confirmBtn.click();
                }
            });
        });
    }

    /**
     * Select a country and close modal
     */
    selectCountry(countryId) {
        this.setCountry(countryId);
        
        // Close modal
        const modalElement = document.getElementById('countrySelectionModal');
        if (modalElement) {
            const modal = bootstrap.Modal.getInstance(modalElement);
            if (modal) {
                modal.hide();
            }
            // Remove modal after hide animation
            setTimeout(() => {
                modalElement.remove();
            }, 500);
        }

        // Update currency display if custom.js function exists
        if (typeof updateCurrencyDisplay === 'function') {
            updateCurrencyDisplay();
        }
        
        // Reload or update prices
        location.reload();
    }

    /**
     * Set the selected country and save to localStorage
     */
    setCountry(countryId) {
        const country = this.countriesData.countries.find(c => c.id === countryId);
        
        if (country) {
            this.selectedCountry = country;
            localStorage.setItem('selectedCountry', countryId);
            localStorage.setItem('selectedCountryData', JSON.stringify(country));
            localStorage.setItem('countrySelectionInitiated', 'true');
            
            console.log('==== COUNTRY & CURRENCY SELECTED ====');
            console.log('Country: ' + country.name);
            console.log('Currency: ' + country.currency);
            console.log('Currency Name: ' + country.currencyName);
            console.log('Currency Symbol: ' + country.symbol);
            console.log('Exchange Rate (to BHD): ' + country.exchangeRate);
            console.log('======================================');
        }
    }

    /**
     * Get the selected country
     */
    getSelectedCountry() {
        if (!this.selectedCountry) {
            const saved = localStorage.getItem('selectedCountryData');
            if (saved) {
                this.selectedCountry = JSON.parse(saved);
            }
        }
        return this.selectedCountry;
    }

    /**
     * Get decimal places for specific currency
     */
    getDecimalPlaces(currencyCode) {
        // Currencies with 3 decimal places (Dinars and Rials)
        const threeDecimalCurrencies = ['BHD', 'KWD', 'OMR'];
        
        if (threeDecimalCurrencies.includes(currencyCode)) {
            return 3;
        }
        
        // Default to 2 decimal places
        return 2;
    }

    /**
     * Format number with thousands separator and decimal places
     */
    formatNumberWithCommas(number, decimalPlaces) {
        // Split number into integer and decimal parts
        const parts = number.toFixed(decimalPlaces).split('.');
        const integerPart = parts[0];
        const decimalPart = parts[1];

        // Add commas to integer part
        const formattedInteger = integerPart.replace(/\B(?=(\d{3})+(?!\d))/g, ',');

        // Return formatted number
        return `${formattedInteger}.${decimalPart}`;
    }

    /**
     * Convert price from BHD to selected country's currency
     */
    convertPrice(bhdPrice) {
        if (!this.selectedCountry) {
            return bhdPrice;
        }

        // Convert BHD to selected currency
        // Formula: BHD Price * Exchange Rate
        const convertedPrice = bhdPrice * this.selectedCountry.exchangeRate;
        return convertedPrice;
    }

    /**
     * Format a price that's already been converted (no conversion needed)
     */
    formatConvertedPrice(convertedPrice, includeSymbol = false) {
        const country = this.getSelectedCountry();
        
        if (!country) {
            const decimalPlaces = this.getDecimalPlaces('BHD');
            return this.formatNumberWithCommas(convertedPrice, decimalPlaces);
        }

        const decimalPlaces = this.getDecimalPlaces(country.currency);
        const formattedPrice = this.formatNumberWithCommas(convertedPrice, decimalPlaces);
        
        if (includeSymbol) {
            return `${country.symbol} ${formattedPrice}`;
        } else {
            return formattedPrice;
        }
    }

    /**
     * Format price with currency symbol
     */
    formatPrice(price, includeSymbol = true) {
        const country = this.getSelectedCountry();
        
        if (!country) {
            const decimalPlaces = this.getDecimalPlaces('BHD');
            return this.formatNumberWithCommas(price, decimalPlaces);
        }

        const convertedPrice = this.convertPrice(price);
        const decimalPlaces = this.getDecimalPlaces(country.currency);
        const formattedPrice = this.formatNumberWithCommas(convertedPrice, decimalPlaces);
        
        if (includeSymbol) {
            return `${country.symbol} ${formattedPrice}`;
        } else {
            return formattedPrice;
        }
    }

    /**
     * Change country manually (can be called from UI)
     */
    changeCountry() {
        localStorage.removeItem('selectedCountry');
        localStorage.removeItem('selectedCountryData');
        this.selectedCountry = null;
        this.showCountrySelectionModal();
    }

    /**
     * Get currency symbol
     */
    getCurrencySymbol() {
        const country = this.getSelectedCountry();
        return country ? country.symbol : 'BHD';
    }

    /**
     * Get currency code
     */
    getCurrencyCode() {
        const country = this.getSelectedCountry();
        return country ? country.currency : 'BHD';
    }
}

// Initialize globally
let countryManager;

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
        countryManager = new CountryManager();
    });
} else {
    countryManager = new CountryManager();
}
