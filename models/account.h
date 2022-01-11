#ifndef ACCOUNT_H
#define ACCOUNT_H

#include <string>
#include <map>
#include <optional>
#include "SQLiteCpp/SQLiteCpp.h"
#include "transaction.h"

namespace NickvisionMoney::Models
{
    class Account
    {
    public:
        Account(const std::string& path);
        const std::string& getPath() const;
        const std::map<unsigned int, Transaction>& getTransactions() const;
        std::optional<Transaction> getTransactionByID(unsigned int id) const;
        int getNextID() const;
        bool addTransaction(const Transaction& transaction);
        bool updateTransaction(const Transaction& transaction);
        bool deleteTransaction(unsigned int id);
        double getIncome() const;
        std::string getIncomeAsString() const;
        double getExpense() const;
        std::string getExpenseAsString() const;
        double getTotal() const;
        std::string getTotalAsString() const;
        void backup(const std::string& backupPath);
        void restore(const std::string& restorePath);

    private:
        std::string m_path;
        SQLite::Database m_db;
        std::map<unsigned int, Transaction> m_transactions;
    };
}

#endif // ACCOUNT_H
