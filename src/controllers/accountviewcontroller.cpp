#include "accountviewcontroller.hpp"
#include <sstream>

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::Models;

AccountViewController::AccountViewController(const std::string& path, const std::string& currencySymbol, bool displayCurrencySymbolOnRight, const std::function<void(const std::string& message)>& sendToastCallback) : m_currencySymbol{ currencySymbol }, m_displayCurrencySymbolOnRight{ displayCurrencySymbolOnRight }, m_account{ path }, m_sendToastCallback{ sendToastCallback }
{

}

const std::string& AccountViewController::getAccountPath() const
{
    return m_account.getPath();
}

const std::string& AccountViewController::getCurrencySymbol() const
{
    return m_currencySymbol;
}

bool AccountViewController::getDisplayCurrencySymbolOnRight() const
{
    return m_displayCurrencySymbolOnRight;
}

std::string AccountViewController::getAccountTotalString() const
{
    std::stringstream builder;
    if(m_displayCurrencySymbolOnRight)
    {
        builder << m_account.getTotal() << m_currencySymbol;
    }
    else
    {
        builder << m_currencySymbol << m_account.getTotal();
    }
    return builder.str();
}

std::string AccountViewController::getAccountIncomeString() const
{
    std::stringstream builder;
    if(m_displayCurrencySymbolOnRight)
    {
        builder << m_account.getIncome() << m_currencySymbol;
    }
    else
    {
        builder << m_currencySymbol << m_account.getIncome();
    }
    return builder.str();
}

std::string AccountViewController::getAccountExpenseString() const
{
    std::stringstream builder;
    if(m_displayCurrencySymbolOnRight)
    {
        builder << m_account.getExpense() << m_currencySymbol;
    }
    else
    {
        builder << m_currencySymbol << m_account.getExpense();
    }
    return builder.str();
}

const std::map<unsigned int, Group>& AccountViewController::getGroups() const
{
    return m_account.getGroups();
}

const std::map<unsigned int, Transaction>& AccountViewController::getTransactions() const
{
    return m_account.getTransactions();
}

void AccountViewController::registerAccountInfoChangedCallback(const std::function<void()>& callback)
{
    m_accountInfoChangedCallback = callback;
}

void AccountViewController::exportAsCSV(std::string& path)
{
    if(std::filesystem::path(path).extension().empty() || std::filesystem::path(path).extension() != ".csv")
    {
        path += ".csv";
    }
    if(m_account.exportAsCSV(path))
    {
        m_sendToastCallback("Exported account to CSV successfully.");
    }
    else
    {
        m_sendToastCallback("Unable to export account as CSV.");
    }
}

void AccountViewController::importFromCSV(std::string& path)
{
    int imported{ m_account.importFromCSV(path) };
    if(imported > 0)
    {
        m_accountInfoChangedCallback();
    }
    m_sendToastCallback("Imported " + std::to_string(imported) + " transactions from CSV.");
}

void AccountViewController::addGroup(const Group& group)
{
    m_account.addGroup(group);
    m_accountInfoChangedCallback();
}

void AccountViewController::updateGroup(const Group& group)
{
    m_account.updateGroup(group);
    m_accountInfoChangedCallback();
}

void AccountViewController::deleteGroup(unsigned int id)
{
    m_account.deleteGroup(id);
    m_accountInfoChangedCallback();
}

void AccountViewController::addTransaction(const Transaction& transaction)
{
    m_account.addTransaction(transaction);
    m_accountInfoChangedCallback();
}

void AccountViewController::updateTransaction(const Transaction& transaction)
{
    m_account.updateTransaction(transaction);
    m_accountInfoChangedCallback();
}

void AccountViewController::deleteTransaction(unsigned int id)
{
    m_account.deleteTransaction(id);
    m_accountInfoChangedCallback();
}

TransactionDialogController AccountViewController::createTransactionDialogController() const
{
    return { m_account.getNextAvailableTransactionId(), m_currencySymbol };
}

TransactionDialogController AccountViewController::createTransactionDialogController(unsigned int id) const
{
    return { m_account.getTransactionById(id).value(), m_currencySymbol };
}
