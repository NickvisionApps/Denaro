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
        //==Shortcuts==//
        std::shared_ptr<Gtk::ShortcutController> m_shortcutController;
        //New Account
        std::shared_ptr<Gtk::Shortcut> m_shortcutNewAccount;
        std::shared_ptr<Gtk::KeyvalTrigger> m_shortcutNewAccountTrigger;
        std::shared_ptr<Gtk::CallbackAction> m_shortcutNewAccountAction;
        //Open Account
        std::shared_ptr<Gtk::Shortcut> m_shortcutOpenAccount;
        std::shared_ptr<Gtk::KeyvalTrigger> m_shortcutOpenAccountTrigger;
        std::shared_ptr<Gtk::CallbackAction> m_shortcutOpenAccountAction;
        //Close Account
        std::shared_ptr<Gtk::Shortcut> m_shortcutCloseAccount;
        std::shared_ptr<Gtk::KeyvalTrigger> m_shortcutCloseAccountTrigger;
        std::shared_ptr<Gtk::CallbackAction> m_shortcutCloseAccountAction;
        //Backup Account
        std::shared_ptr<Gtk::Shortcut> m_shortcutBackupAccount;
        std::shared_ptr<Gtk::KeyvalTrigger> m_shortcutBackupAccountTrigger;
        std::shared_ptr<Gtk::CallbackAction> m_shortcutBackupAccountAction;
        //Restore Account
        std::shared_ptr<Gtk::Shortcut> m_shortcutRestoreAccount;
        std::shared_ptr<Gtk::KeyvalTrigger> m_shortcutRestoreAccountTrigger;
        std::shared_ptr<Gtk::CallbackAction> m_shortcutRestoreAccountAction;
        //New Transaction
        std::shared_ptr<Gtk::Shortcut> m_shortcutNewTransaction;
        std::shared_ptr<Gtk::KeyvalTrigger> m_shortcutNewTransactionTrigger;
        std::shared_ptr<Gtk::CallbackAction> m_shortcutNewTransactionAction;
        //Edit Transaction
        std::shared_ptr<Gtk::Shortcut> m_shortcutEditTransaction;
        std::shared_ptr<Gtk::KeyvalTrigger> m_shortcutEditTransactionTrigger;
        std::shared_ptr<Gtk::CallbackAction> m_shortcutEditTransactionAction;
        //Delete Transaction
        std::shared_ptr<Gtk::Shortcut> m_shortcutDeleteTransaction;
        std::shared_ptr<Gtk::KeyvalTrigger> m_shortcutDeleteTransactionTrigger;
        std::shared_ptr<Gtk::CallbackAction> m_shortcutDeleteTransactionAction;
        //About
        std::shared_ptr<Gtk::Shortcut> m_shortcutAbout;
        std::shared_ptr<Gtk::KeyvalTrigger> m_shortcutAboutTrigger;
        std::shared_ptr<Gtk::CallbackAction> m_shortcutAboutAction;
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
        void shortcuts(const Glib::VariantBase& args);
        void changelog(const Glib::VariantBase& args);
        void about(const Glib::VariantBase& args);
        void onRowDoubleClick(const Gtk::TreeModel::Path& path, Gtk::TreeViewColumn* column);
        //==Other Functions==//
        void reloadAccount();
    };
}

#endif // MAINWINDOW_H
