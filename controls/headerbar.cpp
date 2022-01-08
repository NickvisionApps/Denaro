#include "headerbar.h"

namespace NickvisionMoney::Controls
{
    HeaderBar::HeaderBar()
    {
        //==Title==//
        m_boxTitle.set_orientation(Gtk::Orientation::VERTICAL);
        m_boxTitle.set_halign(Gtk::Align::CENTER);
        m_boxTitle.set_valign(Gtk::Align::CENTER);
        m_lblTitle.get_style_context()->add_class("title");
        m_lblSubtitle.get_style_context()->add_class("subtitle");
        m_boxTitle.append(m_lblTitle);
        //==Account==//
        m_actionAccount = Gio::SimpleActionGroup::create();
        m_actionNewAccount = m_actionAccount->add_action("newAccount");
        m_actionOpenAccount = m_actionAccount->add_action("openAccount");
        m_actionCloseAccount = m_actionAccount->add_action("closeAccount");
        insert_action_group("account", m_actionAccount);
        m_menuAccount = Gio::Menu::create();
        m_menuAccount->append("New Account", "account.newAccount");
        m_menuAccount->append("Open Account", "account.openAccount");
        m_menuAccount->append("Close Account", "account.closeAccount");
        m_btnAccount.set_icon_name("text-x-generic");
        m_btnAccount.set_menu_model(m_menuAccount);
        m_btnAccount.set_tooltip_text("Account Actions");
        //==New Transaction==//
        m_btnNewTransaction.set_icon_name("list-add");
        m_btnNewTransaction.set_tooltip_text("New Transaction");
        //==Edit Transaction==//
        m_btnEditTransaction.set_icon_name("edit");
        m_btnEditTransaction.set_tooltip_text("Edit Transaction");
        //==Delete Transaction==//
        m_boxDeleteTransaction.set_orientation(Gtk::Orientation::VERTICAL);
        m_lblDeleteTransaction.set_label("Are you sure you want to delete this transaction?");
        m_lblDeleteTransaction.set_margin(4);
        m_btnDTDelete.set_label("Delete");
        m_btnDTCancel.set_label("Cancel");
        m_btnDTCancel.signal_clicked().connect(sigc::mem_fun(m_popDeleteTransaction, &Gtk::Popover::popdown));
        m_boxDTBtns.set_homogeneous(true);
        m_boxDTBtns.set_spacing(6);
        m_boxDTBtns.append(m_btnDTDelete);
        m_boxDTBtns.append(m_btnDTCancel);
        m_boxDeleteTransaction.append(m_lblDeleteTransaction);
        m_boxDeleteTransaction.append(m_boxDTBtns);
        m_popDeleteTransaction.set_child(m_boxDeleteTransaction);
        m_btnDeleteTransaction.set_icon_name("edit-delete");
        m_btnDeleteTransaction.set_popover(m_popDeleteTransaction);
        m_btnDeleteTransaction.set_tooltip_text("Delete Transaction");
        //==Backup Account==//
        m_btnBackupAccount.set_icon_name("document-save");
        m_btnBackupAccount.set_tooltip_text("Backup Account");
        //==Restore Account==//
        m_btnRestoreAccount.set_icon_name("document-open");
        m_btnRestoreAccount.set_tooltip_text("Restore Account");
        //==Help==//
        m_actionHelp = Gio::SimpleActionGroup::create();
        m_actionCheckForUpdates = m_actionHelp->add_action("checkForUpdates");
        m_actionGitHubRepo = m_actionHelp->add_action("gitHubRepo");
        m_actionReportABug = m_actionHelp->add_action("reportABug");
        m_actionSettings = m_actionHelp->add_action("settings");
        m_actionChangelog = m_actionHelp->add_action("changelog");
        m_actionAbout = m_actionHelp->add_action("about");
        insert_action_group("help", m_actionHelp);
        m_menuHelp = Gio::Menu::create();
        m_menuHelpUpdate = Gio::Menu::create();
        m_menuHelpUpdate->append("Check for Updates", "help.checkForUpdates");
        m_menuHelpLinks = Gio::Menu::create();
        m_menuHelpLinks->append("GitHub Repo", "help.gitHubRepo");
        m_menuHelpLinks->append("Report a Bug", "help.reportABug");
        m_menuHelpActions = Gio::Menu::create();
        m_menuHelpActions->append("Settings", "help.settings");
        m_menuHelpActions->append("Changelog", "help.changelog");
        m_menuHelpActions->append("About Money", "help.about");
        m_menuHelp->append_section(m_menuHelpUpdate);
        m_menuHelp->append_section(m_menuHelpLinks);
        m_menuHelp->append_section(m_menuHelpActions);
        m_btnHelp.set_direction(Gtk::ArrowType::NONE);
        m_btnHelp.set_menu_model(m_menuHelp);
        m_btnHelp.set_tooltip_text("Help");
        //==Layout==//
        set_title_widget(m_boxTitle);
        pack_start(m_btnAccount);
        pack_start(m_sep1);
        pack_start(m_btnNewTransaction);
        pack_start(m_btnEditTransaction);
        pack_start(m_btnDeleteTransaction);
        pack_end(m_btnHelp);
        pack_end(m_sep2);
        pack_end(m_btnRestoreAccount);
        pack_end(m_btnBackupAccount);

    }

    void HeaderBar::setTitle(const std::string& title)
    {
        m_lblTitle.set_text(title);
    }

    void HeaderBar::setSubtitle(const std::string& subtitle)
    {
        m_boxTitle.remove(m_lblSubtitle);
        if(!subtitle.empty())
        {
            m_boxTitle.append(m_lblSubtitle);
            m_lblSubtitle.set_text(subtitle);
        }
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionNewAccount() const
    {
        return m_actionNewAccount;
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionOpenAccount() const
    {
        return m_actionOpenAccount;
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionCloseAccount() const
    {
        return m_actionCloseAccount;
    }

    Gtk::Button& HeaderBar::getBtnNewTransaction()
    {
        return m_btnNewTransaction;
    }

    Gtk::Button& HeaderBar::getBtnEditTransaction()
    {
        return m_btnEditTransaction;
    }

    Gtk::Popover& HeaderBar::getPopDeleteTransaction()
    {
        return m_popDeleteTransaction;
    }

    Gtk::Button& HeaderBar::getBtnDTDelete()
    {
        return m_btnDTDelete;
    }

    Gtk::MenuButton& HeaderBar::getBtnDeleteTransaction()
    {
        return m_btnDeleteTransaction;
    }

    Gtk::Button& HeaderBar::getBtnBackupAccount()
    {
        return m_btnBackupAccount;
    }

    Gtk::Button& HeaderBar::getBtnRestoreAccount()
    {
        return m_btnRestoreAccount;
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionCheckForUpdates() const
    {
        return m_actionCheckForUpdates;
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionGitHubRepo() const
    {
        return m_actionGitHubRepo;
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionReportABug() const
    {
        return m_actionReportABug;
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionSettings() const
    {
        return m_actionSettings;
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionChangelog() const
    {
        return m_actionChangelog;
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionAbout() const
    {
        return m_actionAbout;
    }
}
