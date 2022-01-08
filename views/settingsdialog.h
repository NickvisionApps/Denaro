#ifndef SETTINGSDIALOG_H
#define SETTINGSDIALOG_H

#include <gtkmm.h>
#include "../models/configuration.h"

namespace NickvisionMoney::Views
{
    class SettingsDialog : public Gtk::Dialog
    {
    public:
        SettingsDialog(Gtk::Window& parent);
        ~SettingsDialog();

    private:
        NickvisionMoney::Models::Configuration m_configuration;
        Gtk::ScrolledWindow m_scroll;
        Gtk::Box m_mainBox;
        Gtk::Label m_lblGeneral;
        Gtk::ListBox m_listGeneral;
        Gtk::CheckButton m_chkRememberLastOpenedAccount;
    };
}

#endif // SETTINGSDIALOG_H
