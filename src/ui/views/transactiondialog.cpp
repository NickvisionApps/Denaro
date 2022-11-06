#include "transactiondialog.hpp"
#include "../../helpers/stringhelpers.hpp"
#include "../../helpers/translation.hpp"

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::Helpers;
using namespace NickvisionMoney::UI::Views;

TransactionDialog::TransactionDialog(GtkWindow* parent, NickvisionMoney::Controllers::TransactionDialogController& controller) : m_controller{ controller }, m_gobj{ adw_message_dialog_new(parent, _("Transaction"), nullptr) }
{
    //Dialog Settings
    gtk_window_set_hide_on_close(GTK_WINDOW(m_gobj), true);
    adw_message_dialog_add_responses(ADW_MESSAGE_DIALOG(m_gobj), "cancel", _("Cancel"), "ok", _("OK"), nullptr);
    adw_message_dialog_set_response_appearance(ADW_MESSAGE_DIALOG(m_gobj), "ok", ADW_RESPONSE_SUGGESTED);
    adw_message_dialog_set_default_response(ADW_MESSAGE_DIALOG(m_gobj), "ok");
    adw_message_dialog_set_close_response(ADW_MESSAGE_DIALOG(m_gobj), "cancel");
    g_signal_connect(m_gobj, "response", G_CALLBACK((void (*)(AdwMessageDialog*, gchar*, gpointer))([](AdwMessageDialog*, gchar* response, gpointer data) { reinterpret_cast<TransactionDialog*>(data)->setResponse({ response }); })), this);
    //Preferences Group
    m_preferencesGroup = adw_preferences_group_new();
    //Id
    m_rowId = adw_entry_row_new();
    gtk_widget_set_size_request(m_rowId, 420, -1);
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowId), _("ID"));
    gtk_editable_set_editable(GTK_EDITABLE(m_rowId), false);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_preferencesGroup), m_rowId);
    //Date
    m_calendarDate = gtk_calendar_new();
    g_signal_connect(m_calendarDate, "day-selected", G_CALLBACK((void (*)(GtkCalendar*, gpointer))([](GtkCalendar*, gpointer data) { reinterpret_cast<TransactionDialog*>(data)->onDateChanged(); })), this);
    m_popoverDate = gtk_popover_new();
    gtk_popover_set_child(GTK_POPOVER(m_popoverDate), m_calendarDate);
    m_btnDate = gtk_menu_button_new();
    gtk_style_context_add_class(gtk_widget_get_style_context(m_btnDate), "flat");
    gtk_widget_set_valign(m_btnDate, GTK_ALIGN_CENTER);
    gtk_menu_button_set_popover(GTK_MENU_BUTTON(m_btnDate), m_popoverDate);
    gtk_menu_button_set_label(GTK_MENU_BUTTON(m_btnDate), g_date_time_format(gtk_calendar_get_date(GTK_CALENDAR(m_calendarDate)), "%Y-%m-%d"));
    m_rowDate = adw_action_row_new();
    gtk_widget_set_size_request(m_rowDate, 420, -1);
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowDate), _("Date"));
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_rowDate), m_btnDate);
    adw_action_row_set_activatable_widget(ADW_ACTION_ROW(m_rowDate), m_btnDate);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_preferencesGroup), m_rowDate);
    //Description
    m_rowDescription = adw_entry_row_new();
    gtk_widget_set_size_request(m_rowDescription, 420, -1);
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowDescription), _("Description"));
    adw_entry_row_set_activates_default(ADW_ENTRY_ROW(m_rowDescription), true);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_preferencesGroup), m_rowDescription);
    //Type
    m_rowType = adw_combo_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowType), _("Type"));
    adw_combo_row_set_model(ADW_COMBO_ROW(m_rowType), G_LIST_MODEL(gtk_string_list_new(new const char*[3]{ _("Income"), _("Expense"), nullptr })));
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_preferencesGroup), m_rowType);
    //Repeat Interval
    m_rowRepeatInterval = adw_combo_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowRepeatInterval), _("Repeat Interval"));
    adw_combo_row_set_model(ADW_COMBO_ROW(m_rowRepeatInterval), G_LIST_MODEL(gtk_string_list_new(new const char*[8]{ _("Never"), _("Daily"), _("Weekly"), _("Monthly"), _("Quarterly"), _("Yearly"), _("Biyearly"), nullptr })));
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_preferencesGroup), m_rowRepeatInterval);
    //Group
    m_rowGroup = adw_combo_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowGroup), _("Group"));
    const char** groupNames{ new const char*[m_controller.getGroupNames().size() + 1] };
    for(size_t i = 0; i < m_controller.getGroupNames().size(); i++)
    {
        groupNames[i] = m_controller.getGroupNames()[i].c_str();
    }
    groupNames[m_controller.getGroupNames().size()] = nullptr;
    adw_combo_row_set_model(ADW_COMBO_ROW(m_rowGroup), G_LIST_MODEL(gtk_string_list_new(groupNames)));
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_preferencesGroup), m_rowGroup);
    //Amount
    m_rowAmount = adw_entry_row_new();
    gtk_widget_set_size_request(m_rowAmount, 420, -1);
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowAmount), StringHelpers::format(_("Amount (%s)"), m_controller.getCurrencySymbol().c_str()).c_str());
    adw_entry_row_set_activates_default(ADW_ENTRY_ROW(m_rowAmount), true);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_preferencesGroup), m_rowAmount);
    //Layout
    adw_message_dialog_set_extra_child(ADW_MESSAGE_DIALOG(m_gobj), m_preferencesGroup);
    //Load Transaction
    gtk_editable_set_text(GTK_EDITABLE(m_rowId), m_controller.getIdAsString().c_str());
    gtk_calendar_select_day(GTK_CALENDAR(m_calendarDate), g_date_time_new_local(m_controller.getYear(), m_controller.getMonth(), m_controller.getDay(), 0, 0, 0.0));
    gtk_editable_set_text(GTK_EDITABLE(m_rowDescription), m_controller.getDescription().c_str());
    adw_combo_row_set_selected(ADW_COMBO_ROW(m_rowType), m_controller.getTypeAsInt());
    adw_combo_row_set_selected(ADW_COMBO_ROW(m_rowRepeatInterval), m_controller.getRepeatIntervalAsInt());
    adw_combo_row_set_selected(ADW_COMBO_ROW(m_rowGroup), m_controller.getGroupAsIndex());
    gtk_editable_set_text(GTK_EDITABLE(m_rowAmount), m_controller.getAmountAsString().c_str());
}

GtkWidget* TransactionDialog::gobj()
{
    return m_gobj;
}

bool TransactionDialog::run()
{
    gtk_widget_show(m_gobj);
    gtk_widget_grab_focus(m_rowDescription);
    while(gtk_widget_is_visible(m_gobj))
    {
        g_main_context_iteration(g_main_context_default(), false);
    }
    if(m_controller.getResponse() == "ok")
    {
        gtk_widget_hide(m_gobj);
        TransactionCheckStatus status{ m_controller.updateTransaction(g_date_time_format(gtk_calendar_get_date(GTK_CALENDAR(m_calendarDate)), "%Y-%m-%d"), gtk_editable_get_text(GTK_EDITABLE(m_rowDescription)), adw_combo_row_get_selected(ADW_COMBO_ROW(m_rowType)), adw_combo_row_get_selected(ADW_COMBO_ROW(m_rowRepeatInterval)), adw_combo_row_get_selected(ADW_COMBO_ROW(m_rowGroup)), gtk_editable_get_text(GTK_EDITABLE(m_rowAmount))) };
        //Invalid Transaction
        if(status != TransactionCheckStatus::Valid)
        {
            //Reset UI
            gtk_style_context_remove_class(gtk_widget_get_style_context(m_rowDescription), "error");
            adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowDescription), _("Description"));
            gtk_style_context_remove_class(gtk_widget_get_style_context(m_rowAmount), "error");
            adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowAmount), StringHelpers::format(_("Amount (%s)"), m_controller.getCurrencySymbol().c_str()).c_str());
            //Mark Error
            if(status == TransactionCheckStatus::EmptyDescription)
            {
                gtk_style_context_add_class(gtk_widget_get_style_context(m_rowDescription), "error");
                adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowDescription), _("Description (Empty)"));
            }
            else if(status == TransactionCheckStatus::EmptyAmount)
            {
                gtk_style_context_add_class(gtk_widget_get_style_context(m_rowAmount), "error");
                adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowAmount), _("Amount (Empty)"));
            }
            else if(status == TransactionCheckStatus::InvalidAmount)
            {
                gtk_style_context_add_class(gtk_widget_get_style_context(m_rowAmount), "error");
                adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowAmount), _("Amount (Invalid)"));
            }
            return run();
        }
    }
    gtk_window_destroy(GTK_WINDOW(m_gobj));
    return m_controller.getResponse() == "ok";
}

void TransactionDialog::setResponse(const std::string& response)
{
    m_controller.setResponse(response);
}

void TransactionDialog::onDateChanged()
{
    gtk_menu_button_set_label(GTK_MENU_BUTTON(m_btnDate), g_date_time_format(gtk_calendar_get_date(GTK_CALENDAR(m_calendarDate)), "%Y-%m-%d"));
    gtk_popover_popdown(GTK_POPOVER(m_popoverDate));
}
