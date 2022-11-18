#include "transactionrow.hpp"
#include <sstream>
#include <boost/date_time/gregorian/gregorian.hpp>
#include "../../helpers/moneyhelpers.hpp"
#include "../../helpers/translation.hpp"

using namespace NickvisionMoney::Helpers;
using namespace NickvisionMoney::Models;
using namespace NickvisionMoney::UI::Controls;

TransactionRow::TransactionRow(const Transaction& transaction, const std::locale& locale) : m_transaction{ transaction }, m_gobj{ adw_action_row_new() }
{
    //Row Settings
    std::stringstream builder;
    builder << m_transaction.getDescription();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_gobj), builder.str().c_str());
    builder.str("");
    builder.clear();
    builder << g_date_time_format(g_date_time_new_local(m_transaction.getDate().year(), m_transaction.getDate().month(), m_transaction.getDate().day(), 0, 0, 0.0), "%x");
    if(m_transaction.getRepeatInterval() != RepeatInterval::Never)
    {
        builder << "\n" << _("Repeat Interval: ") << m_transaction.getRepeatIntervalAsString();
    }
    adw_action_row_set_subtitle(ADW_ACTION_ROW(m_gobj), builder.str().c_str());
    gtk_widget_add_css_class(m_gobj, "card");
    gtk_widget_set_size_request(m_gobj, 250, -1);
    //Button ID
    m_btnId = gtk_button_new();
    gtk_widget_add_css_class(m_btnId, "circular");
    gtk_widget_set_valign(m_btnId, GTK_ALIGN_CENTER);
    builder.str("");
    builder.clear();
    builder << m_transaction.getId();
    gtk_button_set_label(GTK_BUTTON(m_btnId), builder.str().c_str());
    adw_action_row_add_prefix(ADW_ACTION_ROW(m_gobj), m_btnId);
    //Amount Label
    m_lblAmount = gtk_label_new(MoneyHelpers::boostMoneyToLocaleString(m_transaction.getAmount(), locale).c_str());
    gtk_widget_add_css_class(m_lblAmount, m_transaction.getType() == TransactionType::Income ? "success" : "error");
    //Edit Button
    m_btnEdit = gtk_button_new();
    gtk_widget_set_valign(m_btnEdit, GTK_ALIGN_CENTER);
    gtk_widget_add_css_class(m_btnEdit, "flat");
    gtk_button_set_icon_name(GTK_BUTTON(m_btnEdit), "edit-symbolic");
    gtk_widget_set_tooltip_text(m_btnEdit, _("Edit Transaction"));
    adw_action_row_set_activatable_widget(ADW_ACTION_ROW(m_gobj), m_btnEdit);
    g_signal_connect(m_btnEdit, "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer))[](GtkButton*, gpointer data) { reinterpret_cast<TransactionRow*>(data)->onEdit(); }), this);
    //Delete Button
    m_btnDelete = gtk_button_new();
    gtk_widget_set_valign(m_btnDelete, GTK_ALIGN_CENTER);
    gtk_widget_add_css_class(m_btnDelete, "flat");
    gtk_button_set_icon_name(GTK_BUTTON(m_btnDelete), "user-trash-symbolic");
    gtk_widget_set_tooltip_text(m_btnDelete, _("Delete Transaction"));
    g_signal_connect(m_btnDelete, "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer))[](GtkButton*, gpointer data) { reinterpret_cast<TransactionRow*>(data)->onDelete(); }), this);
    //Box
    m_box = gtk_box_new(GTK_ORIENTATION_HORIZONTAL, 6);
    gtk_box_append(GTK_BOX(m_box), m_lblAmount);
    gtk_box_append(GTK_BOX(m_box), m_btnEdit);
    //gtk_box_append(GTK_BOX(m_box), m_btnDelete);
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_gobj), m_box);
}

GtkWidget* TransactionRow::gobj()
{
    return m_gobj;
}

void TransactionRow::registerEditCallback(const std::function<void(unsigned int)>& callback)
{
    m_editCallback = callback;
}

void TransactionRow::registerDeleteCallback(const std::function<void(unsigned int)>& callback)
{
    m_deleteCallback = callback;
}

void TransactionRow::onEdit()
{
    m_editCallback(m_transaction.getId());
}

void TransactionRow::onDelete()
{
    m_deleteCallback(m_transaction.getId());
}
