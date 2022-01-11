#include "transactiondialog.h"

namespace NickvisionMoney::Views
{
    using namespace NickvisionMoney::Models;

    TransactionDialog::TransactionDialog(Gtk::Window& parent, Account& account, std::optional<unsigned int> idToEdit) : Gtk::Dialog("Transaction", parent, true, true), m_account(account), m_isNew(!idToEdit.has_value())
    {
        //==Settings==//
        set_resizable(false);
        set_deletable(false);
        //==Buttons==//
        Gtk::Button* btnCancel = add_button("_Cancel", Gtk::ResponseType::CANCEL);
        Gtk::Button* btnSave = add_button("_Save", Gtk::ResponseType::NONE);
        btnSave->signal_clicked().connect(sigc::mem_fun(*this, &TransactionDialog::save));
        //==Main Box==//
        m_mainBox.set_orientation(Gtk::Orientation::VERTICAL);
        m_mainBox.set_spacing(6);
        m_mainBox.set_margin(6);
        //InfoBar
        m_mainBox.append(m_infoBar);
        //ID
        m_lblID.set_label("ID");
        m_lblID.set_halign(Gtk::Align::START);
        m_txtID.set_placeholder_text("Enter id here");
        m_txtID.set_size_request(400, -1);
        m_mainBox.append(m_lblID);
        m_mainBox.append(m_txtID);
        //Date
        m_lblDate.set_label("Date");
        m_lblDate.set_halign(Gtk::Align::START);
        m_calDate.set_size_request(400, -1);
        m_mainBox.append(m_lblDate);
        m_mainBox.append(m_calDate);
        //Description
        m_lblDescription.set_label("Description");
        m_lblDescription.set_halign(Gtk::Align::START);
        m_txtDescription.set_placeholder_text("Enter description here");
        m_txtDescription.set_size_request(400, -1);
        m_mainBox.append(m_lblDescription);
        m_mainBox.append(m_txtDescription);
        //Type
        m_lblType.set_label("Type");
        m_lblType.set_halign(Gtk::Align::START);
        m_cmbType.append("Income");
        m_cmbType.append("Expense");
        m_cmbType.set_active(0);
        m_cmbType.set_size_request(400, -1);
        m_mainBox.append(m_lblType);
        m_mainBox.append(m_cmbType);
        //Amount
        m_lblAmount.set_label("Amount");
        m_lblAmount.set_halign(Gtk::Align::START);
        m_txtAmount.set_placeholder_text("Enter amount here");
        m_txtAmount.set_size_request(400, -1);
        m_mainBox.append(m_lblAmount);
        m_mainBox.append(m_txtAmount);
        //==Layout==//
        set_child(m_mainBox);
        //==Load Transaction==//
        if(idToEdit.has_value())
        {
            Transaction transaction = *(m_account.getTransactionByID(*idToEdit));
            m_txtID.set_text(std::to_string(transaction.getID()));
            m_txtID.set_editable(false);
            m_calDate.select_day(Glib::DateTime::create_from_iso8601(transaction.getDate()));
            m_txtDescription.set_text(transaction.getDescription());
            m_cmbType.set_active(static_cast<int>(transaction.getType()));
            m_txtAmount.set_text(transaction.getAmountAsString());
        }
    }

    void TransactionDialog::save()
    {
        if(m_txtID.get_text().empty())
        {
            m_infoBar.showMessage("Error", "ID can not be empty.");
        }
        else if(m_txtDescription.get_text().empty())
        {
            m_infoBar.showMessage("Error", "Description can not be empty.");
        }
        else if(m_txtAmount.get_text().empty())
        {
            m_infoBar.showMessage("Error", "Amount can not be empty.");
        }
        else
        {
            int id = 0;
            try
            {
                id = std::stoi(m_txtID.get_text());
            }
            catch(...)
            {
                m_infoBar.showMessage("Error", "ID must be a valid number value.");
                return;
            }
            double amount = 0.00;
            try
            {
                amount = std::stod(m_txtAmount.get_text());
            }
            catch(...)
            {
                m_infoBar.showMessage("Error", "Amount must be a valid number value.");
                return;
            }
            if(id <= 0)
            {
                m_infoBar.showMessage("Error", "ID must be greater than 0.");
            }
            else
            {
                Transaction transaction(id);
                transaction.setDate(m_calDate.get_date().format_iso8601());
                transaction.setDescription(m_txtDescription.get_text());
                transaction.setType(static_cast<TransactionType>(m_cmbType.get_active_row_number()));
                transaction.setAmount(amount);
                if(m_isNew)
                {

                    if(m_account.getTransactionByID(id).has_value())
                    {
                        m_infoBar.showMessage("Error", "A transaction with this ID already exists.");
                    }
                    else
                    {
                        m_account.addTransaction(transaction);
                        response(Gtk::ResponseType::OK);
                    }
                }
                else
                {
                    m_account.updateTransaction(transaction);
                    response(Gtk::ResponseType::OK);
                }
            }
        }
    }
}
