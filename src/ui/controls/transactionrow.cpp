#include "transactionrow.hpp"
#include <sstream>
#include <boost/date_time/gregorian/gregorian.hpp>
#include "../../helpers/moneyhelpers.hpp"
#include "../../helpers/translation.hpp"

using namespace NickvisionMoney::Helpers;
using namespace NickvisionMoney::Models;
using namespace NickvisionMoney::UI::Controls;

TransactionRow::TransactionRow(const Transaction& transaction, const std::locale& locale) : m_transaction{ transaction }, m_gobj{ adw_preferences_group_new() }
{
    //Row Settings
    m_row = adw_action_row_new();
    std::stringstream builder;
    builder << m_transaction.getDescription();
    adw_action_row_set_title_lines(ADW_ACTION_ROW(m_row), 1);
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_row), builder.str().c_str());
    builder.str("");
    builder.clear();
    builder << g_date_time_format(g_date_time_new_local(m_transaction.getDate().year(), m_transaction.getDate().month(), m_transaction.getDate().day(), 0, 0, 0.0), "%x");
    if(m_transaction.getRepeatInterval() != RepeatInterval::Never)
    {
        builder << "\n" << _("Repeat Interval: ") << m_transaction.getRepeatIntervalAsString();
    }
    adw_action_row_set_subtitle(ADW_ACTION_ROW(m_row), builder.str().c_str());
    gtk_widget_set_size_request(m_row, 300, 68);
    //Button ID
    m_btnId = gtk_button_new();
    gtk_widget_set_name(m_btnId, "btnId");
    gtk_widget_add_css_class(m_btnId, "circular");
    gtk_widget_set_valign(m_btnId, GTK_ALIGN_CENTER);
    gtk_button_set_label(GTK_BUTTON(m_btnId), std::to_string(m_transaction.getId()).c_str());
    //Button ID Color
    GdkRGBA colorBtnId;
    if(!gdk_rgba_parse(&colorBtnId, m_transaction.getRGBA().c_str()))
    {
        gdk_rgba_parse(&colorBtnId, "#3584e4");
    }
    GtkCssProvider* cssProviderBtnId{ gtk_css_provider_new() };
    std::string cssBtnId = R"(
        #btnId {
            font-size: 14px;
            color: )";
    cssBtnId += std::string(gdk_rgba_to_string(&colorBtnId));
    cssBtnId += ";\n}";
    gtk_css_provider_load_from_data(cssProviderBtnId, cssBtnId.c_str(), -1);
    gtk_style_context_add_provider(gtk_widget_get_style_context(m_btnId), GTK_STYLE_PROVIDER(cssProviderBtnId),  GTK_STYLE_PROVIDER_PRIORITY_USER);
    adw_action_row_add_prefix(ADW_ACTION_ROW(m_row), m_btnId);
    //Amount Label
    std::string amount{ MoneyHelpers::boostMoneyToLocaleString(m_transaction.getAmount(), locale) };
    amount.insert(0, m_transaction.getType() == TransactionType::Income ? "+  " : "-  ");
    m_lblAmount = gtk_label_new(amount.c_str());
    gtk_widget_add_css_class(m_lblAmount, m_transaction.getType() == TransactionType::Income ? "success" : "error");
    //Edit Button
    m_btnEdit = gtk_button_new();
    gtk_widget_set_valign(m_btnEdit, GTK_ALIGN_CENTER);
    gtk_widget_add_css_class(m_btnEdit, "flat");
    gtk_button_set_icon_name(GTK_BUTTON(m_btnEdit), "document-edit-symbolic");
    gtk_widget_set_tooltip_text(m_btnEdit, _("Edit Transaction"));
    adw_action_row_set_activatable_widget(ADW_ACTION_ROW(m_row), m_btnEdit);
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
    gtk_box_append(GTK_BOX(m_box), m_btnDelete);
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_row), m_box);
    //Group Settings
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_gobj), m_row);
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

