#include "headerbar.h"

namespace NickvisionMoney::Controls
{
    HeaderBar::HeaderBar()
    {
        //==Open Folder==//
        m_btnOpenFolder.set_icon_name("folder-open");
        m_btnOpenFolder.set_tooltip_text("Open Folder");
        //==Settings==//
        m_btnSettings.set_icon_name("preferences-system");
        m_btnSettings.set_tooltip_text("Settings");
        //==Help==//
        m_actionHelp = Gio::SimpleActionGroup::create();
        m_actionCheckForUpdates = m_actionHelp->add_action("checkForUpdates");
        m_actionGitHubRepo = m_actionHelp->add_action("gitHubRepo");
        m_actionReportABug = m_actionHelp->add_action("reportABug");
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
        m_menuHelpActions->append("Changelog", "help.changelog");
        m_menuHelpActions->append("About", "help.about");
        m_menuHelp->append_section(m_menuHelpUpdate);
        m_menuHelp->append_section(m_menuHelpLinks);
        m_menuHelp->append_section(m_menuHelpActions);
        m_btnHelp.set_direction(Gtk::ArrowType::NONE);
        m_btnHelp.set_menu_model(m_menuHelp);
        m_btnHelp.set_tooltip_text("Help");
        //==Layout==//
        pack_start(m_btnOpenFolder);
        pack_end(m_btnHelp);
        pack_end(m_btnSettings);
    }

    Gtk::Button& HeaderBar::getBtnOpenFolder()
    {
        return m_btnOpenFolder;
    }

    Gtk::Button& HeaderBar::getBtnSettings()
    {
        return m_btnSettings;
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

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionChangelog() const
    {
        return m_actionChangelog;
    }

    const std::shared_ptr<Gio::SimpleAction>& HeaderBar::getActionAbout() const
    {
        return m_actionAbout;
    }
}
