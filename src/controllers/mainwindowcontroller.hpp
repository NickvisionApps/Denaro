#pragma once

#include <functional>
#include <string>
#include "preferencesdialogcontroller.hpp"
#include "../models/appinfo.hpp"
#include "../models/configuration.hpp"

namespace NickvisionMoney::Controllers
{
	/**
	 * A controller for the MainWindow
	 */
	class MainWindowController
	{
	public:
		/**
		 * Constructs a MainWindowController
		 *
		 * @param appInfo The AppInfo for the application (Stored as a reference)
		 * @param configuration The Configuration for the application (Stored as a reference)
		 */
		MainWindowController(NickvisionMoney::Models::AppInfo& appInfo, NickvisionMoney::Models::Configuration& configuration);
		/**
		 * Gets the AppInfo object representing the application's information
		 *
		 * @returns The AppInfo object for the application
		 */
		const NickvisionMoney::Models::AppInfo& getAppInfo() const;
		/**
		 * Gets whether or not the application version is a development version or not
		 *
		 * @returns True for development version, else false
		 */
		bool getIsDevVersion() const;
		/**
		 * Creates a PreferencesDialogController
		 *
		 * @returns A new PreferencesDialogController
		 */
		PreferencesDialogController createPreferencesDialogController() const;
		/**
		 * Registers a callback for sending a toast notification on the MainWindow
		 *
		 * @param callback A void(const std::string&) function
		 */
		void registerSendToastCallback(const std::function<void(const std::string& message)>& callback);
		/**
		 * Runs startup functions
		 */
		void startup();
		/**
		 * Updates the controller based on the configuration changes
		 */
		void onConfigurationChanged();
		/**
		 * Gets the opened folder path
		 *
		 * @returns The opened folder path or "No Folder Path" if no folder is opened
		 */
		const std::string& getFolderPath() const;
		/**
		 * Gets whether or not the folder is valid
		 *
		 * @returns True if folder is valid, else false
		 */
		bool getIsFolderValid() const;
		/**
		 * Registers a callback for when the folder is changed
		 *
		 * @param callback A void() function
		 */
		void registerFolderChangedCallback(const std::function<void()>& callback);
		/**
		 * Opens a folder with the given path
		 *
		 * @param folderPath The path to the folder to open
		 * @returns True if the folderPath is valid and the folder was opened, else false
		 */
		bool openFolder(const std::string& folderPath);
		/**
		 * Closes the folder if one is open
		 */
		void closeFolder();

	private:
		NickvisionMoney::Models::AppInfo& m_appInfo;
		NickvisionMoney::Models::Configuration& m_configuration;
		bool m_isOpened;
		bool m_isDevVersion;
		std::function<void(const std::string& message)> m_sendToastCallback;
		std::string m_folderPath;
		std::function<void()> m_folderChangedCallback;
	};
}