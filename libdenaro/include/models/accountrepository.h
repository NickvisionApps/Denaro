#ifndef ACCOUNTREPOSITORY_H
#define ACCOUNTREPOSITORY_H

#include <filesystem>
#include <string>
#include <unordered_map>
#include <vector>
#include <libnick/database/sqldatabase.h>
#include "accountmetadata.h"
#include "group.h"
#include "transaction.h"

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief Repository (SQLite database) for the account data
     */
    class AccountRepository
    {
    public:
        /**
         * @brief Constructs an AccountRepository.
         * @param path The path to the repository file 
         */
        AccountRepository(const std::filesystem::path& path);
        /**
         * @brief Gets whether or not the account repository is encrypted.
         * @return True if encrypted, else false 
         */
        bool isEncrypted() const;
        /**
         * @brief Logins in to the account repository.
         * @param password The password to unlock the repository
         * @return True if successful, else false
         */
        bool login(const std::string& password);
        /**
         * @brief Changes the password of the account repository.
         * @param password new password for the account. If removing a password from the account
         * @return True if password changed successfully, else false
         */
        bool changePassword(const std::string& password);
        /**
         * @brief Begins a transaction in the repository.
         * @return True if successful, else false (false means a transaction is already in progress, run commitTransaction() first)
         */
        bool beginTransaction();
        /**
         * @brief Commits the transaction in the repository.
         * @return True if successful, else false (false means no transaction in progress, run beginTransaction() first)
         */
        bool commitTransaction();
        /**
         * @brief Fetches the account metadata from the repository.
         * @return The account metadata
         */
        AccountMetadata getMetadata() const;
        /**
         * @brief Sets the account metadata in the repository.
         * @param metadata The new account metadata
         */
        void setMetadata(const AccountMetadata& metadata);
        /**
         * @brief Fetches the groups from the repository.
         * @return The groups map
         */
        std::unordered_map<int, Group> getGroups() const;
        /**
         * @brief Adds a group to the account.
         * @param group The group to add
         * @return True if successful, else false 
         */
        bool addGroup(const Group& group);
        /**
         * @brief Updates a group in the account.
         * @param group The group to update
         * @return True if successful, else false 
         */
        bool updateGroup(const Group& group);
        /**
         * @brief Deletes a group from the account.
         * @param group The group to update
         * @return True if successful, else false 
         */
        bool deleteGroup(const Group& group);
        /**
         * @brief Fetches the tags from the repository.
         * @return The tags vector
         */
        std::vector<std::string> getTags() const;
        /**
         * @brief Fetches the transactions from the repository.
         * @return The transactions map
         */
        std::unordered_map<int, Transaction> getTransactions() const;
        /**
         * @brief Fetches the future transactions from the repository before a certain maximum date.
         * @brief The returned list is sorted by upcoming date.
         * @param max The maximum date threshold for future transactions
         * @return The future transactions before the threshold
         */
        std::vector<Transaction> getFutureTransactions(const boost::gregorian::date& max) const;
        /**
         * @brief Adds a transaction to the account
         * @param transaction The transaction to add
         * @return True if successful, else false
         */
        bool addTransaction(const Transaction& transaction);
        /**
         * @brief Updates a transaction in the account
         * @param transaction The transaction to update
         * @param updateGenerated Whether or not to update generated transactions associated with this transaction if it is a source transaction
         * @return True if successful, else false
         */
        bool updateTransaction(const Transaction& transaction, bool updateGenerated = true);
        /**
         * @brief Deletes a transaction from the account.
         * @param transaction The transaction to delete
         * @param deleteGenerated Whether or not to delete generated transactions associated with this transaction if it is a source transaction
         * @return True if successful, else false
         */
        bool deleteTransaction(const Transaction& transaction, bool deleteGenerated = true);
        /**
         * @brief Deletes generated repeat transactions from the account.
         * @param sourceId The id of the source transaction 
         * @return True if successful, else false
         */
        bool deleteGeneratedTransactions(int sourceId);
    
    private:
        mutable Database::SqlDatabase m_database;
        bool m_transactionInProcess;
    };
}

#endif //ACCOUNTREPOSITORY_H