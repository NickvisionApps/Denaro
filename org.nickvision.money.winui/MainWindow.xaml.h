#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include "includes.h"
#include <filesystem>
#include <memory>
#include <unordered_map>
#include "Controls/StatusPage.g.h"
#include "Controls/ViewStack.g.h"
#include "Controls/ViewStackPage.g.h"
#include "MainWindow.g.h"
#include "controllers/accountviewcontroller.h"
#include "controllers/mainwindowcontroller.h"

namespace winrt::Nickvision::Money::WinUI::implementation 
{
    /**
     * @brief The main window for the application. 
     */
    class MainWindow : public MainWindowT<MainWindow>
    {
    public:
        /**
         * @brief Constructs a MainWindow.
         */
        MainWindow();
        /**
         * @brief Sets the controller for the main window.
         * @param controller The MainWindowController 
         * @param systemTheme The ElementTheme of the system
         */
        void SetController(const std::shared_ptr<::Nickvision::Money::Shared::Controllers::MainWindowController>& controller, Microsoft::UI::Xaml::ElementTheme systemTheme);
        /**
         * @brief Handles when the main window is loaded.
         * @param sender IInspectable
         * @param args Microsoft::UI::Xaml::RoutedEventArgs
         */
        void OnLoaded(const IInspectable& sender, const Microsoft::UI::Xaml::RoutedEventArgs& args);
        /**
         * @brief Handles when the main window is being closed.
         * @param sender Microsoft::UI::Windowing::AppWindow
         * @param args Microsoft::UI::Windowing::AppWindowClosingEventArgs
         */
        void OnClosing(const Microsoft::UI::Windowing::AppWindow& sender, const Microsoft::UI::Windowing::AppWindowClosingEventArgs& args);
        /**
         * @brief Handles when the main window is activated.
         * @param sender IInspectable
         * @param args Microsoft::UI::Xaml::WindowActivatedEventArgs
         */
        void OnActivated(const IInspectable& sender, const Microsoft::UI::Xaml::WindowActivatedEventArgs& args);
        /**
         * @brief Handles when the main window's theme is changed.
         * @param sender Microsoft::UI::Xaml::FrameworkElement
         * @param args IInspectable
         */
        void OnThemeChanged(const Microsoft::UI::Xaml::FrameworkElement& sender, const IInspectable& args);
        /**
         * @brief Handles when the main window's receives something dragged over.
         * @param sender IInspectable
         * @param args Microsoft::UI::Xaml::DragEventArgs
         */
        void OnDragOver(const IInspectable& sender, const Microsoft::UI::Xaml::DragEventArgs& args);
        /**
         * @brief Handles when the main window's receives a drop.
         * @param sender Microsoft::UI::Xaml::FrameworkElement
         * @param args IInspectable
         */
        Windows::Foundation::IAsyncAction OnDrop(const IInspectable& sender, const Microsoft::UI::Xaml::DragEventArgs& args);
        /**
         * @brief Handles when the application's configuration is saved to disk.
         * @param args Nickvision::Events::EventArgs 
         */
        void OnConfigurationSaved(const ::Nickvision::Events::EventArgs& args);
        /**
         * @brief Handles when a notification is sent to the window.
         * @param args Nickvision::Notifications::NotificationSentEventArgs 
         */
        void OnNotificationSent(const ::Nickvision::Notifications::NotificationSentEventArgs& args);
        /**
         * @brief Handles when a shell notification is sent to the window.
         * @param args Nickvision::Notifications::ShellNotificationSentEventArgs
         */
        void OnShellNotificationSent(const ::Nickvision::Notifications::ShellNotificationSentEventArgs& args);
        /**
         * @brief Handles when a change in the window's navigation occurs.
         * @param sender Microsoft::UI::Xaml::Controls::NavigationView
         * @param args Microsoft::UI::Xaml::Controls::NavigationViewSelectionChangedEventArgs
         */
        void OnNavSelectionChanged(const Microsoft::UI::Xaml::Controls::NavigationView& sender, const Microsoft::UI::Xaml::Controls::NavigationViewSelectionChangedEventArgs& args);
        /**
         * @brief Handles when a navigation item is tapped (to display it's flyout).
         * @param sender IInspectable
         * @param args Microsoft::UI::Xaml::Input::TappedRoutedEventArgs
         */
        void OnNavViewItemTapped(const IInspectable& sender, const Microsoft::UI::Xaml::Input::TappedRoutedEventArgs& args);
        /**
         * @brief Checks for an update to the application.
         * @param sender IInspectable
         * @param args Microsoft::UI::Xaml::RoutedEventArgs
         */
        void CheckForUpdates(const IInspectable& sender, const Microsoft::UI::Xaml::RoutedEventArgs& args);
        /**
         * @brief Updates the application.
         * @param sender IInspectable
         * @param args Microsoft::UI::Xaml::RoutedEventArgs
         */
        void WindowsUpdate(const IInspectable& sender, const Microsoft::UI::Xaml::RoutedEventArgs& args);
        /**
         * @brief Copies debugging information about the application to the clipboard.
         * @param sender IInspectable
         * @param args Microsoft::UI::Xaml::RoutedEventArgs
         */
        void CopyDebugInformation(const IInspectable& sender, const Microsoft::UI::Xaml::RoutedEventArgs& args);
        /**
         * @brief Opens the application's GitHub repo.
         * @param sender IInspectable
         * @param args Microsoft::UI::Xaml::RoutedEventArgs
         */
        Windows::Foundation::IAsyncAction GitHubRepo(const IInspectable& sender, const Microsoft::UI::Xaml::RoutedEventArgs& args);
        /**
         * @brief Opens the application's issue tracker.
         * @param sender IInspectable
         * @param args Microsoft::UI::Xaml::RoutedEventArgs
         */
        Windows::Foundation::IAsyncAction ReportABug(const IInspectable& sender, const Microsoft::UI::Xaml::RoutedEventArgs& args);
        /**
         * @brief Opens the application's support page.
         * @param sender IInspectable
         * @param args Microsoft::UI::Xaml::RoutedEventArgs
         */
        Windows::Foundation::IAsyncAction Discussions(const IInspectable& sender, const Microsoft::UI::Xaml::RoutedEventArgs& args);
        /**
         * @brief Opens the currency converter dialog.
         * @param sender IInspectable
         * @param args Microsoft::UI::Xaml::Input::TappedRoutedEventArgs 
         */
        Windows::Foundation::IAsyncAction CurrencyConverter(const IInspectable& sender, const Microsoft::UI::Xaml::Input::TappedRoutedEventArgs& args);
        /**
         * @brief Handles when an account is added.
         * @param args Nickvision::Events::ParamEventArgs<Nickvision::Money::Shared::Controllers::AccountViewController>
         */
        void OnAccountAdded(const ::Nickvision::Events::ParamEventArgs<std::shared_ptr<::Nickvision::Money::Shared::Controllers::AccountViewController>>& args);
        /**
         * @brief Prompts the user to create an account.
         * @param sender IInspectable
         * @param args Microsoft::UI::Xaml::RoutedEventArgs
         */
        Windows::Foundation::IAsyncAction NewAccount(const IInspectable& sender, const Microsoft::UI::Xaml::RoutedEventArgs& args);
        /**
         * @brief Prompts the user to open an account.
         * @param sender IInspectable
         * @param args Microsoft::UI::Xaml::RoutedEventArgs
         */
        Windows::Foundation::IAsyncAction OpenAccount(const IInspectable& sender, const Microsoft::UI::Xaml::RoutedEventArgs& args);

    private:
        /**
         * @brief Establishes drag regions for the main window's title bar. 
         */
        void SetDragRegionForCustomTitleBar();
        /**
         * @brief Loads the recent accounts list.
         */
        void LoadRecentAccounts();
        /**
         * @brief Opens an account.
         * @param path The path of the account file to open 
         */
        Windows::Foundation::IAsyncAction OpenAccount(const std::filesystem::path& path);
        std::shared_ptr<::Nickvision::Money::Shared::Controllers::MainWindowController> m_controller;
        bool m_opened;
        bool m_isActivated;
        HWND m_hwnd;
        Microsoft::UI::Xaml::ElementTheme m_systemTheme;
        winrt::event_token m_notificationClickToken;
        std::unordered_map<std::filesystem::path, Microsoft::UI::Xaml::Controls::UserControl> m_accountPages;
    };
}

namespace winrt::Nickvision::Money::WinUI::factory_implementation 
{
    class MainWindow : public MainWindowT<MainWindow, implementation::MainWindow>
    {

    };
}

#endif //MAINWINDOW_H