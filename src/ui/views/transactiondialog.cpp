#include "transactiondialog.hpp"

using namespace NickvisionMoney::UI::Views;

TransactionDialog::TransactionDialog(GtkWindow* parent) : m_gobj{ adw_message_dialog_new(parent, "Transaction", nullptr) }
{
    //Dialog Settings
    gtk_window_set_hide_on_close(GTK_WINDOW(m_gobj), true);
    adw_message_dialog_add_responses(ADW_MESSAGE_DIALOG(m_gobj), "cancel", "Cancel", "ok", "OK", nullptr);
    adw_message_dialog_set_response_appearance(ADW_MESSAGE_DIALOG(m_gobj), "ok", ADW_RESPONSE_SUGGESTED);
    adw_message_dialog_set_default_response(ADW_MESSAGE_DIALOG(m_gobj), "cancel");
    adw_message_dialog_set_close_response(ADW_MESSAGE_DIALOG(m_gobj), "cancel");
    g_signal_connect(m_gobj, "response", G_CALLBACK((void (*)(AdwMessageDialog*, gchar*, gpointer))([](AdwMessageDialog*, gchar* response, gpointer data) { reinterpret_cast<TransactionDialog*>(data)->setResponse({ response }); })), this);
    //Preferences Group
    m_preferencesGroup = adw_preferences_group_new();
    //Layout
    adw_message_dialog_set_extra_child(ADW_MESSAGE_DIALOG(m_gobj), m_preferencesGroup);
}

GtkWidget* TransactionDialog::gobj()
{
    return m_gobj;
}

bool TransactionDialog::run()
{
    gtk_widget_show(m_gobj);
    while(gtk_widget_is_visible(m_gobj))
    {
        g_main_context_iteration(g_main_context_default(), false);
    }
    gtk_window_destroy(GTK_WINDOW(m_gobj));
    return true;
}

void TransactionDialog::setResponse(const std::string& response)
{
    //m_controller.setResponse(response);
}
