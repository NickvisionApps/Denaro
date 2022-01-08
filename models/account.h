#ifndef ACCOUNT_H
#define ACCOUNT_H

#include <string>
#include <vector>
#include <optional>
#include "SQLiteCpp/SQLiteCpp.h"
#include "transaction.h"

namespace NickvisionMoney::Models
{
    class Account
    {
    public:
        Account(const std::string& path);
        std::vector<Transaction> getTransactions();
        std::optional<Transaction> getTransactionByID(int id);
        bool addTransaction(const Transaction& transaction);
        bool updateTransaction(const Transaction& transaction);
        bool deleteTransaction(int id);
        double getIncome();
        double getExpense();
        double getTotal();
        void backup(const std::string& backupPath);
        void restore(const std::string& restorePath);

    private:
        std::string m_path;
        SQLite::Database m_db;
    };
}

#endif // ACCOUNT_H
