#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <filesystem>
#include <memory>
#include <string>
#include <unordered_map>
#include <adwaita.h>
#include "controls/currencyconverterpage.h"
#include "controllers/mainwindowcontroller.h"
#include "views/accountpage.h"

#define SET_ACCEL_FOR_ACTION(App, Action, Accel) { \
const char* accels[2] { Accel, nullptr }; \
gtk_application_set_accels_for_action(App, Action, accels); \
}

namespace Nickvision::Money::GNOME::Views
{
    /**
     * @brief The main window for the application. 
     */
    class MainWindow
    {
    public:
        /**
         * @brief Constructs a MainWindow.
         * @param controller The MainWindowController
         * @param app The GtkApplication object of the running app 
         */
        MainWindow(const std::shared_ptr<Shared::Controllers::MainWindowController>& controller, GtkApplication* app);
        /**
         * @brief Destructs a MainWindow. 
         */
        ~MainWindow();
        /**
         * @brief Gets the GObject object for the main window.
         * @return The GObject for the main window 
         */
        GObject* gobj() const;
        /**
         * @brief Shows the main window. 
         */
        void show();
        /**
         * @brief Handles when the window requests to close.
         * @return True to prevent closing, else false 
         */
        bool onCloseRequested();
        /**
         * @brief Handles when a file is dropped on the window.
         * @param value The GValue dropped on the window
         * @return True if drop was accepted, else false
         */
        bool onDrop(const GValue* value);
        /**
         * @brief Handles when a notification is sent to the window.
         * @param args Nickvision::Notifications::NotificationSentEventArgs 
         */
        void onNotificationSent(const Nickvision::Notifications::NotificationSentEventArgs& args);
        /**
         * @brief Handles when a shell notification is sent to the window.
         * @param args Nickvision::Notifications::ShellNotificationSentEventArgs
         */
        void onShellNotificationSent(const Nickvision::Notifications::ShellNotificationSentEventArgs& args);
        /**
         * @brief Quits the application. 
         */
        void quit();
        /**
         * @brief Opens the application's preferences dialog. 
         */
        void preferences();
        /**
         * @brief Opens the application's keyboard shortcut dialog.
         */
        void keyboardShortcuts();
        /**
         * @brief Opens the application's help documentation. 
         */
        void help();
        /**
         * @brief Opens the application's about dialog. 
         */
        void about();
        /**
         * @brief Handles when a navigation item is selected.
         * @param box The listNavItems box
         * @param row The selected row
         */
        void onNavItemSelected(GtkListBox* box, GtkListBoxRow* row);
        /**
         * @brief Handles when an account is added.
         * @param args Nickvision::Events::ParamEventArgs<Nickvision::Money::Shared::Controllers::AccountViewController>
         */
        void onAccountAdded(const ::Nickvision::Events::ParamEventArgs<std::shared_ptr<::Nickvision::Money::Shared::Controllers::AccountViewController>>& args);
        /**
         * @brief Opens the new account dialog for account creation. 
         */
        void newAccount();
        /**
         * @brief Prompts the user to open an account.
         */
        void openAccount();

    private:
        /**
         * @brief Loads the recent accounts list.
         */
        void loadRecentAccounts();
        /**
         * @brief Removes a recent account from the list.
         * @param path The path of the account file to remove 
         */
        void removeRecentAccount(const std::filesystem::path& path);
        /**
         * @brief Opens an account.
         * @param path The path of the account file to open 
         */
        void openAccount(const std::filesystem::path& path);
        std::shared_ptr<Shared::Controllers::MainWindowController> m_controller;
        GtkApplication* m_app;
        GtkBuilder* m_builder;
        AdwApplicationWindow* m_window;
        Controls::CurrencyConverterPage m_currencyConverterPage;
        std::unordered_map<std::filesystem::path, std::shared_ptr<AccountPage>> m_accountPages;
    };
}

#endif //MAINWINDOW_H