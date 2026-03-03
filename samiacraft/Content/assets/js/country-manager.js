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
            
            /* Enhanced backrop styling when modal is open */
            .modal-open .modal-backdrop {
                background: rgba(28, 61, 90, 0.6);
                backdrop-filter: blur(4px);
                -webkit-backdrop-filter: blur(4px);
            }
            
            #countrySelectionModal .modal-content {
                border: none;
                border-radius: 10px;
                box-shadow: 0 8px 32px rgba(28, 61, 90, 0.25);
                overflow: hidden;
            }
            
            #countrySelectionModal .modal-header {
                background: linear-gradient(135deg, #1C3D5A 0%, #0f2a3f 100%);
                border-bottom: none !important;
                padding: 24px 24px 20px 24px;
            }
            
            #countrySelectionModal .modal-title {
                color: white !important;
                font-size: 18px !important;
                font-weight: 700 !important;
                letter-spacing: 0.5px;
            }
            
            #countrySelectionModal .modal-body {
                padding: 28px 24px !important;
                background: #fff;
            }
            
            #countrySelectionModal .modal-body p {
                color: #666;
                font-size: 14px;
                line-height: 1.6;
            }
            
            #countrySelectionModal .modal-footer {
                background: #f8f9fa;
                border-top: 1px solid #e0e0e0;
                padding: 16px 24px;
            }
            
            #countrySelect {
                appearance: none;
                -webkit-appearance: none;
                -moz-appearance: none;
                background: white url("data:image/svg+xml;utf8,<svg fill='%231C3D5A' height='24' viewBox='0 0 24 24' width='24' xmlns='http://www.w3.org/2000/svg'><path d='M7 10l5 5 5-5z'/></svg>") no-repeat right 12px center;
                background-size: 20px;
                padding: 12px 45px 12px 12px;
                border: 2px solid #e0e0e0 !important;
                border-radius: 6px;
                font-size: 15px !important;
                font-weight: 500;
                color: #333;
                cursor: pointer;
                transition: all 0.3s ease;
                box-shadow: 0 2px 6px rgba(0, 0, 0, 0.08);
                width: 100%;
            }
            
            #countrySelect:hover {
                border-color: #1C3D5A !important;
                box-shadow: 0 4px 10px rgba(28, 61, 90, 0.15);
                background-color: #f8f9fa;
            }
            
            #countrySelect:focus {
                outline: none !important;
                border-color: #1C3D5A !important;
                box-shadow: 0 0 0 4px rgba(28, 61, 90, 0.1) !important;
            }
            
            #countrySelect option {
                padding: 12px 10px;
                background: white;
                color: #333;
            }
            
            #countrySelect option:checked {
                background: linear-gradient(135deg, #1C3D5A 0%, #0f2a3f 100%);
                color: white;
                font-weight: 600;
            }
            
            #confirmCountryBtn {
                background: linear-gradient(135deg, #1C3D5A 0%, #0f2a3f 100%) !important;
                border: none !important;
                padding: 10px 24px !important;
                font-weight: 600 !important;
                border-radius: 6px !important;
                transition: all 0.3s ease !important;
                text-transform: uppercase;
                font-size: 13px;
                letter-spacing: 0.5px;
            }
            
            #confirmCountryBtn:hover {
                box-shadow: 0 6px 16px rgba(28, 61, 90, 0.3) !important;
                transform: translateY(-2px);
            }
            
            #confirmCountryBtn:active {
                transform: translateY(0);
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
            } else {
                // Show country selection modal
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
        const countriesOptions = this.countriesData.countries
            .map(country => `
                <option value="${country.id}">${country.flag} ${country.name} (${country.currency})</option>
            `)
            .join('');

        return `
            <div class="modal fade" id="countrySelectionModal" tabindex="-1" role="dialog" 
                 aria-labelledby="countrySelectionLabel" aria-hidden="true">
                <div class="modal-dialog" role="document">
                    <div class="modal-content">
                        <div class="modal-header" style="border-bottom: 2px solid #1C3D5A;">
                            <h5 class="modal-title" id="countrySelectionLabel" 
                                style="color: #1C3D5A; font-weight: 700;">
                                <i class="ion-location"></i> Select Your Country
                            </h5>
                        </div>
                        <div class="modal-body p-4">
                            <p class="text-muted mb-4">
                                Please select your country to see prices in your local currency.
                            </p>
                            <div class="form-group">
                                <label for="countrySelect" class="form-label" style="font-weight: 600; margin-bottom: 10px;">
                                    Choose your country:
                                </label>
                                <select id="countrySelect" class="form-control" 
                                        style="padding: 6px; font-size: 16px; border: 1px solid #ddd; border-radius: 4px;">
                                    <option value="">-- Select a Country --</option>
                                    ${countriesOptions}
                                </select>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-primary" id="confirmCountryBtn" style="background-color: #1C3D5A; border-color: #1C3D5A;">
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
        const countrySelect = document.getElementById('countrySelect');
        const confirmBtn = document.getElementById('confirmCountryBtn');
        
        confirmBtn.addEventListener('click', () => {
            const selectedCountryId = countrySelect.value;
            
            if (!selectedCountryId) {
                alert('Please select a country');
                return;
            }
            
            this.selectCountry(selectedCountryId);
        });

        // Allow Enter key to confirm selection
        countrySelect.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                confirmBtn.click();
            }
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
