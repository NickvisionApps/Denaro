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
        //Build UI
        gtk_window_set_transient_for(GTK_WINDOW(m_dialog), parent);
        gtk_window_set_icon_name(GTK_WINDOW(m_dialog), m_controller->getId().c_str());
        gtk_color_dialog_button_set_dialog(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "transactionColorButton")), gtk_color_dialog_new());
        gtk_color_dialog_set_with_alpha(gtk_color_dialog_button_get_dialog(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "transactionColorButton"))), false);
        gtk_color_dialog_button_set_dialog(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "transferColorButton")), gtk_color_dialog_new());
        gtk_color_dialog_set_with_alpha(gtk_color_dialog_button_get_dialog(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "transferColorButton"))), false);
        gtk_color_dialog_button_set_dialog(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "groupColorButton")), gtk_color_dialog_new());
        gtk_color_dialog_set_with_alpha(gtk_color_dialog_button_get_dialog(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "groupColorButton"))), false);
        gtk_color_dialog_button_set_dialog(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "accountCheckingColorButton")), gtk_color_dialog_new());
        gtk_color_dialog_set_with_alpha(gtk_color_dialog_button_get_dialog(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "accountCheckingColorButton"))), false);
        gtk_color_dialog_button_set_dialog(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "accountSavingsColorButton")), gtk_color_dialog_new());
        gtk_color_dialog_set_with_alpha(gtk_color_dialog_button_get_dialog(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "accountSavingsColorButton"))), false);
        gtk_color_dialog_button_set_dialog(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "accountBusinessColorButton")), gtk_color_dialog_new());
        gtk_color_dialog_set_with_alpha(gtk_color_dialog_button_get_dialog(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "accountBusinessColorButton"))), false);
        //Load
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "themeRow")), static_cast<unsigned int>(m_controller->getTheme()));
        GdkRGBA color;
        gdk_rgba_parse(&color, m_controller->getTransactionDefaultColor().toRGBString(true).c_str());
        gtk_color_dialog_button_set_rgba(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "transactionColorButton")), &color);
        gdk_rgba_parse(&color, m_controller->getTransferDefaultColor().toRGBString(true).c_str());
        gtk_color_dialog_button_set_rgba(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "transferColorButton")), &color); 
        gdk_rgba_parse(&color, m_controller->getGroupDefaultColor().toRGBString(true).c_str());
        gtk_color_dialog_button_set_rgba(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "groupColorButton")), &color); 
        gdk_rgba_parse(&color, m_controller->getAccountCheckingColor().toRGBString(true).c_str());
        gtk_color_dialog_button_set_rgba(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "accountCheckingColorButton")), &color); 
        gdk_rgba_parse(&color, m_controller->getAccountSavingsColor().toRGBString(true).c_str());
        gtk_color_dialog_button_set_rgba(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "accountSavingsColorButton")), &color);
        gdk_rgba_parse(&color, m_controller->getAccountBusinessColor().toRGBString(true).c_str());
        gtk_color_dialog_button_set_rgba(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "accountBusinessColorButton")), &color);
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "insertSeparatorRow")), static_cast<unsigned int>(m_controller->getInsertSeparator()));
        //Signals
        g_signal_connect(gtk_builder_get_object(m_builder, "themeRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec* pspec, gpointer data){ reinterpret_cast<PreferencesDialog*>(data)->onThemeChanged(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "transactionColorButton"), "notify::rgba", G_CALLBACK(+[](GObject*, GParamSpec* pspec, gpointer data){ reinterpret_cast<PreferencesDialog*>(data)->applyChanges(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "transferColorButton"), "notify::rgba", G_CALLBACK(+[](GObject*, GParamSpec* pspec, gpointer data){ reinterpret_cast<PreferencesDialog*>(data)->applyChanges(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "groupColorButton"), "notify::rgba", G_CALLBACK(+[](GObject*, GParamSpec* pspec, gpointer data){ reinterpret_cast<PreferencesDialog*>(data)->applyChanges(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "accountCheckingColorButton"), "notify::rgba", G_CALLBACK(+[](GObject*, GParamSpec* pspec, gpointer data){ reinterpret_cast<PreferencesDialog*>(data)->applyChanges(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "accountSavingsColorButton"), "notify::rgba", G_CALLBACK(+[](GObject*, GParamSpec* pspec, gpointer data){ reinterpret_cast<PreferencesDialog*>(data)->applyChanges(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "accountBusinessColorButton"), "notify::rgba", G_CALLBACK(+[](GObject*, GParamSpec* pspec, gpointer data){ reinterpret_cast<PreferencesDialog*>(data)->applyChanges(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "insertSeparatorRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec* pspec, gpointer data){ reinterpret_cast<PreferencesDialog*>(data)->applyChanges(); }), this);
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

    void PreferencesDialog::applyChanges()
    {
        m_controller->setTheme(static_cast<Theme>(adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "themeRow")))));
        m_controller->setTransactionDefaultColor({ gdk_rgba_to_string(gtk_color_dialog_button_get_rgba(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "transactionColorButton")))) });
        m_controller->setTransferDefaultColor({ gdk_rgba_to_string(gtk_color_dialog_button_get_rgba(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "transferColorButton")))) });
        m_controller->setGroupDefaultColor({ gdk_rgba_to_string(gtk_color_dialog_button_get_rgba(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "groupColorButton")))) });
        m_controller->setAccountCheckingColor({ gdk_rgba_to_string(gtk_color_dialog_button_get_rgba(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "accountCheckingColorButton")))) });
        m_controller->setAccountSavingsColor({ gdk_rgba_to_string(gtk_color_dialog_button_get_rgba(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "accountSavingsColorButton")))) });
        m_controller->setAccountBusinessColor({ gdk_rgba_to_string(gtk_color_dialog_button_get_rgba(GTK_COLOR_DIALOG_BUTTON(gtk_builder_get_object(m_builder, "accountBusinessColorButton")))) });
        m_controller->setInsertSeparator(static_cast<InsertSeparatorTrigger>(adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "insertSeparatorRow")))));
    }

    void PreferencesDialog::onThemeChanged()
    {
        applyChanges();
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