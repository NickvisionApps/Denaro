#include "accountview.hpp"
#include "transactiondialog.hpp"

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::Models;
using namespace NickvisionMoney::UI::Views;

AccountView::AccountView(GtkWindow* parentWindow, AdwTabView* parentTabView, const AccountViewController& controller) : m_controller{ controller }, m_parentWindow{ parentWindow }
{
    //Main Box
    m_boxMain = gtk_box_new(GTK_ORIENTATION_VERTICAL, 0);
    //Account Total
    m_rowTotal = adw_expander_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowTotal), "Total");
    adw_expander_row_set_subtitle(ADW_EXPANDER_ROW(m_rowTotal), "");
    //Account Income
    m_lblIncome = gtk_label_new("");
    gtk_widget_set_valign(m_lblIncome, GTK_ALIGN_CENTER);
    gtk_style_context_add_class(gtk_widget_get_style_context(m_lblIncome), "success");
    m_rowIncome = adw_action_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowIncome), "Income");
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_rowIncome), m_lblIncome);
    adw_expander_row_add_row(ADW_EXPANDER_ROW(m_rowTotal), m_rowIncome);
    //Account Expense
    m_lblExpense = gtk_label_new("");
    gtk_widget_set_valign(m_lblExpense, GTK_ALIGN_CENTER);
    gtk_style_context_add_class(gtk_widget_get_style_context(m_lblExpense), "error");
    m_rowExpense = adw_action_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowExpense), "Expense");
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_rowExpense), m_lblExpense);
    adw_expander_row_add_row(ADW_EXPANDER_ROW(m_rowTotal), m_rowExpense);
    //Overview Group
    m_grpOverview = adw_preferences_group_new();
    gtk_widget_set_margin_start(m_grpOverview, 30);
    gtk_widget_set_margin_top(m_grpOverview, 10);
    gtk_widget_set_margin_end(m_grpOverview, 30);
    gtk_widget_set_margin_bottom(m_grpOverview, 10);
    adw_preferences_group_set_title(ADW_PREFERENCES_GROUP(m_grpOverview), "Overview");
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_grpOverview), m_rowTotal);
    gtk_box_append(GTK_BOX(m_boxMain), m_grpOverview);
    //Button New Transaction
    m_btnNewTransaction = gtk_button_new();
    gtk_style_context_add_class(gtk_widget_get_style_context(m_btnNewTransaction), "flat");
    GtkWidget* btnNewTransactionContent{ adw_button_content_new() };
    adw_button_content_set_icon_name(ADW_BUTTON_CONTENT(btnNewTransactionContent), "list-add-symbolic");
    adw_button_content_set_label(ADW_BUTTON_CONTENT(btnNewTransactionContent), "New");
    gtk_button_set_child(GTK_BUTTON(m_btnNewTransaction), btnNewTransactionContent);
    g_signal_connect(m_btnNewTransaction, "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer))[](GtkButton*, gpointer data) { reinterpret_cast<AccountView*>(data)->onNewTransaction(); }), this);
    //Transactions Group
    m_grpTransactions = adw_preferences_group_new();
    gtk_widget_set_margin_start(m_grpTransactions, 30);
    gtk_widget_set_margin_top(m_grpTransactions, 10);
    gtk_widget_set_margin_end(m_grpTransactions, 30);
    gtk_widget_set_margin_bottom(m_grpTransactions, 10);
    adw_preferences_group_set_title(ADW_PREFERENCES_GROUP(m_grpTransactions), "Transactions");
    adw_preferences_group_set_header_suffix(ADW_PREFERENCES_GROUP(m_grpTransactions), m_btnNewTransaction);
    gtk_box_append(GTK_BOX(m_boxMain), m_grpTransactions);
    //Main Layout
    m_scrollMain = gtk_scrolled_window_new();
    gtk_widget_set_hexpand(m_scrollMain, true);
    gtk_widget_set_vexpand(m_scrollMain, true);
    gtk_scrolled_window_set_child(GTK_SCROLLED_WINDOW(m_scrollMain), m_boxMain);
    //Tab Page
    m_gobj = adw_tab_view_append(parentTabView, m_scrollMain);
    adw_tab_page_set_title(m_gobj, m_controller.getAccountPath().c_str());
    //Account Info Changed Callback
    m_controller.registerAccountInfoChangedCallback([&]() { onAccountInfoChanged(); });
    //Information
    onAccountInfoChanged();
}

AdwTabPage* AccountView::gobj()
{
    return m_gobj;
}

void AccountView::onAccountInfoChanged()
{
    //Overview
    adw_expander_row_set_subtitle(ADW_EXPANDER_ROW(m_rowTotal), m_controller.getAccountTotalString().c_str());
    gtk_label_set_label(GTK_LABEL(m_lblIncome), m_controller.getAccountIncomeString().c_str());
    gtk_label_set_label(GTK_LABEL(m_lblExpense), m_controller.getAccountExpenseString().c_str());
    //Transactions
    for(GtkWidget* transactionRow : m_transactionRows)
    {
        adw_preferences_group_remove(ADW_PREFERENCES_GROUP(m_grpTransactions), transactionRow);
    }
    m_transactionRows.clear();
    for(const std::pair<const unsigned int, Transaction>& pair : m_controller.getTransactions())
    {
        GtkWidget* row{ adw_action_row_new() };
        adw_preferences_row_set_title(ADW_PREFERENCES_ROW(row), pair.second.getDescription().c_str());
        adw_action_row_set_subtitle(ADW_ACTION_ROW(row), std::to_string(pair.second.getId()).c_str());
        adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_grpTransactions), row);
        m_transactionRows.push_back(row);
    }
}

void AccountView::onNewTransaction()
{
    TransactionDialogController controller{ m_controller.createTransactionDialogController() };
    TransactionDialog dialog{ m_parentWindow, controller };
    if(dialog.run())
    {
        m_controller.addTransaction(controller.getTransaction());
    }
}
