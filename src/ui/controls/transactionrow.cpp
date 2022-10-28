#include "transactionrow.hpp"
#include <sstream>
#include <boost/date_time/gregorian/gregorian.hpp>

using namespace NickvisionMoney::Models;
using namespace NickvisionMoney::UI::Controls;

TransactionRow::TransactionRow(const Transaction& transaction, const std::string& currencySymbol) : m_transaction{ transaction }, m_gobj{ adw_action_row_new() }
{
    //Row Settings
    std::stringstream builder;
    builder << m_transaction.getId() << " - " << m_transaction.getDescription();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_gobj), builder.str().c_str());
    builder.str("");
    builder.clear();
    builder << (m_transaction.getType() == TransactionType::Income ? "+ " : "- ") << currencySymbol << m_transaction.getAmount() << "\n";
    builder << boost::gregorian::to_iso_extended_string(m_transaction.getDate());
    adw_action_row_set_subtitle(ADW_ACTION_ROW(m_gobj), builder.str().c_str());
    //Edit Button
    m_btnEdit = gtk_button_new();
    gtk_widget_set_valign(m_btnEdit, GTK_ALIGN_CENTER);
    gtk_style_context_add_class(gtk_widget_get_style_context(m_btnEdit), "flat");
    gtk_button_set_icon_name(GTK_BUTTON(m_btnEdit), "edit-symbolic");
    gtk_widget_set_tooltip_text(m_btnEdit, "Edit Transaction");
    //Delete Button
    m_btnDelete = gtk_button_new();
    gtk_widget_set_valign(m_btnDelete, GTK_ALIGN_CENTER);
    gtk_style_context_add_class(gtk_widget_get_style_context(m_btnDelete), "flat");
    gtk_button_set_icon_name(GTK_BUTTON(m_btnDelete), "user-trash-symbolic");
    gtk_widget_set_tooltip_text(m_btnDelete, "Delete Transaction");
    //Buttons Box
    m_boxButtons = gtk_box_new(GTK_ORIENTATION_HORIZONTAL, 6);
    gtk_box_append(GTK_BOX(m_boxButtons), m_btnEdit);
    gtk_box_append(GTK_BOX(m_boxButtons), m_btnDelete);
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_gobj), m_boxButtons);
}

GtkWidget* TransactionRow::gobj()
{
    return m_gobj;
}
