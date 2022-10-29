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
	/**
	 * A model of an account
	 */
	class Account
	{
	public:
		/**
		 * Constructs an account
		 *
		 * @param path The path to the account file on disk
		 */
		Account(const std::string& path);
		/**
		 * Gets the path of the account
		 *
		 * @returns The path of the account
		 */
        const std::string& getPath() const;
        /**
         * Gets a map of transactions in the account
         *
         * @returns The map of transaction in the account
         */
        const std::map<unsigned int, Transaction>& getTransactions() const;
        /**
         * Attempts to get a transaction from the account by id
         *
         * @param id The id of the transaction
         * @returns The transaction if found, else std::nullopt
         */
        std::optional<Transaction> getTransactionById(unsigned int id) const;
        /**
         * Gets the next available id in the account
         *
         * @returns The next available id in the account
         */
        unsigned int getNextAvailableId() const;
        /**
         * Adds a transaction to the account
         *
         * @param transaction The transaction to add
         * @returns True if successful, else false
         */
        bool addTransaction(const Transaction& transaction);
        /**
         * Updates a transaction in the account
         *
         * @param transaction The transaction to update
         * @returns True if successful, else false
         */
        bool updateTransaction(const Transaction& transaction);
        /**
         * Deletes a transaction in the account
         *
         * @param id The id of the transaction to delete
         * @returns True if successful, else false
         */
        bool deleteTransaction(unsigned int id);
        /**
         * Gets the income amount of the account
         *
         * @returns The income amount of the account
         */
        boost::multiprecision::cpp_dec_float_50 getIncome() const;
        /**
         * Gets the expense amount of the account
         *
         * @returns The expense amount of the account
         */
        boost::multiprecision::cpp_dec_float_50 getExpense() const;
        /**
         * Gets the total amount of the account
         *
         * @returns The total amount of the account
         */
        boost::multiprecision::cpp_dec_float_50 getTotal() const;
        /**
         * Exports the account as a CSV file
         *
         * @param path The path of the CSV file
         * @returns True if successful, else false
         */
        bool exportAsCSV(const std::string& path);

	private:
		std::string m_path;
        std::shared_ptr<SQLite::Database> m_db;
        std::map<unsigned int, Transaction> m_transactions;
	};
}