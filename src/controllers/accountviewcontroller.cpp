#include "accountviewcontroller.hpp"
#include "../helpers/moneyhelpers.hpp"
#include "../helpers/stringhelpers.hpp"
#include "../helpers/translation.hpp"

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::Helpers;
using namespace NickvisionMoney::Models;

AccountViewController::AccountViewController(const std::string& path, Configuration& configuration, const std::function<void(const std::string& message)>& sendToastCallback) : m_configuration{ configuration }, m_account{ path }, m_sendToastCallback{ sendToastCallback }
{

}

const std::string& AccountViewController::getAccountPath() const
{
    return m_account.getPath();
}

const std::locale& AccountViewController::getLocale() const
{
    return m_configuration.getLocale();
}

std::string AccountViewController::getAccountTotalString() const
{
    return MoneyHelpers::boostMoneyToLocaleString(m_account.getTotal(), m_configuration.getLocale());
}

std::string AccountViewController::getAccountIncomeString() const
{
    return MoneyHelpers::boostMoneyToLocaleString(m_account.getIncome(), m_configuration.getLocale());
}

std::string AccountViewController::getAccountExpenseString() const
{
    return MoneyHelpers::boostMoneyToLocaleString(m_account.getExpense(), m_configuration.getLocale());
}

const std::map<unsigned int, Group>& AccountViewController::getGroups() const
{
    return m_account.getGroups();
}

const std::map<unsigned int, Transaction>& AccountViewController::getTransactions() const
{
    return m_account.getTransactions();
}

std::vector<Transaction> AccountViewController::getFilteredTransactions() const
{
    std::vector<Transaction> filteredTransactions;
    for(const std::pair<const unsigned int, Transaction>& pair : m_account.getTransactions())
    {
        filteredTransactions.push_back(pair.second);
    }
    return filteredTransactions;
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
    m_sendToastCallback(StringHelpers::format(ngettext("Imported %d transaction from CSV.", "Imported %d transactions from CSV.", imported), imported));
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
    std::vector<std::string> groupNames;
    for(const std::pair<const unsigned int, Group>& pair : m_account.getGroups())
    {
        groupNames.push_back(pair.second.getName());
    }
    return { m_account.getNextAvailableGroupId(), groupNames };
}

GroupDialogController AccountViewController::createGroupDialogController(unsigned int id) const
{
    std::vector<std::string> groupNames;
    for(const std::pair<const unsigned int, Group>& pair : m_account.getGroups())
    {
        groupNames.push_back(pair.second.getName());
    }
    return { m_account.getGroupById(id).value(), groupNames };
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
    return { m_account.getNextAvailableTransactionId(), m_account.getGroups(), m_configuration.getLocale() };
}

TransactionDialogController AccountViewController::createTransactionDialogController(unsigned int id) const
{
    return { m_account.getTransactionById(id).value(), m_account.getGroups(), m_configuration.getLocale() };
}

bool AccountViewController::getSortFirstToLast() const
{
    return m_configuration.getSortFirstToLast();
}

void AccountViewController::setSortFirstToLast(bool sortFirstToLast)
{
    m_configuration.setSortFirstToLast(sortFirstToLast);
    m_configuration.save();
}
