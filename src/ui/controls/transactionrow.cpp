#include "transactionrow.hpp"
#include <sstream>
#include <boost/date_time/gregorian/gregorian.hpp>

using namespace NickvisionMoney::Models;
using namespace NickvisionMoney::UI::Controls;

TransactionRow::TransactionRow(const Transaction& transaction, const std::string& currencySymbol) : m_transaction{ transaction }, m_gobj{ adw_action_row_new() }
{
    //Row Settings
    std::stringstream builder;
    builder << m_transaction.getId() << " - " << m_transaction.getDescription();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_gobj), builder.str().c_str());
    builder.str("");
    builder.clear();
    builder << (m_transaction.getType() == TransactionType::Income ? "+ " : "- ") << currencySymbol << m_transaction.getAmount() << "\n";
    builder << boost::gregorian::to_iso_extended_string(m_transaction.getDate());
    adw_action_row_set_subtitle(ADW_ACTION_ROW(m_gobj), builder.str().c_str());
}

GtkWidget* TransactionRow::gobj()
{
    return m_gobj;
}
