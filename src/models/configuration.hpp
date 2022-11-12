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
		std::string getCurrencySymbol() const;
		/**
		 * Gets whether or not to display the currency symbol on the right of a monetary value
		 *
		 * @returns True to display currency symbol on the right, else false
		 */
		bool getDisplayCurrencySymbolOnRight() const;
		/**
		 * Saves the configuration to disk
		 */
		void save() const;

	private:
		std::string m_configDir;
		std::locale m_locale;
		Theme m_theme;
	};
}