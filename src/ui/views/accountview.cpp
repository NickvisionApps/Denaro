#include "accountview.hpp"

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::UI::Views;

AccountView::AccountView(AdwTabView* parent, const AccountViewController& controller) : m_controller{ controller }
{
    //Main Box
    m_boxMain = gtk_box_new(GTK_ORIENTATION_VERTICAL, 0);
    //Account Total
    m_rowTotal = adw_expander_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowTotal), "Total");
    adw_expander_row_set_subtitle(ADW_EXPANDER_ROW(m_rowTotal), m_controller.getAccountTotalString().c_str());
    //Account Income
    m_lblIncome = gtk_label_new(m_controller.getAccountIncomeString().c_str());
    gtk_widget_set_valign(m_lblIncome, GTK_ALIGN_CENTER);
    gtk_style_context_add_class(gtk_widget_get_style_context(m_lblIncome), "success");
    m_rowIncome = adw_action_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowIncome), "Income");
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_rowIncome), m_lblIncome);
    adw_expander_row_add_row(ADW_EXPANDER_ROW(m_rowTotal), m_rowIncome);
    //Account Expense
    m_lblExpense = gtk_label_new(m_controller.getAccountExpenseString().c_str());
    gtk_widget_set_valign(m_lblExpense, GTK_ALIGN_CENTER);
    gtk_style_context_add_class(gtk_widget_get_style_context(m_lblExpense), "error");
    m_rowExpense = adw_action_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowExpense), "Expense");
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_rowExpense), m_lblExpense);
    adw_expander_row_add_row(ADW_EXPANDER_ROW(m_rowTotal), m_rowExpense);
    //Account Overview
    m_grpOverview = adw_preferences_group_new();
    gtk_widget_set_margin_start(m_grpOverview, 30);
    gtk_widget_set_margin_top(m_grpOverview, 10);
    gtk_widget_set_margin_end(m_grpOverview, 30);
    gtk_widget_set_margin_bottom(m_grpOverview, 10);
    adw_preferences_group_set_title(ADW_PREFERENCES_GROUP(m_grpOverview), "Overview");
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_grpOverview), m_rowTotal);
    gtk_box_append(GTK_BOX(m_boxMain), m_grpOverview);
    //Main Layout
    m_scrollMain = gtk_scrolled_window_new();
    gtk_widget_set_hexpand(m_scrollMain, true);
    gtk_widget_set_vexpand(m_scrollMain, true);
    gtk_scrolled_window_set_child(GTK_SCROLLED_WINDOW(m_scrollMain), m_boxMain);
    //Tab Page
    m_gobj = adw_tab_view_append(parent, m_scrollMain);
    adw_tab_page_set_title(m_gobj, m_controller.getAccountPath().c_str());
}

AdwTabPage* AccountView::gobj()
{
    return m_gobj;
}

void AccountView::refreshInformation()
{

}
