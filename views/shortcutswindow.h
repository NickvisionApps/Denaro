#ifndef SHORTCUTSWINDOW_H
#define SHORTCUTSWINDOW_H

#include <gtkmm.h>

namespace NickvisionMoney::Views
{
    class ShortcutsWindow : public Gtk::ShortcutsWindow
    {
    public:
        ShortcutsWindow(Gtk::Window& parent);

    private:
        Gtk::ShortcutsSection m_section;
        Gtk::ShortcutsGroup m_grpAccount;
        Gtk::ShortcutsShortcut m_shortNewAccount;
        Gtk::ShortcutsShortcut m_shortOpenAccount;
        Gtk::ShortcutsShortcut m_shortCloseAccount;
        Gtk::ShortcutsShortcut m_shortBackupAccount;
        Gtk::ShortcutsShortcut m_shortRestoreAccount;
        Gtk::ShortcutsGroup m_grpTransaction;
        Gtk::ShortcutsShortcut m_shortNewTransaction;
        Gtk::ShortcutsShortcut m_shortEditTransaction;
        Gtk::ShortcutsShortcut m_shortDeleteTransaction;
        Gtk::ShortcutsGroup m_grpApplication;
        Gtk::ShortcutsShortcut m_shortAbout;
    };
}

#endif // SHORTCUTSWINDOW_H
