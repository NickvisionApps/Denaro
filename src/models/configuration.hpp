#pragma once

#include <locale>
#include <string>

namespace NickvisionMoney::Models
{
	/**
	 * Themes for the application
	 */
	enum class Theme
	{
		System = 0,
		Light,
		Dark
	};

	/**
	 * A model for the settings of the application
	 */
	class Configuration
	{
	public:
		/**
		 * Constructs a Configuration (loading the configuraton from disk)
		 */
		Configuration();
		/**
		 * Gets the user's locale
		 *
		 * @returns The user's theme
		 */
		const std::locale& getLocale() const;
		/**
		 * Gets the requested theme
		 *
		 * @returns The requested theme
		 */
		Theme getTheme() const;
		/**
		 * Sets the requested theme
		 *
		 * @param theme The new theme
		 */
		void setTheme(Theme theme);
		/**
		 * Gets the currency symbol to use when displaying monetary values
		 *
		 * @returns The currency symbol to use when displaying monetary values
		 */
		const std::string& getCurrencySymbol() const;
		/**
		 * Sets the currency symbol to use when displaying monetary values
		 *
		 * @param currencySymbol The new currency symbol to use when displaying monetary values
		 */
		void setCurrencySymbol(const std::string& currencySymbol);
		/**
		 * Gets whether or not to display the currency symbol on the right of a monetary value
		 *
		 * @returns True to display currency symbol on the right, else false
		 */
		bool getDisplayCurrencySymbolOnRight() const;
		/**
		 * Sets whether or not to display the currency symbol on the right of a monetary value
		 *
		 * @param displayCurrencySymbolOnRight True to display currency symbol on the right, else false
		 */
		void setDisplayCurrencySymbolOnRight(bool displayCurrencySymbolOnRight);
		/**
		 * Saves the configuration to disk
		 */
		void save() const;

	private:
		std::string m_configDir;
		std::locale m_locale;
		Theme m_theme;
		std::string m_currencySymbol;
		bool m_displayCurrencySymbolOnRight;
	};
}