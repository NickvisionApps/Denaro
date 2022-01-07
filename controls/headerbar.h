#ifndef HEADERBAR_H
#define HEADERBAR_H

#include <memory>
#include <gtkmm.h>

namespace NickvisionMoney::Controls
{
    class HeaderBar : public Gtk::HeaderBar
    {
    public:
        HeaderBar();
        const std::shared_ptr<Gio::SimpleAction>& getActionNewAccount() const;
        const std::shared_ptr<Gio::SimpleAction>& getActionOpenAccount() const;
        const std::shared_ptr<Gio::SimpleAction>& getActionCloseAccount() const;
        Gtk::Button& getBtnNewTransaction();
        Gtk::Button& getBtnEditTransaction();
        Gtk::Popover& getPopDeleteTransaction();
        Gtk::Button& getBtnDTDelete();
        Gtk::MenuButton& getBtnDeleteTransaction();
        Gtk::Button& getBtnBackupAccount();
        Gtk::Button& getBtnRestoreAccount();
        Gtk::Button& getBtnSettings();
        const std::shared_ptr<Gio::SimpleAction>& getActionCheckForUpdates() const;
        const std::shared_ptr<Gio::SimpleAction>& getActionGitHubRepo() const;
        const std::shared_ptr<Gio::SimpleAction>& getActionReportABug() const;
        const std::shared_ptr<Gio::SimpleAction>& getActionChangelog() const;
        const std::shared_ptr<Gio::SimpleAction>& getActionAbout() const;

    private:
        //==Account Actions==//
        std::shared_ptr<Gio::SimpleActionGroup> m_actionAccount;
        std::shared_ptr<Gio::SimpleAction> m_actionNewAccount;
        std::shared_ptr<Gio::SimpleAction> m_actionOpenAccount;
        std::shared_ptr<Gio::SimpleAction> m_actionCloseAccount;
        std::shared_ptr<Gio::Menu> m_menuAccount;
        Gtk::MenuButton m_btnAccount;
        //==New Transaction==//
        Gtk::Button m_btnNewTransaction;
        //==Edit Transaction==//
        Gtk::Button m_btnEditTransaction;
        //==Delete Transaction==//
        Gtk::Popover m_popDeleteTransaction;
        Gtk::Box m_boxDeleteTransaction;
        Gtk::Label m_lblDeleteTransaction;
        Gtk::Button m_btnDTDelete;
        Gtk::Button m_btnDTCancel;
        Gtk::Box m_boxDTBtns;
        Gtk::MenuButton m_btnDeleteTransaction;
        //==Backup Account==//
        Gtk::Button m_btnBackupAccount;
        //==Restore Account==//
        Gtk::Button m_btnRestoreAccount;
        //==Settings==//
        Gtk::Button m_btnSettings;
        //==Help==//
        std::shared_ptr<Gio::SimpleActionGroup> m_actionHelp;
        std::shared_ptr<Gio::SimpleAction> m_actionCheckForUpdates;
        std::shared_ptr<Gio::SimpleAction> m_actionGitHubRepo;
        std::shared_ptr<Gio::SimpleAction> m_actionReportABug;
        std::shared_ptr<Gio::SimpleAction> m_actionChangelog;
        std::shared_ptr<Gio::SimpleAction> m_actionAbout;
        std::shared_ptr<Gio::Menu> m_menuHelp;
        std::shared_ptr<Gio::Menu> m_menuHelpUpdate;
        std::shared_ptr<Gio::Menu> m_menuHelpLinks;
        std::shared_ptr<Gio::Menu> m_menuHelpActions;
        Gtk::MenuButton m_btnHelp;
        //==Separators==//
        Gtk::Separator m_sep1;
        Gtk::Separator m_sep2;
    };
}

#endif // HEADERBAR_H
