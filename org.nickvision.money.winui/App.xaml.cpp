#include "App.xaml.h"
#include "MainWindow.xaml.h"

using namespace ::Nickvision::Money::Shared::Controllers;
using namespace ::Nickvision::Money::Shared::Models;
using namespace winrt::Microsoft::UI::Xaml;

namespace winrt::Nickvision::Money::WinUI::implementation 
{
    App::App()
        : m_controller{ std::make_shared<MainWindowController>() },
        m_mainWindow{ nullptr }
    {
        InitializeComponent();
#ifdef DEBUG
        UnhandledException([this](const IInspectable&, const UnhandledExceptionEventArgs& args)
        {
            if(IsDebuggerPresent())
            {
                winrt::hstring err{ args.Message() };
                __debugbreak();
            }
            throw;
        });
#endif
        m_controller->getAppInfo().setChangelog("- Initial Release");
        m_systemTheme = RequestedTheme() == ApplicationTheme::Light ? ElementTheme::Light : ElementTheme::Dark;
        switch (m_controller->getTheme())
        {
        case Theme::Light:
            RequestedTheme(ApplicationTheme::Light);
            break;
        case Theme::Dark:
            RequestedTheme(ApplicationTheme::Dark);
            break;
        default:
            break;
        }
    }

    void App::OnLaunched(const LaunchActivatedEventArgs& args)
    {
        if(!m_mainWindow)
        {
            m_mainWindow = winrt::make<MainWindow>();
            m_mainWindow.as<MainWindow>()->SetController(m_controller, m_systemTheme);
        }
        m_mainWindow.Activate();
    }
}
