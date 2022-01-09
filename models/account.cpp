#include "account.h"
#include <stdexcept>

namespace NickvisionMoney::Models
{
    Account::Account(const std::string& path) : m_path(path), m_db(m_path, SQLite::OPEN_READWRITE | SQLite::OPEN_CREATE)
    {
        m_db.exec("CREATE TABLE IF NOT EXISTS transactions (id INTEGER PRIMARY KEY, date TEXT, description TEXT, type INTEGER, amount REAL)");
        SQLite::Statement qryGetAll(m_db, "SELECT * FROM transactions");
        while(qryGetAll.executeStep())
        {
            Transaction transaction(qryGetAll.getColumn(0).getInt());
            transaction.setDate(qryGetAll.getColumn(1).getString());
            transaction.setDescription(qryGetAll.getColumn(2).getString());
            transaction.setType(static_cast<TransactionType>(qryGetAll.getColumn(3).getInt()));
            transaction.setAmount(qryGetAll.getColumn(4).getDouble());
            m_transactions.push_back(transaction);
        }
    }

    const std::vector<Transaction>& Account::getTransactions() const
    {
        return m_transactions;
    }

    std::optional<Transaction> Account::getTransactionByID(int id) const
    {
        try
        {
            return m_transactions[id - 1];
        }
        catch (...)
        {
            return std::nullopt;
        }
    }

    int Account::getNextID() const
    {
        try
        {
            return m_transactions[m_transactions.size() - 1].getID() + 1;
        }
        catch (...)
        {
            return 1;
        }
    }

    bool Account::addTransaction(const Transaction& transaction)
    {
        SQLite::Statement qryInsert(m_db, "INSERT INTO transactions (id, date, description, type, amount) VALUES (?, ?, ?, ?, ?)");
        qryInsert.bind(1, transaction.getID());
        qryInsert.bind(2, transaction.getDate());
        qryInsert.bind(3, transaction.getDescription());
        qryInsert.bind(4, static_cast<int>(transaction.getType()));
        qryInsert.bind(5, transaction.getAmount());
        if(qryInsert.exec() > 0)
        {
            m_transactions.push_back(transaction);
            return true;
        }
        return false;
    }

    bool Account::updateTransaction(const Transaction& transaction)
    {
        SQLite::Statement qryUpdate(m_db, "UPDATE transactions SET date = ?, description = ?, type = ?, amount = ? WHERE id = " + std::to_string(transaction.getID()));
        qryUpdate.bind(1, transaction.getDate());
        qryUpdate.bind(2, transaction.getDescription());
        qryUpdate.bind(3, static_cast<int>(transaction.getType()));
        qryUpdate.bind(4, transaction.getAmount());
        if(qryUpdate.exec() > 0)
        {
            m_transactions[transaction.getID() - 1] = transaction;
            return true;
        }
        return false;
    }

    bool Account::deleteTransaction(int id)
    {
       if(m_db.exec("DELETE FROM transactions WHERE id = " + std::to_string(id)) > 0)
       {
           m_transactions.erase(m_transactions.begin() + (id - 1));
           return true;
       }
       return false;
    }

    double Account::getIncome()
    {
        double income = 0.00;
        for(const Transaction& transaction : m_transactions)
        {
            if(transaction.getType() == TransactionType::Income)
            {
                income += transaction.getAmount();
            }
        }
        return income;
    }

    double Account::getExpense()
    {
        double expense = 0.00;
        for(const Transaction& transaction : m_transactions)
        {
            if(transaction.getType() == TransactionType::Expense)
            {
                expense += transaction.getAmount();
            }
        }
        return expense;
    }

    double Account::getTotal()
    {
        double total = 0.00;
        for(const Transaction& transaction : m_transactions)
        {
            if(transaction.getType() == TransactionType::Income)
            {
                total += transaction.getAmount();
            }
            else
            {
                total -= transaction.getAmount();
            }
        }
        return total;
    }

    void Account::backup(const std::string& backupPath)
    {
        if(backupPath == m_path)
        {
            throw std::invalid_argument("Backup path can not be the same as the open database's path.");
        }
        m_db.backup(backupPath.c_str(), SQLite::Database::BackupType::Save);
    }

    void Account::restore(const std::string& restorePath)
    {
        if(restorePath == m_path)
        {
            throw std::invalid_argument("Restore path can not be the same as the open database's path.");
        }
        m_db.backup(restorePath.c_str(), SQLite::Database::BackupType::Load);
    }
}
