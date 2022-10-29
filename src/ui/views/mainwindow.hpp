#pragma once

#include <memory>
#include <vector>
#include <adwaita.h>
#include "accountview.hpp"
#include "../../controllers/mainwindowcontroller.hpp"

namespace NickvisionMoney::UI::Views
{
	/**
	 * The MainWindow for the application
	 */
	class MainWindow
	{
	public:
		/**
		 * Constructs a MainWindow
		 *
		 * @param application GtkApplication*
		 * @param appInfo The AppInfo for the application
		 */
		MainWindow(GtkApplication* application, const NickvisionMoney::Controllers::MainWindowController& controller);
		/**
		 * Gets the GtkWidget* representing the MainWindow
		 *
		 * @returns The GtkWidget* representing the MainWindow
		 */
		GtkWidget* gobj();
		/**
		 * Starts the MainWindow
		 */
		void start();

	private:
		NickvisionMoney::Controllers::MainWindowController m_controller;
		GtkWidget* m_gobj{ nullptr };
		GtkWidget* m_mainBox{ nullptr };
		GtkWidget* m_headerBar{ nullptr };
		GtkWidget* m_adwTitle{ nullptr };
		GtkWidget* m_btnMenuAccount{ nullptr };
		GtkWidget* m_btnMenuHelp{ nullptr };
		GtkWidget* m_toastOverlay{ nullptr };
		GtkWidget* m_viewStack{ nullptr };
		GtkWidget* m_pageStatusNoAccounts{ nullptr };
		GtkWidget* m_boxStatusButtons{ nullptr };
		GtkWidget* m_btnNewAccount{ nullptr };
		GtkWidget* m_btnOpenAccount{ nullptr };
		GtkWidget* m_pageTabs{ nullptr };
		AdwTabView* m_tabView{ nullptr };
		AdwTabBar* m_tabBar{ nullptr };
		GSimpleAction* m_actNewAccount{ nullptr };
		GSimpleAction* m_actOpenAccount{ nullptr };
		GSimpleAction* m_actCloseAccount{ nullptr };
		GSimpleAction* m_actPreferences{ nullptr };
		GSimpleAction* m_actKeyboardShortcuts{ nullptr };
		GSimpleAction* m_actAbout{ nullptr };
		GtkDropTarget* m_dropTarget{ nullptr };
		std::vector<std::unique_ptr<AccountView>> m_accountViews;
		/**
		 * Occurs when an account is created or opened
		 *
		 * @param path The full path to the account
		 */
		void onAccountAdded(const std::string& path);
		/**
		 * Creates a new account
		 */
		void onNewAccount();
		/**
		 * Opens a new account
		 */
		void onOpenAccount();
		/**
		 * Closes an opened account
		 */
		void onCloseAccount();
		/**
		 * Displays the preferences dialog
		 */
		void onPreferences();
		/**
		 * Displays the keyboard shortcuts dialog
		 */
		void onKeyboardShortcuts();
		/**
		 * Displays the about dialog
		 */
		void onAbout();
		/**
		 * Occurs when the GtkDropTarget is triggered
		 *
		 * @param value The value from the drop
		 * @returns True to accept the drop, else false
		 */
		bool onDrop(const GValue* value);
		/**
		 * Occurs when an account page is closing
		 *
		 * @param page The page that is closing
		 */
		bool onCloseAccountPage(AdwTabPage* page);
	};
}