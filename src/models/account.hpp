#pragma once

#include <optional>
#include <string>
#include <map>
#include <SQLiteCpp/SQLiteCpp.h>
#include "transaction.hpp"

namespace NickvisionMoney::Models
{
	class Account
	{
	public:
		Account(const std::string& path);
        const std::string& getPath() const;
        const std::map<unsigned int, Transaction>& getTransactions() const;
        std::optional<Transaction> getTransactionById(unsigned int id) const;
        unsigned int getNextAvailableId() const;
        bool addTransaction(const Transaction& transaction);
        bool updateTransaction(const Transaction& transaction);
        bool deleteTransaction(unsigned int id);
        double getIncome() const;
        double getExpense() const;
        double getTotal() const;
        bool backup(const std::string& backupPath);
        bool restore(const std::string& restorePath);

	private:
		std::string m_path;
        SQLite::Database m_db;
        std::map<unsigned int, Transaction> m_transactions;
	};
}