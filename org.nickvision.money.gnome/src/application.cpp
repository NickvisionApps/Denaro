#include "application.h"
#include <stdexcept>
#include <libnick/app/aura.h>

using namespace Nickvision::App;
using namespace Nickvision::Money::Shared::Controllers;
using namespace Nickvision::Money::Shared::Models;

namespace Nickvision::Money::GNOME
{
    Application::Application(int argc, char* argv[])
        : m_controller{ std::make_shared<MainWindowController>() },
        m_adw{ adw_application_new(m_controller->getAppInfo().getId().c_str(), G_APPLICATION_DEFAULT_FLAGS) },
        m_mainWindow{ nullptr }
    {
        m_args.reserve(static_cast<size_t>(argc));
        for(int i = 0; i < argc; i++)
        {
            m_args.push_back(argv[i]);
        }
        m_controller->getAppInfo().setChangelog("- Initial Release");
        std::filesystem::path resources{ Aura::getActive().getExecutableDirectory() / (m_controller->getAppInfo().getId() + ".gresource") };
        GError* resourceLoadError{ nullptr };
        GResource* resource{ g_resource_load(resources.string().c_str(), &resourceLoadError) };
        if(resourceLoadError)
        {
            throw std::runtime_error(resourceLoadError->message);
        }
        g_resources_register(resource);
        g_signal_connect(m_adw, "activate", G_CALLBACK(+[](GtkApplication* app, gpointer data){ reinterpret_cast<Application*>(data)->onActivate(app); }), this);
    }

    int Application::run()
    {
        return g_application_run(G_APPLICATION(m_adw), static_cast<int>(m_args.size()), &m_args[0]);
    }

    void Application::onActivate(GtkApplication* app)
    {
        switch (m_controller->getTheme())
        {
        case Theme::Light:
            adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_FORCE_LIGHT);
            break;
        case Theme::Dark:
            adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_FORCE_DARK);
            break;
        default:
            adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_DEFAULT);
            break;
        }
        if(!m_mainWindow)
        {
            m_mainWindow = std::make_shared<Views::MainWindow>(m_controller, app);
        }
        m_mainWindow->show();
    }
}