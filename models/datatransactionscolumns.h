#ifndef DATATRANSACTIONSCOLUMNS_H
#define DATATRANSACTIONSCOLUMNS_H

#include <string>
#include <gtkmm.h>

namespace NickvisionMoney::Models
{
    class DataTransactionsColumns : public Gtk::TreeModel::ColumnRecord
    {
    public:
        DataTransactionsColumns();
        const Gtk::TreeModelColumn<unsigned int>& getColID() const;
        const Gtk::TreeModelColumn<std::string>& getColDate() const;
        const Gtk::TreeModelColumn<std::string>& getColDescription() const;
        const Gtk::TreeModelColumn<std::string>& getColType() const;
        const Gtk::TreeModelColumn<double>& getColAmount() const;

    private:
        Gtk::TreeModelColumn<unsigned int> m_colID;
        Gtk::TreeModelColumn<std::string> m_colDate;
        Gtk::TreeModelColumn<std::string> m_colDescription;
        Gtk::TreeModelColumn<std::string> m_colType;
        Gtk::TreeModelColumn<double> m_colAmount;
    };
}

#endif // DATATRANSACTIONSCOLUMNS_H
