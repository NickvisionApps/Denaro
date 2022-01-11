#ifndef TRANSACTIONDIALOG_H
#define TRANSACTIONDIALOG_H

#include <optional>
#include <gtkmm.h>
#include "../models/account.h"
#include "../controls/infobar.h"

namespace NickvisionMoney::Views
{
    class TransactionDialog : public Gtk::Dialog
    {
    public:
        TransactionDialog(Gtk::Window& parent, NickvisionMoney::Models::Account& account, std::optional<unsigned int> idToEdit = std::nullopt);

    private:
        NickvisionMoney::Models::Account& m_account;
        bool m_isNew;
        //==UI==//
        Gtk::Box m_mainBox;
        NickvisionMoney::Controls::InfoBar m_infoBar;
        Gtk::Label m_lblID;
        Gtk::Entry m_txtID;
        Gtk::Label m_lblDate;
        Gtk::Calendar m_calDate;
        Gtk::Label m_lblDescription;
        Gtk::Entry m_txtDescription;
        Gtk::Label m_lblType;
        Gtk::ComboBoxText m_cmbType;
        Gtk::Label m_lblAmount;
        Gtk::Entry m_txtAmount;
        //==Slots==//
        void save();
    };
}

#endif // TRANSACTIONDIALOG_H
