#include "views/preferencesdialog.h"
#include "helpers/builder.h"

using namespace Nickvision::Money::Shared::Controllers;
using namespace Nickvision::Money::Shared::Models;

namespace Nickvision::Money::GNOME::Views
{
    PreferencesDialog::PreferencesDialog(const std::shared_ptr<PreferencesViewController>& controller, GtkWindow* parent)
        : m_controller{ controller },
        m_builder{ BuilderHelpers::fromBlueprint("preferences_dialog") },
        m_dialog{ ADW_PREFERENCES_WINDOW(gtk_builder_get_object(m_builder, "root")) }
    {
        gtk_window_set_transient_for(GTK_WINDOW(m_dialog), parent);
        gtk_window_set_icon_name(GTK_WINDOW(m_dialog), m_controller->getId().c_str());
        g_signal_connect(gtk_builder_get_object(m_builder, "themeRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec* pspec, gpointer data){ reinterpret_cast<PreferencesDialog*>(data)->onThemeChanged(); }), this);
        //Load
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "themeRow")), static_cast<unsigned int>(m_controller->getTheme()));
    }

    PreferencesDialog::~PreferencesDialog()
    {
        gtk_window_destroy(GTK_WINDOW(m_dialog));
    }

    void PreferencesDialog::run()
    {
        gtk_window_present(GTK_WINDOW(m_dialog));
        while(gtk_widget_is_visible(GTK_WIDGET(m_dialog)))
        {
            g_main_context_iteration(g_main_context_default(), false);
        }
        m_controller->saveConfiguration();
    }

    void PreferencesDialog::onThemeChanged()
    {
        m_controller->setTheme(static_cast<Theme>(adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "themeRow")))));
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
    }
}