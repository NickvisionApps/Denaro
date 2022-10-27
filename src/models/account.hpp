#pragma once

#include <map>
#include <memory>
#include <optional>
#include <string>
#include <boost/multiprecision/cpp_dec_float.hpp>
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
        boost::multiprecision::cpp_dec_float_50 getIncome() const;
        boost::multiprecision::cpp_dec_float_50 getExpense() const;
        boost::multiprecision::cpp_dec_float_50 getTotal() const;
        bool backup(const std::string& backupPath);
        bool restore(const std::string& restorePath);

	private:
		std::string m_path;
        std::shared_ptr<SQLite::Database> m_db;
        std::map<unsigned int, Transaction> m_transactions;
	};
}