#include "accountviewcontroller.hpp"
#include "../helpers/moneyhelpers.hpp"
#include "../helpers/stringhelpers.hpp"
#include "../helpers/translation.hpp"

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::Helpers;
using namespace NickvisionMoney::Models;

AccountViewController::AccountViewController(const std::string& path, const std::locale& locale, const std::function<void(const std::string& message)>& sendToastCallback) : m_locale{ locale }, m_account{ path }, m_sendToastCallback{ sendToastCallback }
{

}

const std::string& AccountViewController::getAccountPath() const
{
    return m_account.getPath();
}

const std::locale& AccountViewController::getLocale() const
{
    return m_locale;
}

std::string AccountViewController::getAccountTotalString() const
{
    return MoneyHelpers::boostMoneyToLocaleString(m_account.getTotal(), m_locale);
}

std::string AccountViewController::getAccountIncomeString() const
{
    return MoneyHelpers::boostMoneyToLocaleString(m_account.getIncome(), m_locale);
}

std::string AccountViewController::getAccountExpenseString() const
{
    return MoneyHelpers::boostMoneyToLocaleString(m_account.getExpense(), m_locale);
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
        m_sendToastCallback(_("Exported account to CSV successfully."));
    }
    else
    {
        m_sendToastCallback(_("Unable to export account as CSV."));
    }
}

void AccountViewController::importFromCSV(std::string& path)
{
    int imported{ m_account.importFromCSV(path) };
    if(imported > 0)
    {
        m_accountInfoChangedCallback();
    }
    m_sendToastCallback(StringHelpers::format(_("Imported %d transactions from CSV."), imported));
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

GroupDialogController AccountViewController::createGroupDialogController() const
{
    return { m_account.getNextAvailableGroupId() };
}

GroupDialogController AccountViewController::createGroupDialogController(unsigned int id) const
{
    return { m_account.getGroupById(id).value() };
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
    return { m_account.getNextAvailableTransactionId(), m_account.getGroups(), m_locale };
}

TransactionDialogController AccountViewController::createTransactionDialogController(unsigned int id) const
{
    return { m_account.getTransactionById(id).value(), m_account.getGroups(), m_locale };
}

