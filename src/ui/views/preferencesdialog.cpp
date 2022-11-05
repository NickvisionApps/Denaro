#include "preferencesdialog.hpp"

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::UI::Views;

PreferencesDialog::PreferencesDialog(GtkWindow* parent, const PreferencesDialogController& controller) : m_controller{ controller }, m_gobj{ adw_window_new() }
{
    //Window Settings
    gtk_window_set_transient_for(GTK_WINDOW(m_gobj), parent);
    gtk_window_set_default_size(GTK_WINDOW(m_gobj), 800, 600);
    gtk_window_set_modal(GTK_WINDOW(m_gobj), true);
    gtk_window_set_destroy_with_parent(GTK_WINDOW(m_gobj), false);
    gtk_window_set_hide_on_close(GTK_WINDOW(m_gobj), true);
    //Header Bar
    m_headerBar = adw_header_bar_new();
    adw_header_bar_set_title_widget(ADW_HEADER_BAR(m_headerBar), adw_window_title_new(_("Preferences"), nullptr));
    //User Interface Group
    m_grpUserInterface = adw_preferences_group_new();
    adw_preferences_group_set_title(ADW_PREFERENCES_GROUP(m_grpUserInterface), _("User Interface"));
    adw_preferences_group_set_description(ADW_PREFERENCES_GROUP(m_grpUserInterface), _("Customize the application's user interface."));
    //Theme Row
    m_rowTheme = adw_combo_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowTheme), _("Theme"));
    // TODO: Extract list to make it translatable
    adw_combo_row_set_model(ADW_COMBO_ROW(m_rowTheme), G_LIST_MODEL(gtk_string_list_new(new const char*[4]{ "System", "Light", "Dark", nullptr })));
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_grpUserInterface), m_rowTheme);
    g_signal_connect(m_rowTheme, "notify::selected-item", G_CALLBACK((void (*)(GObject*, GParamSpec*, gpointer))[](GObject*, GParamSpec*, gpointer data) { reinterpret_cast<PreferencesDialog*>(data)->onThemeChanged(); }), this);
    //Currency Group
    m_grpCurrency = adw_preferences_group_new();
    adw_preferences_group_set_title(ADW_PREFERENCES_GROUP(m_grpCurrency), _("Currency"));
    adw_preferences_group_set_description(ADW_PREFERENCES_GROUP(m_grpCurrency), _("Customize currency settings.\n\nA change in one of these settings will only be applied on newly opened accounts."));
    //Currency Symbol Row
    m_rowCurrencySymbol = adw_entry_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowCurrencySymbol), _("Currency Symbol"));
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_grpCurrency), m_rowCurrencySymbol);
    //Display Currency Symbol On Right Row
    m_rowDisplayCurrencySymbolOnRight = adw_action_row_new();
    m_switchDisplayCurrencySymbolOnRight = gtk_switch_new();
    gtk_widget_set_valign(m_switchDisplayCurrencySymbolOnRight, GTK_ALIGN_CENTER);
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowDisplayCurrencySymbolOnRight), _("Display Currency Symbol On Right"));
    adw_action_row_set_subtitle(ADW_ACTION_ROW(m_rowDisplayCurrencySymbolOnRight), _("If checked, the currency symbol will be displayed on the right of a monetary value."));
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_rowDisplayCurrencySymbolOnRight), m_switchDisplayCurrencySymbolOnRight);
    adw_action_row_set_activatable_widget(ADW_ACTION_ROW(m_rowDisplayCurrencySymbolOnRight), m_switchDisplayCurrencySymbolOnRight);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_grpCurrency), m_rowDisplayCurrencySymbolOnRight);
    //Page
    m_page = adw_preferences_page_new();
    adw_preferences_page_add(ADW_PREFERENCES_PAGE(m_page), ADW_PREFERENCES_GROUP(m_grpUserInterface));
    adw_preferences_page_add(ADW_PREFERENCES_PAGE(m_page), ADW_PREFERENCES_GROUP(m_grpCurrency));
    //Main Box
    m_mainBox = gtk_box_new(GTK_ORIENTATION_VERTICAL, 0);
    gtk_box_append(GTK_BOX(m_mainBox), m_headerBar);
    gtk_box_append(GTK_BOX(m_mainBox), m_page);
    adw_window_set_content(ADW_WINDOW(m_gobj), m_mainBox);
    //Load Configuration
    adw_combo_row_set_selected(ADW_COMBO_ROW(m_rowTheme), m_controller.getThemeAsInt());
    gtk_editable_set_text(GTK_EDITABLE(m_rowCurrencySymbol), m_controller.getCurrencySymbol().c_str());
    gtk_switch_set_active(GTK_SWITCH(m_switchDisplayCurrencySymbolOnRight), m_controller.getDisplayCurrencySymbolOnRight());
}

GtkWidget* PreferencesDialog::gobj()
{
    return m_gobj;
}

void PreferencesDialog::run()
{
    gtk_widget_show(m_gobj);
    while(gtk_widget_is_visible(m_gobj))
    {
        g_main_context_iteration(g_main_context_default(), false);
    }
    std::string currencySymbol{ gtk_editable_get_text(GTK_EDITABLE(m_rowCurrencySymbol)) };
    m_controller.setCurrencySymbol(currencySymbol.empty() ? "$" : currencySymbol);
    m_controller.setDisplayCurrencySymbolOnRight(gtk_switch_get_active(GTK_SWITCH(m_switchDisplayCurrencySymbolOnRight)));
    m_controller.saveConfiguration();
    gtk_window_destroy(GTK_WINDOW(m_gobj));
}

void PreferencesDialog::onThemeChanged()
{
    m_controller.setTheme(adw_combo_row_get_selected(ADW_COMBO_ROW(m_rowTheme)));
    if(m_controller.getThemeAsInt() == 0)
    {
        adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_PREFER_LIGHT);
    }
    else if(m_controller.getThemeAsInt() == 1)
    {
        adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_FORCE_LIGHT);
    }
    else if(m_controller.getThemeAsInt() == 2)
    {
        adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_FORCE_DARK);
    }
}
