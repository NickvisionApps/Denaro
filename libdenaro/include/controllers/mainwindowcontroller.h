#if (defined(_WIN32) && !defined(_CRT_SECURE_NO_WARNINGS))
#define _CRT_SECURE_NO_WARNINGS
#endif

#ifndef MAINWINDOWCONTROLLER_H
#define MAINWINDOWCONTROLLER_H

#include <filesystem>
#include <memory>
#include <string>
#include <vector>
#include <unordered_map>
#include <libnick/app/appinfo.h>
#include <libnick/events/event.h>
#include <libnick/events/parameventargs.h>
#include <libnick/notifications/notificationsenteventargs.h>
#include <libnick/notifications/shellnotificationsenteventargs.h>
#include <libnick/taskbar/taskbaritem.h>
#include <libnick/update/updater.h>
#include "controllers/accountviewcontroller.h"
#include "controllers/dashboardviewcontroller.h"
#include "controllers/newaccountdialogcontroller.h"
#include "controllers/preferencesviewcontroller.h"
#include "models/accounttype.h"
#include "models/color.h"
#include "models/recentaccount.h"
#include "models/theme.h"

namespace Nickvision::Money::Shared::Controllers
{
    /**
     * @brief A controller for a MainWindow.
     */
    class MainWindowController
    {
    public:
        /**
         * @brief Constructs a MainWindowController.
         * @param args A list of argument strings for the application
         */
        MainWindowController();
        /**
         * @brief Gets the AppInfo object for the application
         * @return The current AppInfo object
         */
        Nickvision::App::AppInfo& getAppInfo() const;
        /**
         * @brief Gets whether or not the specified version is a development (preview) version.
         * @return True for preview version, else false
         */
        bool isDevVersion() const;
        /**
         * @brief Gets the preferred theme for the application.
         * @return The preferred theme
         */
        Models::Theme getTheme() const;
        /**
         * @brief Gets the list of recently opened accounts.
         * @return The list of recent accounts 
         */
        std::vector<Models::RecentAccount> getRecentAccounts() const;
        /**
         * @brief Gets the Saved event for the application's configuration.
         * @return The configuration Saved event
         */
        Nickvision::Events::Event<Nickvision::Events::EventArgs>& configurationSaved();
        /**
         * @brief Gets the event for when a notification is sent.
         * @return The notification sent event
         */
        Nickvision::Events::Event<Nickvision::Notifications::NotificationSentEventArgs>& notificationSent();
        /**
         * @brief Gets the event for when a shell notification is sent.
         * @return The shell notification sent event
         */
        Nickvision::Events::Event<Nickvision::Notifications::ShellNotificationSentEventArgs>& shellNotificationSent();
        /**
         * @brief Gets the event for when an account is added.
         * @return The account added event
         */
        Nickvision::Events::Event<Nickvision::Events::ParamEventArgs<std::shared_ptr<AccountViewController>>>& accountAdded();
        /**
         * @brief Gets the debugging information for the application.
         * @param extraInformation Extra, ui-specific, information to include in the debug info statement
         * @return The application's debug information
         */
        std::string getDebugInformation(const std::string& extraInformation = "") const;
        /**
         * @brief Gets the string for greeting on the home page.
         * @return The greeting string
         */
        std::string getGreeting() const;
        /**
         * @brief Gets a PreferencesViewController.
         * @return The PreferencesViewController
         */
        std::shared_ptr<PreferencesViewController> createPreferencesViewController() const;
        /**
         * @brief Gets a NewAccountDialogController.
         * @return The NewAccountDialogController
         */
        std::shared_ptr<NewAccountDialogController> createNewAccountDialogController() const;
        /**
         * @brief Gets a DashboardViewController.
         * @return The DashboardViewController
         */
        std::shared_ptr<DashboardViewController> createDashboardViewController() const;
        /**
         * @brief Gets a AccountViewController.
         * @param path The path of the account
         * @return The AccountViewController for the account 
         */
        const std::shared_ptr<AccountViewController>& getAccountViewController(const std::filesystem::path& path) const;
        /**
         * @brief Starts the application.
         * @brief Will only have an effect on the first time called.
         */
        void startup();
        /**
         * @brief Checks for an application update and sends a notification if one is available.
         */
        void checkForUpdates();
#ifdef _WIN32
        /**
         * @brief Downloads and installs the latest application update in the background.
         * @brief Will send a notification if the update fails.
         * @brief MainWindowController::checkForUpdates() must be called before this method.
         */
        void windowsUpdate();
        /**
         * @brief Connects the main window to the taskbar interface.
         * @param hwnd The main window handle
         * @return True if connection successful, else false
         */
        bool connectTaskbar(HWND hwnd);
#elif defined(__linux__)
        /**
         * @brief Connects the application to the taskbar interface.
         * @param desktopFile The desktop file name (with the extension) of the running application
         * @return True if connection successful, else false
         */
        bool connectTaskbar(const std::string& desktopFile);
#endif
        /**
         * @brief Gets whether or not an account file requires a password to open.
         * @param path The path of the account file 
         */
        bool isAccountPasswordProtected(const std::filesystem::path& path) const;
        /**
         * @brief Creates a new account.
         * @brief This method will invoke the AccountAdded event if the account is successfully created.
         * @param newAccountDialogController The NewAccountDialogController for the new account
         */
        void newAccount(const std::shared_ptr<NewAccountDialogController>& newAccountDialogController);
        /**
         * @brief Opens an account.
         * @brief This method will invoke the AccountAdded event if the account is successfully opened.
         * @param path The path of the account to open
         * @param password The password of the account
         */
        void openAccount(std::filesystem::path path, const std::string& password);
        /**
         * @brief Removes a recent account.
         * @param recent The RecentAccount to remove 
         */
        void removeRecentAccount(const Models::RecentAccount& recent);

    private:
        bool m_started;
        std::shared_ptr<Nickvision::Update::Updater> m_updater;
        Nickvision::Taskbar::TaskbarItem m_taskbar;
        Nickvision::Events::Event<Nickvision::Notifications::NotificationSentEventArgs> m_notificationSent;
        Nickvision::Events::Event<Nickvision::Notifications::ShellNotificationSentEventArgs> m_shellNotificationSent;
        Nickvision::Events::Event<Nickvision::Events::ParamEventArgs<std::shared_ptr<AccountViewController>>> m_accountAdded;
        std::unordered_map<std::filesystem::path, std::shared_ptr<Controllers::AccountViewController>> m_accountViewControllers;
    };
}

#endif //MAINWINDOWCONTROLLER_H