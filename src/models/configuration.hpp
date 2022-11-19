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
		 * Gets the first recent account
		 *
		 * @returns The first recent account
		 */
		const std::string& getRecentAccount1() const;
		/**
		 * Gets the second recent account
		 *
		 * @returns The second recent account
		 */
		const std::string& getRecentAccount2() const;
		/**
		 * Gets the third recent account
		 *
		 * @returns The third recent account
		 */
		const std::string& getRecentAccount3() const;
		/**
		 * Adds a recent account to the list of recent accounts
		 *
		 * @param newRecentAccount The new recent account to add to the list
		 */
		void addRecentAccount(const std::string& newRecentAccount);
		/**
		 * Gets whether or not to sort transactions from first to last
		 *
		 * @returns True for first to last, false for last to first
		 */
		bool getSortFirstToLast() const;
		/**
		 * Sets whether or not to sort transactions from first to last
		 *
		 * @param sortFirstToLast True for first to last, false for last to first
		 */
		void setSortFirstToLast(bool sortFirstToLast);
		/**
		 * Saves the configuration to disk
		 */
		void save() const;

	private:
		std::string m_configDir;
		std::locale m_locale;
		Theme m_theme;
		std::string m_recentAccount1;
		std::string m_recentAccount2;
		std::string m_recentAccount3;
		bool m_sortFirstToLast;
	};
}