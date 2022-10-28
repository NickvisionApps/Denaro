#include "transactiondialogcontroller.hpp"
#include <sstream>
#include <boost/date_time/gregorian/gregorian.hpp>

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::Models;

TransactionDialogController::TransactionDialogController(unsigned int newId) : m_response{ "cancel" }, m_transaction{ newId }
{

}

TransactionDialogController::TransactionDialogController(const Transaction& transaction) : m_response{ "cancel" }, m_transaction{ transaction }
{

}

const std::string& TransactionDialogController::getResponse() const
{
    return m_response;
}

void TransactionDialogController::setResponse(const std::string& response)
{
    m_response = response;
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

std::string TransactionDialogController::getAmountAsString() const
{
    std::stringstream builder;
    builder << m_transaction.getAmount();
    return builder.str();
}

TransactionCheckStatus TransactionDialogController::updateTransaction(const std::string& dateString, const std::string& description, int type, int repeatInterval, const std::string& amountString)
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
    m_transaction.setAmount(amount);
    return TransactionCheckStatus::Valid;
}
