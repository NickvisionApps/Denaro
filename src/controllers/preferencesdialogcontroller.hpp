#pragma once

#include "../models/configuration.hpp"

namespace NickvisionMoney::Controllers
{
	/**
	 * A controller for PreferencesDialog
	 */
	class PreferencesDialogController
	{
	public:
		/**
		 * Constructs a PreferencesDialogController
		 *
		 * @param configuration The configuration fot the application (Stored as reference)
		 */
		PreferencesDialogController(NickvisionMoney::Models::Configuration& configuration);
		/**
		 * Gets the theme from the configuration as an int
		 *
		 * @returns The theme from the configuration as an int
		 */
		int getThemeAsInt() const;
		/**
		 * Sets the theme in the configuration
		 *
		 * @param theme The new theme as an int
		 */
		void setTheme(int theme);
		/**
		 * Gets transaction default color
		 *
		 * @returns Transaction default color (RGB string)
		 */
		const std::string& getTransactionDefaultColor() const;
		/**
		 * Sets transaction default color
		 *
		 * @param color New transaction default color (RGB string)
		 */
		void setTransactionDefaultColor(std::string color);
		/**
		 * Gets transfer default color
		 *
		 * @returns Transfer default color (RGB string)
		 */
		const std::string& getTransferDefaultColor() const;
		/**
		 * Sets transfer default color
		 *
		 * @param color New transfer default color (RGB string)
		 */
		void setTransferDefaultColor(std::string color);
		/**
		 * Saves the configuration file
		 */
		void saveConfiguration() const;

	private:
		NickvisionMoney::Models::Configuration& m_configuration;
	};
}