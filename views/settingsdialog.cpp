#include "settingsdialog.h"

namespace NickvisionMoney::Views
{
    SettingsDialog::SettingsDialog(Gtk::Window& parent) : Gtk::Dialog("Settings", parent, true, true)
    {
        //==Settings==//
        set_default_size(600, 500);
        set_resizable(false);
        set_hide_on_close(true);
        //==General Section==//
        m_lblGeneral.set_markup("<b>General</b>");
        m_lblGeneral.set_halign(Gtk::Align::START);
        m_lblGeneral.set_margin_start(20);
        m_lblGeneral.set_margin_top(20);
        m_listGeneral.set_selection_mode(Gtk::SelectionMode::NONE);
        m_listGeneral.set_margin_top(6);
        m_listGeneral.set_margin_start(20);
        m_listGeneral.set_margin_end(20);
        m_chkRememberLastOpenedAccount.set_label("Remember Last Opened Account");
        m_chkRememberLastOpenedAccount.set_tooltip_text("If checked, Money will remember the last opened account and automatically open it again when the application starts again.");
        m_listGeneral.append(m_chkRememberLastOpenedAccount);
        //==Layout==//
        m_mainBox.set_orientation(Gtk::Orientation::VERTICAL);
        m_mainBox.append(m_lblGeneral);
        m_mainBox.append(m_listGeneral);
        m_scroll.set_child(m_mainBox);
        set_child(m_scroll);
        //==Load Configuration==//
        m_chkRememberLastOpenedAccount.set_active(m_configuration.rememberLastOpenedAccount());
    }

    SettingsDialog::~SettingsDialog()
    {
        m_configuration.setRememberLastOpenedAccount(m_chkRememberLastOpenedAccount.get_active());
        m_configuration.save();
    }
}
