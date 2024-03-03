#include "MainWindow.xaml.h"
#if __has_include("MainWindow.g.cpp")
#include "MainWindow.g.cpp"
#endif
#include <format>
#include <libnick/helpers/stringhelpers.h>
#include <libnick/notifications/shellnotification.h>
#include <libnick/localization/gettext.h>
#include "SettingsPage.xaml.h"
#include "Controls/CurrencyConverterDialog.xaml.h"
#include "Controls/SettingsRow.xaml.h"
#include "Helpers/WinUI.h"

using namespace ::Nickvision;
using namespace ::Nickvision::Events;
using namespace ::Nickvision::Notifications;
using namespace ::Nickvision::Money::Shared::Controllers;
using namespace ::Nickvision::Money::Shared::Models;
using namespace winrt::Microsoft::UI;
using namespace winrt::Microsoft::UI::Dispatching;
using namespace winrt::Microsoft::UI::Input;
using namespace winrt::Microsoft::UI::Xaml;
using namespace winrt::Microsoft::UI::Xaml::Controls;
using namespace winrt::Microsoft::UI::Xaml::Controls::Primitives;
using namespace winrt::Microsoft::UI::Xaml::Input;
using namespace winrt::Microsoft::UI::Xaml::Media;
using namespace winrt::Microsoft::UI::Windowing;
using namespace winrt::Windows::ApplicationModel::DataTransfer;
using namespace winrt::Windows::Foundation::Collections;
using namespace winrt::Windows::Graphics;
using namespace winrt::Windows::Storage;
using namespace winrt::Windows::Storage::Pickers;
using namespace winrt::Windows::System;

namespace winrt::Nickvision::Money::WinUI::implementation 
{
    static std::vector<std::string> keys(const std::unordered_map<std::string, std::string>& m)
    {
        std::vector<std::string> k;
        for(std::unordered_map<std::string, std::string>::const_iterator it = m.begin(); it != m.end(); it++)
        {
            k.push_back(it->first);
        }
        return k;
    }

    MainWindow::MainWindow()
        : m_opened{ false },
        m_isActivated{ true },
        m_notificationClickToken{ 0 }
    {
        InitializeComponent();
        this->m_inner.as<::IWindowNative>()->get_WindowHandle(&m_hwnd);
        //Set TitleBar
        TitleBarTitle().Text(winrt::to_hstring(m_controller->getAppInfo().getShortName()));
        AppWindow().TitleBar().ExtendsContentIntoTitleBar(true);
        AppWindow().TitleBar().PreferredHeightOption(TitleBarHeightOption::Tall);
        AppWindow().TitleBar().ButtonBackgroundColor(Colors::Transparent());
        AppWindow().TitleBar().ButtonInactiveBackgroundColor(Colors::Transparent());
        AppWindow().Title(TitleBarTitle().Text());
        AppWindow().SetIcon(L"resources\\org.nickvision.money.ico");
        TitleBar().Loaded([&](const IInspectable sender, const RoutedEventArgs& args) { SetDragRegionForCustomTitleBar(); });
        TitleBar().SizeChanged([&](const IInspectable sender, const SizeChangedEventArgs& args) { SetDragRegionForCustomTitleBar(); });
        //Localize Strings
        TitleBarSearchBox().PlaceholderText(winrt::to_hstring(_("Search")));
        NavViewHome().Content(winrt::box_value(winrt::to_hstring(_("Home"))));
        NavViewDashboard().Content(winrt::box_value(winrt::to_hstring(_("Dashboard"))));
        NavViewAccounts().Content(winrt::box_value(winrt::to_hstring(_("Accounts"))));
        NavViewHelp().Content(winrt::box_value(winrt::to_hstring(_("Help"))));
        ToolTipService::SetToolTip(BtnCheckForUpdates(), winrt::box_value(winrt::to_hstring(_("Check for Updates"))));
        ToolTipService::SetToolTip(BtnCopyDebugInfo(), winrt::box_value(winrt::to_hstring(_("Copy Debug Information"))));
        LblChangelog().Text(winrt::to_hstring(_("Changelog")));
        BtnGitHubRepo().Content(winrt::box_value(winrt::to_hstring(_("GitHub Repo"))));
        BtnReportABug().Content(winrt::box_value(winrt::to_hstring(_("Report a Bug"))));
        BtnDiscussions().Content(winrt::box_value(winrt::to_hstring(_("Discussions"))));
        LblCredits().Text(winrt::to_hstring(_("Credits")));
        NavViewCurrencyConverter().Content(winrt::box_value(winrt::to_hstring(_("Currency Converter"))));
        NavViewSettings().Content(winrt::box_value(winrt::to_hstring(_("Settings"))));
        HomeNewAccountLabel().Text(winrt::to_hstring(_("New")));
        HomeOpenAccountLabel().Text(winrt::to_hstring(_("Open")));
    }

    void MainWindow::SetController(const std::shared_ptr<MainWindowController>& controller, ElementTheme systemTheme)
    {
        m_controller = controller;
        m_systemTheme = systemTheme;
        //Register Events
        AppWindow().Closing({ this, &MainWindow::OnClosing });
        m_controller->configurationSaved() += [&](const EventArgs& args) { OnConfigurationSaved(args); };
        m_controller->notificationSent() += [&](const NotificationSentEventArgs& args) { OnNotificationSent(args); };
        m_controller->shellNotificationSent() += [&](const ShellNotificationSentEventArgs& args) { OnShellNotificationSent(args); };
        //Localize Strings
        NavView().PaneTitle(m_controller->isDevVersion() ? winrt::to_hstring(_("PREVIEW")) : L"");
        LblAppName().Text(winrt::to_hstring(m_controller->getAppInfo().getShortName()));
        LblAppDescription().Text(winrt::to_hstring(m_controller->getAppInfo().getDescription()));
        LblAppVersion().Text(winrt::to_hstring(m_controller->getAppInfo().getVersion().toString()));
        LblAppChangelog().Text(winrt::to_hstring(m_controller->getAppInfo().getChangelog()));
        if(m_controller->getAppInfo().getTranslatorNames().size() == 1 && m_controller->getAppInfo().getTranslatorNames()[0] == "translator-credits")
        {
            LblAppCredits().Text(winrt::to_hstring(std::vformat(_("Developers:\n{}\nDesigners:\n{}\nArtists:\n{}"), std::make_format_args(StringHelpers::join(keys(m_controller->getAppInfo().getDevelopers()), "\n"), StringHelpers::join(keys(m_controller->getAppInfo().getDesigners()), "\n"), StringHelpers::join(keys(m_controller->getAppInfo().getArtists()), "\n", false)))));
        }
        else
        {
            LblAppCredits().Text(winrt::to_hstring(std::vformat(_("Developers:\n{}\nDesigners:\n{}\nArtists:\n{}\nTranslators:\n{}"), std::make_format_args(StringHelpers::join(keys(m_controller->getAppInfo().getDevelopers()), "\n"), StringHelpers::join(keys(m_controller->getAppInfo().getDesigners()), "\n"), StringHelpers::join(keys(m_controller->getAppInfo().getArtists()), "\n"), StringHelpers::join(m_controller->getAppInfo().getTranslatorNames(), "\n", false)))));
        }
        StatusPageHome().Title(winrt::to_hstring(m_controller->getGreeting()));
    }

    void MainWindow::OnLoaded(const IInspectable& sender, const RoutedEventArgs& args)
    {
        if (!m_opened)
        {
            if (!m_controller)
            {
                throw std::logic_error("MainWindow::SetController() must be called before using the window.");
            }
            m_controller->connectTaskbar(m_hwnd);
            m_controller->startup();
            NavViewHome().IsSelected(true);
            LoadRecentAccounts();
            m_opened = true;
        }
    }

    void MainWindow::OnClosing(const Microsoft::UI::Windowing::AppWindow& sender, const AppWindowClosingEventArgs& args)
    {
        //args.Cancel(true);
    }

    void MainWindow::OnActivated(const IInspectable& sender, const WindowActivatedEventArgs& args)
    {
        m_isActivated = args.WindowActivationState() != WindowActivationState::Deactivated;
        if(m_isActivated)
        {
            OnThemeChanged(MainGrid(), sender);
        }
        else
        {
            TitleBarTitle().Foreground(SolidColorBrush(Colors::Gray()));
            AppWindow().TitleBar().ButtonForegroundColor(Colors::Gray());
        }
    }

    void MainWindow::OnThemeChanged(const FrameworkElement& sender, const IInspectable& args)
    {
        switch(MainGrid().ActualTheme())
        {
        case ElementTheme::Light:
            TitleBarTitle().Foreground(SolidColorBrush(Colors::Black()));
            AppWindow().TitleBar().ButtonForegroundColor(Colors::Black());
            AppWindow().TitleBar().ButtonInactiveForegroundColor(Colors::Black());
            break;
        case ElementTheme::Dark:
            TitleBarTitle().Foreground(SolidColorBrush(Colors::White()));
            AppWindow().TitleBar().ButtonForegroundColor(Colors::White());
            AppWindow().TitleBar().ButtonInactiveForegroundColor(Colors::White());
            break;
        default:
            break;
        }
    }

    void MainWindow::OnDragOver(const IInspectable& sender, const DragEventArgs& args)
    {
        args.AcceptedOperation(DataPackageOperation::Copy | DataPackageOperation::Link);
        args.DragUIOverride().Caption(winrt::to_hstring(_("Drop here to open folder")));
        args.DragUIOverride().IsGlyphVisible(true);
        args.DragUIOverride().IsContentVisible(true);
        args.DragUIOverride().IsCaptionVisible(true);
    }

    Windows::Foundation::IAsyncAction MainWindow::OnDrop(const IInspectable& sender, const DragEventArgs& args)
    {
        if (args.DataView().Contains(StandardDataFormats::StorageItems()))
        {
            IVectorView<IStorageItem> items{ co_await args.DataView().GetStorageItemsAsync() };
            if (items.Size() > 0)
            {
                
            }
        }
    }

    void MainWindow::OnConfigurationSaved(const EventArgs& args)
    {
        switch (m_controller->getTheme())
        {
        case Theme::Light:
            MainGrid().RequestedTheme(ElementTheme::Light);
            break;
        case Theme::Dark:
            MainGrid().RequestedTheme(ElementTheme::Dark);
            break;
        default:
            MainGrid().RequestedTheme(m_systemTheme);
            break;
        }
    }

    void MainWindow::OnNotificationSent(const NotificationSentEventArgs& args)
    {
        DispatcherQueue().TryEnqueue([this, args]()
        {
            InfoBar().Message(winrt::to_hstring(args.getMessage()));
            switch(args.getSeverity())
            {
            case NotificationSeverity::Success:
                InfoBar().Severity(InfoBarSeverity::Success);
                break;
            case NotificationSeverity::Warning:
                InfoBar().Severity(InfoBarSeverity::Warning);
                break;
            case NotificationSeverity::Error:
                InfoBar().Severity(InfoBarSeverity::Error);
                break;
            default:
                InfoBar().Severity(InfoBarSeverity::Informational);
                break;
            }
            if(m_notificationClickToken)
            {
                BtnInfoBar().Click(m_notificationClickToken);
            }
            if(args.getAction() == "error")
            {
                NavViewHome().IsSelected(true);
            }
            else if(args.getAction() == "update")
            {
                BtnInfoBar().Content(winrt::box_value(winrt::to_hstring(_("Update"))));
                m_notificationClickToken = BtnInfoBar().Click({ this, &MainWindow::WindowsUpdate });
            }
            BtnInfoBar().Visibility(!args.getAction().empty() ? Visibility::Visible : Visibility::Collapsed);
            InfoBar().IsOpen(true);
        });
    }

    void MainWindow::OnShellNotificationSent(const ShellNotificationSentEventArgs& args)
    {
        ShellNotification::send(args, m_hwnd);
    }

    void MainWindow::OnNavSelectionChanged(const NavigationView& sender, const NavigationViewSelectionChangedEventArgs& args)
    {
        winrt::hstring tag{ NavView().SelectedItem().as<NavigationViewItem>().Tag().as<winrt::hstring>() };
        if(tag == L"Home")
        {
            ViewStack().CurrentPage(L"Home");
        }
        else if(tag == L"Dashboard")
        {
            ViewStack().CurrentPage(L"Dashboard");
        }
        else if(tag == L"Settings")
        {
            UserControl page{ winrt::make<SettingsPage>() };
            page.as<SettingsPage>()->SetController(m_controller->createPreferencesViewController());
            ViewStack().CurrentPage(L"Custom");
            FrameCustom().Content(winrt::box_value(page));
        }
        TitleBarSearchBox().Visibility(tag == L"Account" ? Visibility::Visible : Visibility::Collapsed);
        SetDragRegionForCustomTitleBar();
    }

    void MainWindow::OnNavViewItemTapped(const IInspectable& sender, const TappedRoutedEventArgs& args)
    {
        FlyoutBase::ShowAttachedFlyout(sender.as<FrameworkElement>());
    }

    void MainWindow::CheckForUpdates(const IInspectable& sender, const RoutedEventArgs& args)
    {
        FlyoutBase::GetAttachedFlyout(NavViewHelp().as<FrameworkElement>()).Hide();
        m_controller->checkForUpdates();
    }

    void MainWindow::WindowsUpdate(const IInspectable& sender, const RoutedEventArgs& args)
    {
        InfoBar().IsOpen(false);
        NavView().IsEnabled(false);
        TitleBarSearchBox().Visibility(Visibility::Collapsed);
        ViewStack().CurrentPage(L"Spinner");
        SetDragRegionForCustomTitleBar();
        m_controller->windowsUpdate();
    }

    void MainWindow::CopyDebugInformation(const IInspectable& sender, const RoutedEventArgs& args)
    {
        FlyoutBase::GetAttachedFlyout(NavViewHelp().as<FrameworkElement>()).Hide();
        DataPackage dataPackage;
        dataPackage.SetText(winrt::to_hstring(m_controller->getDebugInformation()));
        Clipboard::SetContent(dataPackage);
        OnNotificationSent({ _("Debug information copied to clipboard."), NotificationSeverity::Success });
    }

    Windows::Foundation::IAsyncAction MainWindow::GitHubRepo(const IInspectable& sender, const RoutedEventArgs& args)
    {
        co_await Launcher::LaunchUriAsync(Windows::Foundation::Uri{ winrt::to_hstring(m_controller->getAppInfo().getSourceRepo()) });
    }

    Windows::Foundation::IAsyncAction MainWindow::ReportABug(const IInspectable& sender, const RoutedEventArgs& args)
    {
        co_await Launcher::LaunchUriAsync(Windows::Foundation::Uri{ winrt::to_hstring(m_controller->getAppInfo().getIssueTracker()) });
    }

    Windows::Foundation::IAsyncAction MainWindow::Discussions(const IInspectable& sender, const RoutedEventArgs& args)
    {
        co_await Launcher::LaunchUriAsync(Windows::Foundation::Uri{ winrt::to_hstring(m_controller->getAppInfo().getSupportUrl()) });
    }

    Windows::Foundation::IAsyncAction MainWindow::CurrencyConverter(const IInspectable& sender, const TappedRoutedEventArgs& args)
    {
        ContentDialog dialog{ winrt::make<Controls::implementation::CurrencyConverterDialog>() };
        dialog.XamlRoot(MainGrid().XamlRoot());
        dialog.RequestedTheme(MainGrid().RequestedTheme());
        co_await dialog.ShowAsync();
    }

    void MainWindow::SetDragRegionForCustomTitleBar()
    {
        double scaleAdjustment{ TitleBar().XamlRoot().RasterizationScale() };
        RightPaddingColumn().Width({ AppWindow().TitleBar().RightInset() / scaleAdjustment });
        LeftPaddingColumn().Width({ AppWindow().TitleBar().LeftInset() / scaleAdjustment });
        GeneralTransform transformSearch{ TitleBarSearchBox().TransformToVisual(nullptr) };
        Windows::Foundation::Rect boundsSearch{ transformSearch.TransformBounds({ 0, 0, static_cast<float>(TitleBarSearchBox().ActualWidth()), static_cast<float>(TitleBarSearchBox().ActualHeight()) }) };
        RectInt32 searchBoxRect{ static_cast<int>(std::round(boundsSearch.X * scaleAdjustment)), static_cast<int>(std::round(boundsSearch.Y * scaleAdjustment)), static_cast<int>(std::round(boundsSearch.Width * scaleAdjustment)), static_cast<int>(std::round(boundsSearch.Height * scaleAdjustment)) };
        RectInt32 rectArray[1]{ searchBoxRect };
        InputNonClientPointerSource nonClientInputSrc{ InputNonClientPointerSource::GetForWindowId(AppWindow().Id()) };
        nonClientInputSrc.SetRegionRects(NonClientRegionKind::Passthrough, rectArray);
    }

    void MainWindow::LoadRecentAccounts()
    {
        std::vector<RecentAccount> recentAccounts{ m_controller->getRecentAccounts() };
        ListRecentAccounts().Children().Clear();
        if(recentAccounts.size() == 0)
        {
            UserControl row{ winrt::make<Controls::implementation::SettingsRow>() };
            row.as<Controls::implementation::SettingsRow>()->Glyph(L"\uE121");
            row.as<Controls::implementation::SettingsRow>()->Title(winrt::to_hstring(_("No Recent Accounts")));
            ListRecentAccounts().Children().Append(row);
        }
        for(const RecentAccount& recentAccount : m_controller->getRecentAccounts())
        {
            Color color{ m_controller->getAccountTypeColor(recentAccount.getType()) };
            Shapes::Rectangle rect;
            rect.Width(32);
            rect.Height(32);
            rect.Fill(SolidColorBrush{ Windows::UI::ColorHelper::FromArgb(255, color.getR(), color.getG(), color.getB()) });
            FontIcon icon;
            icon.FontFamily(WinUIHelpers::LookupAppResource<FontFamily>(L"SymbolThemeFontFamily"));
            icon.FontSize(16);
            icon.Glyph(L"\uE711");
            Button button;
            button.Content(winrt::box_value(icon));
            ToolTipService::SetToolTip(button, winrt::box_value(winrt::to_hstring(_("Remove"))));
            StackPanel stack;
            stack.Orientation(Orientation::Horizontal);
            stack.Spacing(6);
            stack.Children().Append(rect);
            stack.Children().Append(button);
            UserControl row{ winrt::make<Controls::implementation::SettingsRow>() };
            row.as<Controls::implementation::SettingsRow>()->Glyph(L"\uE8C7");
            row.as<Controls::implementation::SettingsRow>()->Title(winrt::to_hstring(recentAccount.getName()));
            row.as<Controls::implementation::SettingsRow>()->Description(winrt::to_hstring(recentAccount.getPath().string()));
            row.as<Controls::implementation::SettingsRow>()->Child(winrt::box_value(stack));
            ListRecentAccounts().Children().Append(row);
        }
    }
}
