#include "transactiondialogcontroller.hpp"
#include <sstream>
#include <boost/date_time/gregorian/gregorian.hpp>
#include "../helpers/translation.hpp"

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::Models;

TransactionDialogController::TransactionDialogController(unsigned int newId, const std::string& currencySymbol, const std::map<unsigned int, Group>& groups) : m_response{ "cancel" }, m_currencySymbol{ currencySymbol }, m_transaction{ newId }, m_groups{ groups }
{
    m_groupNames.push_back(_("None"));
    for(const std::pair<const unsigned int, Group>& pair : m_groups)
    {
        m_groupNames.push_back(pair.second.getName());
    }
}

TransactionDialogController::TransactionDialogController(const Transaction& transaction, const std::string& currencySymbol, const std::map<unsigned int, Group>& groups) : m_response{ "cancel" }, m_currencySymbol{ currencySymbol }, m_transaction{ transaction }, m_groups{ groups }
{
    m_groupNames.push_back(_("None"));
    for(const std::pair<const unsigned int, Group>& pair : m_groups)
    {
        m_groupNames.push_back(pair.second.getName());
    }
}

const std::string& TransactionDialogController::getResponse() const
{
    return m_response;
}

void TransactionDialogController::setResponse(const std::string& response)
{
    m_response = response;
}

const std::string& TransactionDialogController::getCurrencySymbol() const
{
    return m_currencySymbol;
}

const Transaction& TransactionDialogController::getTransaction() const
{
    return m_transaction;
}

std::string TransactionDialogController::getIdAsString() const
{
    return std::to_string(m_transaction.getId());
}

int TransactionDialogController::getYear() const
{
    return m_transaction.getDate().year();
}

int TransactionDialogController::getMonth() const
{
    return m_transaction.getDate().month();
}

int TransactionDialogController::getDay() const
{
    return m_transaction.getDate().day();
}

const std::string& TransactionDialogController::getDescription() const
{
    return m_transaction.getDescription();
}

int TransactionDialogController::getTypeAsInt() const
{
    return static_cast<int>(m_transaction.getType());
}

int TransactionDialogController::getRepeatIntervalAsInt() const
{
    return static_cast<int>(m_transaction.getRepeatInterval());
}

const std::vector<std::string>& TransactionDialogController::getGroupNames() const
{
    return m_groupNames;
}

int TransactionDialogController::getGroupAsIndex() const
{
    if(m_transaction.getGroupId() == -1)
    {
        return 0;
    }
    return std::distance(m_groups.begin(), m_groups.find(m_transaction.getGroupId())) + 1;
}

std::string TransactionDialogController::getAmountAsString() const
{
    std::stringstream builder;
    builder << m_transaction.getAmount();
    return builder.str();
}

TransactionCheckStatus TransactionDialogController::updateTransaction(const std::string& dateString, const std::string& description, int type, int repeatInterval, int groupIndex, const std::string& amountString)
{
    if(description.empty())
    {
        return TransactionCheckStatus::EmptyDescription;
    }
    if(amountString.empty())
    {
        return TransactionCheckStatus::EmptyAmount;
    }
    boost::multiprecision::cpp_dec_float_50 amount{ 0.0 };
    try
    {
        amount = static_cast<boost::multiprecision::cpp_dec_float_50>(amountString);
    }
    catch(...)
    {
        return TransactionCheckStatus::InvalidAmount;
    }
    m_transaction.setDate(boost::gregorian::from_string(dateString));
    m_transaction.setDescription(description);
    m_transaction.setType(static_cast<TransactionType>(type));
    m_transaction.setRepeatInterval(static_cast<RepeatInterval>(repeatInterval));
    if(groupIndex == 0)
    {
         m_transaction.setGroupId(-1);
    }
    else
    {
        std::map<unsigned int, Group>::iterator it{ m_groups.begin() };
        std::advance(it, groupIndex - 1);
        m_transaction.setGroupId(it->second.getId());
    }
    m_transaction.setAmount(amount);
    return TransactionCheckStatus::Valid;
}
