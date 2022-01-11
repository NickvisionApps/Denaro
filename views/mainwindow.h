#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <optional>
#include <memory>
#include <gtkmm.h>
#include "../models/update/updater.h"
#include "../models/account.h"
#include "../models/datatransactionscolumns.h"
#include "../controls/headerbar.h"
#include "../controls/infobar.h"

namespace NickvisionMoney::Views
{
    class MainWindow : public Gtk::ApplicationWindow
    {
    public:
        MainWindow();
        ~MainWindow();

    private:
        bool m_opened;
        NickvisionMoney::Models::Update::Updater m_updater;
        std::optional<NickvisionMoney::Models::Account> m_account;
        //==UI==//
        NickvisionMoney::Controls::HeaderBar m_headerBar;
        Gtk::Box m_mainBox;
        NickvisionMoney::Controls::InfoBar m_infoBar;
        Gtk::Grid m_gridAccountInfo;
        Gtk::Label m_lblIncome;
        Gtk::Entry m_txtIncome;
        Gtk::Label m_lblExpense;
        Gtk::Entry m_txtExpense;
        Gtk::Label m_lblTotal;
        Gtk::Entry m_txtTotal;
        Gtk::ScrolledWindow m_scrollDataTransactions;
        Gtk::TreeView m_dataTransactions;
        NickvisionMoney::Models::DataTransactionsColumns m_dataTransactionsColumns;
        std::shared_ptr<Gtk::ListStore> m_dataTransactionsModel;
        //==Slots==//
        void onShow();
        void newAccount(const Glib::VariantBase& args);
        void openAccount(const Glib::VariantBase& args);
        void closeAccount(const Glib::VariantBase& args);
        void newTransaction();
        void editTransaction();
        void deleteTransaction();
        void backupAccount();
        void restoreAccount();
        void checkForUpdates(const Glib::VariantBase& args);
        void gitHubRepo(const Glib::VariantBase& args);
        void reportABug(const Glib::VariantBase& args);
        void settings(const Glib::VariantBase& args);
        void changelog(const Glib::VariantBase& args);
        void about(const Glib::VariantBase& args);
        void onRowDoubleClick(const Gtk::TreeModel::Path& path, Gtk::TreeViewColumn* column);
        //==Other Functions==//
        void reloadAccount();
    };
}

#endif // MAINWINDOW_H
