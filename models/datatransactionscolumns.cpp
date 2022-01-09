#include "datatransactionscolumns.h"

namespace NickvisionMoney::Models
{
    DataTransactionsColumns::DataTransactionsColumns()
    {
        add(m_colID);
        add(m_colDate);
        add(m_colDescription);
        add(m_colType);
        add(m_colAmount);
    }

    const Gtk::TreeModelColumn<unsigned int>& DataTransactionsColumns::getColID() const
    {
        return m_colID;
    }

    const Gtk::TreeModelColumn<std::string>& DataTransactionsColumns::getColDate() const
    {
        return m_colDate;
    }

    const Gtk::TreeModelColumn<std::string>& DataTransactionsColumns::getColDescription() const
    {
        return m_colDescription;
    }

    const Gtk::TreeModelColumn<std::string>& DataTransactionsColumns::getColType() const
    {
        return m_colType;
    }

    const Gtk::TreeModelColumn<double>& DataTransactionsColumns::getColAmount() const
    {
        return m_colAmount;
    }
}
