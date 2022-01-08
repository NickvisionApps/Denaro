#include "mainwindow.h"
#include "../models/configuration.h"
#include "../controls/progressdialog.h"
#include "settingsdialog.h"

namespace NickvisionMoney::Views
{
    using namespace NickvisionMoney::Models;
    using namespace NickvisionMoney::Controls;

    MainWindow::MainWindow() : m_opened(false), m_updater("https://raw.githubusercontent.com/nlogozzo/NickvisionMoney/main/UpdateConfig.json", { "2022.1.0" })
    {
        //==Settings==//
        set_default_size(800, 600);
        set_title("Nickvision Money");
        set_titlebar(m_headerBar);
        signal_show().connect(sigc::mem_fun(*this, &MainWindow::onShow));
        //==HeaderBar==//
        m_headerBar.getActionNewAccount()->signal_activate().connect(sigc::mem_fun(*this, &MainWindow::newAccount));
        m_headerBar.getActionOpenAccount()->signal_activate().connect(sigc::mem_fun(*this, &MainWindow::openAccount));
        m_headerBar.getActionCloseAccount()->signal_activate().connect(sigc::mem_fun(*this, &MainWindow::closeAccount));
        m_headerBar.getBtnNewTransaction().signal_clicked().connect(sigc::mem_fun(*this, &MainWindow::newTransaction));
        m_headerBar.getBtnEditTransaction().signal_clicked().connect(sigc::mem_fun(*this, &MainWindow::editTransaction));
        m_headerBar.getBtnDTDelete().signal_clicked().connect(sigc::mem_fun(*this, &MainWindow::deleteTransaction));
        m_headerBar.getBtnBackupAccount().signal_clicked().connect(sigc::mem_fun(*this, &MainWindow::backupAccount));
        m_headerBar.getBtnRestoreAccount().signal_clicked().connect(sigc::mem_fun(*this, &MainWindow::restoreAccount));
        m_headerBar.getBtnSettings().signal_clicked().connect(sigc::mem_fun(*this, &MainWindow::settings));
        m_headerBar.getActionCheckForUpdates()->signal_activate().connect(sigc::mem_fun(*this, &MainWindow::checkForUpdates));
        m_headerBar.getActionGitHubRepo()->signal_activate().connect(sigc::mem_fun(*this, &MainWindow::gitHubRepo));
        m_headerBar.getActionReportABug()->signal_activate().connect(sigc::mem_fun(*this, &MainWindow::reportABug));
        m_headerBar.getActionChangelog()->signal_activate().connect(sigc::mem_fun(*this, &MainWindow::changelog));
        m_headerBar.getActionAbout()->signal_activate().connect(sigc::mem_fun(*this, &MainWindow::about));
        m_headerBar.getActionCloseAccount()->set_enabled(false);
        m_headerBar.getBtnNewTransaction().set_sensitive(false);
        m_headerBar.getBtnEditTransaction().set_sensitive(false);
        m_headerBar.getBtnDeleteTransaction().set_sensitive(false);
        m_headerBar.getBtnBackupAccount().set_sensitive(false);
        m_headerBar.getBtnRestoreAccount().set_sensitive(false);
        //==Name Field==//
        m_lblName.set_label("Name");
        m_lblName.set_halign(Gtk::Align::START);
        m_lblName.set_margin_start(6);
        m_lblName.set_margin_top(6);
        m_txtName.set_margin(6);
        m_txtName.set_placeholder_text("Enter name here");
        //==Layout==//
        m_mainBox.set_orientation(Gtk::Orientation::VERTICAL);
        m_mainBox.append(m_infoBar);
        m_mainBox.append(m_lblName);
        m_mainBox.append(m_txtName);
        set_child(m_mainBox);
        maximize();
    }

    MainWindow::~MainWindow()
    {
        //==Save Config==//
        Configuration configuration;
        configuration.save();
    }

    void MainWindow::onShow()
    {
        if(!m_opened)
        {
            m_opened = true;
            //==Load Config==//
            Configuration configuration;
        }
    }

    void MainWindow::newAccount(const Glib::VariantBase& args)
    {

    }

    void MainWindow::openAccount(const Glib::VariantBase& args)
    {

    }

    void MainWindow::closeAccount(const Glib::VariantBase& args)
    {

    }

    void MainWindow::newTransaction()
    {

    }

    void MainWindow::editTransaction()
    {

    }

    void MainWindow::deleteTransaction()
    {
        m_headerBar.getPopDeleteTransaction().popdown();
    }

    void MainWindow::backupAccount()
    {

    }

    void MainWindow::restoreAccount()
    {

    }

    void MainWindow::settings()
    {
        SettingsDialog* settingsDialog = new SettingsDialog(*this);
        settingsDialog->signal_hide().connect(sigc::bind([](SettingsDialog* dialog)
        {
            delete dialog;
        }, settingsDialog));
        settingsDialog->show();
    }

    void MainWindow::checkForUpdates(const Glib::VariantBase& args)
    {
        ProgressDialog* checkingDialog = new ProgressDialog(*this, "Checking for updates...", [&]() { m_updater.checkForUpdates(); });
        checkingDialog->signal_hide().connect(sigc::bind([&](ProgressDialog* dialog)
        {
            delete dialog;
            if(m_updater.updateAvailable())
            {
                Gtk::MessageDialog* updateDialog = new Gtk::MessageDialog(*this, "Update Available", false, Gtk::MessageType::INFO, Gtk::ButtonsType::YES_NO, true);
                updateDialog->set_secondary_text("\n===V" + m_updater.getLatestVersion()->toString() + " Changelog===\n" + m_updater.getChangelog() + "\n\nNickvision Money can automatically download the latest executable to your Downloads directory. Would you like to continue?");
                updateDialog->signal_response().connect(sigc::bind([&](int response, Gtk::MessageDialog* dialog)
                {
                    delete dialog;
                    if(response == Gtk::ResponseType::YES)
                    {
                        bool* success = new bool(false);
                        ProgressDialog* downloadingDialog = new ProgressDialog(*this, "Downloading the update...", [&]() { *success = m_updater.update(); });
                        downloadingDialog->signal_hide().connect(sigc::bind([&](ProgressDialog* dialog, bool* success)
                        {
                            delete dialog;
                            if(*success)
                            {
                                m_infoBar.showMessage("Download Successful", "We recommend moving the new version out of your Downloads directory and running it from elsewhere to allow future updates to download smoothly.");
                            }
                            else
                            {
                                m_infoBar.showMessage("Error", "Unable to download the executable. Please try again. If the issue continues, file a bug report.");
                            }
                            delete success;
                        }, downloadingDialog, success));
                        downloadingDialog->show();
                    }
                }, updateDialog));
                updateDialog->show();
            }
            else
            {
                m_infoBar.showMessage("No Update Available", "There is no update at this time. Please check again later.");
            }
        }, checkingDialog));
        checkingDialog->show();
    }

    void MainWindow::gitHubRepo(const Glib::VariantBase& args)
    {
        Gio::AppInfo::launch_default_for_uri("https://github.com/nlogozzo/NickvisionMoney");
    }

    void MainWindow::reportABug(const Glib::VariantBase& args)
    {
        Gio::AppInfo::launch_default_for_uri("https://github.com/nlogozzo/NickvisionMoney/issues/new");
    }

    void MainWindow::changelog(const Glib::VariantBase& args)
    {
        Gtk::MessageDialog* changelogDialog = new Gtk::MessageDialog(*this, "What's New?", false, Gtk::MessageType::INFO, Gtk::ButtonsType::OK, true);
        changelogDialog->set_secondary_text("\n- Initial Release");
        changelogDialog->signal_response().connect(sigc::bind([](int response, Gtk::MessageDialog* dialog)
        {
           delete dialog;
        }, changelogDialog));
        changelogDialog->show();
    }

    void MainWindow::about(const Glib::VariantBase& args)
    {
        Gtk::AboutDialog* aboutDialog = new Gtk::AboutDialog();
        aboutDialog->set_transient_for(*this);
        aboutDialog->set_modal(true);
        aboutDialog->set_hide_on_close(true);
        aboutDialog->set_program_name("Nickvision Money");
        aboutDialog->set_version("2022.1.0-alpha1");
        aboutDialog->set_comments("A personal finance manager.");
        aboutDialog->set_copyright("(C) Nickvision 2021-2022");
        aboutDialog->set_license_type(Gtk::License::GPL_3_0);
        aboutDialog->set_website("https://github.com/nlogozzo");
        aboutDialog->set_website_label("GitHub");
        aboutDialog->set_authors({ "Nicholas Logozzo" });
        aboutDialog->signal_hide().connect(sigc::bind([](Gtk::AboutDialog* dialog)
        {
           delete dialog;
        }, aboutDialog));
        aboutDialog->show();
    }
}
