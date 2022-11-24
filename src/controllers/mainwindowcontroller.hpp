#pragma once

#include <functional>
#include <memory>
#include <string>
#include <vector>
#include "accountviewcontroller.hpp"
#include "preferencesdialogcontroller.hpp"
#include "../models/appinfo.hpp"
#include "../models/configuration.hpp"
#include "../models/transfer.hpp"

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
		 * Gets the welcome message for the start screen
		 *
		 * @returns The welcomemessage
		 */
		std::string getWelcomeMessage() const;
		/**
		 * Gets a list of the recent accounts
		 *
		 * @returns The list of recent accounts
		 */
		std::vector<std::string> getRecentAccounts() const;
		/**
		 * Registers a callback for adding an account to the UI
		 *
		 * @param callback A void() function
		 */
		void registerAccountAddedCallback(const std::function<void()>& callback);
		/**
		 * Gets the number of accounts opened
		 *
		 * @returns The number of accounts opened
		 */
		int getNumberOfOpenAccounts() const;
		/**
		 * Gets the path of the first opened account
		 *
		 * @returns The path of the first opened account
		 */
		std::string getFirstOpenAccountPath() const;
		/**
		 * Gets whether or not an account is opened
		 *
		 * @param path The path to the account
		 * @returns True for opened, else false
		 */
		bool isAccountOpened(const std::string& path) const;
		/**
		 * Creates an AccountViewController for the latest account
		 *
		 * @returns A new AccountViewController
		 */
		AccountViewController createAccountViewControllerForLatestAccount();
		/**
		 * Adds an account to the list of opened accounts
		 *
		 * @param path The path of the account to add
		 */
		void addAccount(std::string& path);
		/**
		 * Closes the account with the provided index
		 *
		 * @param index The index of the account
		 */
		void closeAccount(int index);

	private:
		NickvisionMoney::Models::AppInfo& m_appInfo;
		NickvisionMoney::Models::Configuration& m_configuration;
		bool m_isOpened;
		bool m_isDevVersion;
		std::function<void(const std::string& message)> m_sendToastCallback;
		std::function<void()> m_accountAddedCallback;
		std::vector<std::string> m_openAccounts;
		/**
		 * Sends a transfer to an account to receive
		 *
		 * @param transfer The transfer information
		 */
		void receiveTransfer(const NickvisionMoney::Models::Transfer& transfer);
	};
}