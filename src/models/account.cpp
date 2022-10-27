#include "account.hpp"
#include <chrono>
#include <filesystem>
#include <sstream>
#include <boost/date_time/gregorian/gregorian.hpp>

using namespace NickvisionMoney::Models;

Account::Account(const std::string& path) : m_path{ path }, m_db{ std::make_shared<SQLite::Database>(m_path, SQLite::OPEN_READWRITE | SQLite::OPEN_CREATE) }
{
    m_db->exec("CREATE TABLE IF NOT EXISTS transactions (id INTEGER PRIMARY KEY, date TEXT, description TEXT, type INTEGER, repeat INTEGER, amount REAL)");
    SQLite::Statement qryGetAll{ *m_db, "SELECT * FROM transactions" };
    while(qryGetAll.executeStep())
    {
        Transaction transaction(qryGetAll.getColumn(0).getInt());
        transaction.setDate(boost::gregorian::from_string(qryGetAll.getColumn(1).getString()));
        transaction.setDescription(qryGetAll.getColumn(2).getString());
        transaction.setType(static_cast<TransactionType>(qryGetAll.getColumn(3).getInt()));
        transaction.setRepeatInterval(static_cast<RepeatInterval>(qryGetAll.getColumn(4).getInt()));
        transaction.setAmount(boost::multiprecision::cpp_dec_float_50(qryGetAll.getColumn(5).getString()));
        m_transactions.insert({ transaction.getId(), transaction });
    }
    //==Repeat Needed Transactions==//
    size_t i{ 0 };
    size_t startingSize{ m_transactions.size() };
    std::map<unsigned int, Transaction>::iterator it{ m_transactions.begin() };
    while(i != startingSize)
    {
        Transaction transaction = it->second;
        if(transaction.getRepeatInterval() != RepeatInterval::Never)
        {
            bool repeatNeeeded{ false };
            boost::gregorian::date today{ boost::gregorian::day_clock::universal_day() };
            if(transaction.getRepeatInterval() == RepeatInterval::Daily)
            {
                if(today >= transaction.getDate() + boost::gregorian::date_duration(1))
                {
                    repeatNeeeded = true;
                }
            }
            else if(transaction.getRepeatInterval() == RepeatInterval::Weekly)
            {
                if(today >= transaction.getDate() + boost::gregorian::date_duration(7))
                {
                    repeatNeeeded = true;
                }
            }
            else if(transaction.getRepeatInterval() == RepeatInterval::Monthly)
            {
                if(today >= transaction.getDate() + boost::gregorian::months(1))
                {
                    repeatNeeeded = true;
                }
            }
            else if(transaction.getRepeatInterval() == RepeatInterval::Quarterly)
            {
                if(today >= transaction.getDate() + boost::gregorian::months(4))
                {
                    repeatNeeeded = true;
                }
            }
            else if(transaction.getRepeatInterval() == RepeatInterval::Yearly)
            {
                if(today >= transaction.getDate() + boost::gregorian::years(1))
                {
                    repeatNeeeded = true;
                }
            }
            else
            {
                if(today >= transaction.getDate() + boost::gregorian::years(2))
                {
                    repeatNeeeded = true;
                }
            }
            if(repeatNeeeded)
            {
                Transaction newTransaction(getNextAvailableId());
                newTransaction.setDate(today);
                newTransaction.setDescription(transaction.getDescription());
                newTransaction.setType(transaction.getType());
                newTransaction.setRepeatInterval(transaction.getRepeatInterval());
                newTransaction.setAmount(transaction.getAmount());
                addTransaction(newTransaction);
                transaction.setRepeatInterval(RepeatInterval::Never);
                updateTransaction(transaction);
            }
        }
        i++;
        it++;
    }
}

const std::string& Account::getPath() const
{
    return m_path;
}

const std::map<unsigned int, Transaction>& Account::getTransactions() const
{
    return m_transactions;
}

std::optional<Transaction> Account::getTransactionById(unsigned int id) const
{
    try
    {
        return m_transactions.at(id);
    }
    catch (...)
    {
        return std::nullopt;
    }
}

unsigned int Account::getNextAvailableId() const
{
    if(m_transactions.empty())
    {
        return 1;
    }
    else
    {
        return m_transactions.rbegin()->first + 1;
    }
}

bool Account::addTransaction(const Transaction& transaction)
{
    SQLite::Statement qryInsert{ *m_db, "INSERT INTO transactions (id, date, description, type, repeat, amount) VALUES (?, ?, ?, ?, ?, ?)" };
    std::stringstream strAmount;
    strAmount << transaction.getAmount();
    qryInsert.bind(1, transaction.getId());
    qryInsert.bind(2, boost::gregorian::to_iso_extended_string(transaction.getDate()));
    qryInsert.bind(3, transaction.getDescription());
    qryInsert.bind(4, static_cast<int>(transaction.getType()));
    qryInsert.bind(5, static_cast<int>(transaction.getRepeatInterval()));
    qryInsert.bind(6, strAmount.str());
    if(qryInsert.exec() > 0)
    {
        m_transactions.insert({ transaction.getId(), transaction });
        return true;
    }
    return false;
}

bool Account::updateTransaction(const Transaction& transaction)
{
    SQLite::Statement qryUpdate{ *m_db, "UPDATE transactions SET date = ?, description = ?, type = ?, repeat = ?, amount = ? WHERE id = " + std::to_string(transaction.getId()) };
    std::stringstream strAmount;
    strAmount << transaction.getAmount();
    qryUpdate.bind(1, boost::gregorian::to_iso_extended_string(transaction.getDate()));
    qryUpdate.bind(2, transaction.getDescription());
    qryUpdate.bind(3, static_cast<int>(transaction.getType()));
    qryUpdate.bind(4, static_cast<int>(transaction.getRepeatInterval()));
    qryUpdate.bind(5, strAmount.str());
    if(qryUpdate.exec() > 0)
    {
        m_transactions[transaction.getId()] = transaction;
        return true;
    }
    return false;
}

bool Account::deleteTransaction(unsigned int id)
{
    if(m_db->exec("DELETE FROM transactions WHERE id = " + std::to_string(id)) > 0)
    {
        m_transactions.erase(id);
        return true;
    }
    return false;
}

boost::multiprecision::cpp_dec_float_50 Account::getIncome() const
{
    boost::multiprecision::cpp_dec_float_50 income{ 0.00 };
    for(const std::pair<const unsigned int, Transaction>& pair : m_transactions)
    {
        if(pair.second.getType() == TransactionType::Income)
        {
            income += pair.second.getAmount();
        }
    }
    return income;
}

boost::multiprecision::cpp_dec_float_50 Account::getExpense() const
{
    boost::multiprecision::cpp_dec_float_50 expense{ 0.00 };
    for(const std::pair<const unsigned int, Transaction>& pair : m_transactions)
    {
        if(pair.second.getType() == TransactionType::Expense)
        {
            expense += pair.second.getAmount();
        }
    }
    return expense;
}

boost::multiprecision::cpp_dec_float_50 Account::getTotal() const
{
    boost::multiprecision::cpp_dec_float_50 total{ 0.00 };
    for(const std::pair<const unsigned int, Transaction>& pair : m_transactions)
    {
        if(pair.second.getType() == TransactionType::Income)
        {
            total += pair.second.getAmount();
        }
        else
        {
            total -= pair.second.getAmount();
        }
    }
    return total;
}

bool Account::backup(const std::string& backupPath)
{
    if(backupPath == m_path || !std::filesystem::exists(backupPath))
    {
        return false;
    }
    m_db->backup(backupPath.c_str(), SQLite::Database::BackupType::Save);
    return true;
}

bool Account::restore(const std::string& restorePath)
{
    if(restorePath == m_path || !std::filesystem::exists(restorePath))
    {
        return false;
    }
    m_db->backup(restorePath.c_str(), SQLite::Database::BackupType::Load);
    m_transactions.clear();
    SQLite::Statement qryGetAll{ *m_db, "SELECT * FROM transactions" };
    while(qryGetAll.executeStep())
    {
        Transaction transaction(qryGetAll.getColumn(0).getInt());
        transaction.setDate(boost::gregorian::from_string(qryGetAll.getColumn(1).getString()));
        transaction.setDescription(qryGetAll.getColumn(2).getString());
        transaction.setType(static_cast<TransactionType>(qryGetAll.getColumn(3).getInt()));
        transaction.setAmount(boost::multiprecision::cpp_dec_float_50(qryGetAll.getColumn(5).getString()));
        m_transactions.insert({ transaction.getId(), transaction });
    }
    return true;
}


