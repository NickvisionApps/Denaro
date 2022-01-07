#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <gtkmm.h>
#include "../models/update/updater.h"
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
        //==UI==//
        NickvisionMoney::Controls::HeaderBar m_headerBar;
        Gtk::Box m_mainBox;
        NickvisionMoney::Controls::InfoBar m_infoBar;
        Gtk::Label m_lblName;
        Gtk::Entry m_txtName;
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
        void settings();
        void checkForUpdates(const Glib::VariantBase& args);
        void gitHubRepo(const Glib::VariantBase& args);
        void reportABug(const Glib::VariantBase& args);
        void changelog(const Glib::VariantBase& args);
        void about(const Glib::VariantBase& args);
    };
}

#endif // MAINWINDOW_H
