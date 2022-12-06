#include "grouprow.hpp"
#include <boost/date_time/gregorian/gregorian.hpp>
#include "../../helpers/moneyhelpers.hpp"
#include "../../helpers/translation.hpp"

using namespace NickvisionMoney::Helpers;
using namespace NickvisionMoney::Models;
using namespace NickvisionMoney::UI::Controls;

GroupRow::GroupRow(const Group& group, const std::locale& locale, bool filterActive) : m_group{ group }, m_gobj{ adw_action_row_new() }
{
    //Row Settings
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_gobj), m_group.getName().c_str());
    adw_action_row_set_subtitle(ADW_ACTION_ROW(m_gobj), m_group.getDescription().c_str());
    //Filter Checkbox
    m_chkFilter = gtk_check_button_new();
    gtk_check_button_set_active(GTK_CHECK_BUTTON(m_chkFilter), filterActive);
    gtk_widget_add_css_class(m_chkFilter, "selection-mode");
    g_signal_connect(m_chkFilter, "toggled", G_CALLBACK((void (*)(GtkCheckButton*, gpointer))[](GtkCheckButton*, gpointer data) { reinterpret_cast<GroupRow*>(data)->onUpdateFilter(); }), this);
    adw_action_row_add_prefix(ADW_ACTION_ROW(m_gobj), m_chkFilter);
    //Amount Label
    m_lblAmount = gtk_label_new(MoneyHelpers::boostMoneyToLocaleString(m_group.getBalance(), locale).c_str());
    gtk_widget_add_css_class(m_lblAmount, m_group.getBalance() >= 0 ? "success" : "error");
    gtk_widget_set_valign(m_lblAmount, GTK_ALIGN_CENTER);
    gtk_widget_add_css_class(m_lblAmount, m_group.getBalance() >= 0 ? "money-income" : "money-expense");
    //Edit Button
    m_btnEdit = gtk_button_new();
    gtk_widget_set_valign(m_btnEdit, GTK_ALIGN_CENTER);
    gtk_widget_add_css_class(m_btnEdit, "flat");
    gtk_button_set_icon_name(GTK_BUTTON(m_btnEdit), "document-edit-symbolic");
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

void GroupRow::registerUpdateFilterCallback(const std::function<void(int, bool)>& callback)
{
    m_updateFilterCallback = callback;
}

void GroupRow::resetFilter()
{
    if(!gtk_check_button_get_active(GTK_CHECK_BUTTON(m_chkFilter)))
    {
        gtk_check_button_set_active(GTK_CHECK_BUTTON(m_chkFilter), true);
    }
}

void GroupRow::onEdit()
{
    m_editCallback(m_group.getId());
}

void GroupRow::onDelete()
{
    m_deleteCallback(m_group.getId());
}

void GroupRow::onUpdateFilter()
{
    m_updateFilterCallback(m_group.getId(), gtk_check_button_get_active(GTK_CHECK_BUTTON(m_chkFilter)));
}
