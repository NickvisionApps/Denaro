#include "grouprow.hpp"
#include <sstream>
#include <boost/date_time/gregorian/gregorian.hpp>
#include "../../helpers/translation.hpp"

using namespace NickvisionMoney::Models;
using namespace NickvisionMoney::UI::Controls;

GroupRow::GroupRow(const Group& group, const std::string& currencySymbol, bool displayCurrencySymbolOnRight) : m_group{ group }, m_gobj{ adw_action_row_new() }
{
    //Row Settings
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_gobj), m_group.getName().c_str());
    adw_action_row_set_subtitle(ADW_ACTION_ROW(m_gobj), m_group.getDescription().c_str());
    //Amount Label
    std::stringstream builder;
    if(displayCurrencySymbolOnRight)
    {
        builder << m_group.getBalance() << currencySymbol;
    }
    else
    {
        builder << currencySymbol << m_group.getBalance();
    }
    m_lblAmount = gtk_label_new(builder.str().c_str());
    gtk_widget_add_css_class(m_lblAmount, m_group.getBalance() >= 0 ? "success" : "error");
    //Edit Button
    m_btnEdit = gtk_button_new();
    gtk_widget_set_valign(m_btnEdit, GTK_ALIGN_CENTER);
    gtk_widget_add_css_class(m_btnEdit, "flat");
    gtk_button_set_icon_name(GTK_BUTTON(m_btnEdit), "edit-symbolic");
    gtk_widget_set_tooltip_text(m_btnEdit, _("Edit Group"));
    adw_action_row_set_activatable_widget(ADW_ACTION_ROW(m_gobj), m_btnEdit);
    g_signal_connect(m_btnEdit, "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer))[](GtkButton*, gpointer data) { reinterpret_cast<GroupRow*>(data)->onEdit(); }), this);
    //Delete Button
    m_btnDelete = gtk_button_new();
    gtk_widget_set_valign(m_btnDelete, GTK_ALIGN_CENTER);
    gtk_widget_add_css_class(m_btnDelete, "flat");
    gtk_button_set_icon_name(GTK_BUTTON(m_btnDelete), "user-trash-symbolic");
    gtk_widget_set_tooltip_text(m_btnDelete, _("Delete Group"));
    g_signal_connect(m_btnDelete, "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer))[](GtkButton*, gpointer data) { reinterpret_cast<GroupRow*>(data)->onDelete(); }), this);
    //Box
    m_box = gtk_box_new(GTK_ORIENTATION_HORIZONTAL, 6);
    gtk_box_append(GTK_BOX(m_box), m_lblAmount);
    gtk_box_append(GTK_BOX(m_box), m_btnEdit);
    gtk_box_append(GTK_BOX(m_box), m_btnDelete);
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_gobj), m_box);
}

GtkWidget* GroupRow::gobj()
{
    return m_gobj;
}

void GroupRow::registerEditCallback(const std::function<void(unsigned int)>& callback)
{
    m_editCallback = callback;
}

void GroupRow::registerDeleteCallback(const std::function<void(unsigned int)>& callback)
{
    m_deleteCallback = callback;
}

void GroupRow::onEdit()
{
    m_editCallback(m_group.getId());
}

void GroupRow::onDelete()
{
    m_deleteCallback(m_group.getId());
}
