#include "shortcutswindow.h"

namespace NickvisionMoney::Views
{
    ShortcutsWindow::ShortcutsWindow(Gtk::Window& parent)
    {
        //==Settings==//
        set_title("Shortcuts");
        set_transient_for(parent);
        set_modal(true);
        set_hide_on_close(true);
        //==Account==//
        m_grpAccount.property_title().set_value("Account");
        m_shortNewAccount.property_accelerator().set_value("<Ctrl>N");
        m_shortNewAccount.property_title().set_value("New Account");
        m_shortOpenAccount.property_accelerator().set_value("<Ctrl>O");
        m_shortOpenAccount.property_title().set_value("Open Account");
        m_shortCloseAccount.property_accelerator().set_value("<Ctrl>W");
        m_shortCloseAccount.property_title().set_value("Close Account");
        m_shortBackupAccount.property_accelerator().set_value("<Ctrl><Shift>B");
        m_shortBackupAccount.property_title().set_value("Backup Account");
        m_shortRestoreAccount.property_accelerator().set_value("<Ctrl><Shift>R");
        m_shortRestoreAccount.property_title().set_value("Restore Account");
        m_grpAccount.append(m_shortNewAccount);
        m_grpAccount.append(m_shortOpenAccount);
        m_grpAccount.append(m_shortCloseAccount);
        m_grpAccount.append(m_shortBackupAccount);
        m_grpAccount.append(m_shortRestoreAccount);
        m_section.append(m_grpAccount);
        //==Transaction==//
        m_grpTransaction.property_title().set_value("Transaction");
        m_shortNewTransaction.property_accelerator().set_value("<Ctrl><Shift>N");
        m_shortNewTransaction.property_title().set_value("New Transaction");
        m_shortEditTransaction.property_accelerator().set_value("<Ctrl><Shift>O");
        m_shortEditTransaction.property_title().set_value("Edit Transaction");
        m_shortDeleteTransaction.property_accelerator().set_value("Delete");
        m_shortDeleteTransaction.property_title().set_value("Delete Transaction");
        m_grpTransaction.append(m_shortNewTransaction);
        m_grpTransaction.append(m_shortEditTransaction);
        m_grpTransaction.append(m_shortDeleteTransaction);
        m_section.append(m_grpTransaction);
        //==Application==//
        m_grpApplication.property_title().set_value("Application");
        m_shortAbout.property_accelerator().set_value("F1");
        m_shortAbout.property_title().set_value("About");
        m_grpApplication.append(m_shortAbout);
        m_section.append(m_grpApplication);
        //==Layout==//
        set_child(m_section);
    }
}
