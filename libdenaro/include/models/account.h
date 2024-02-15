#ifndef ACCOUNT_H
#define ACCOUNT_H

#include <cstdint>
#include <filesystem>
#include <optional>
#include <string>
#include <tuple>
#include <unordered_map>
#include <vector>
#include <libnick/database/sqldatabase.h>
#include "accountmetadata.h"
#include "color.h"
#include "exportmode.h"
#include "graphtype.h"
#include "group.h"
#include "importresult.h"
#include "transaction.h"
#include "transfer.h"

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief A model of a Denaro account. 
     */
    class Account
    {
    public:
        /**
         * @brief Constructs an Account.
         * @param path The path to the .nmoney file 
         */
        Account(std::filesystem::path path);
        /**
         * @brief Gets whether or not the account file is encrypted.
         * @return True if encrypted, else false 
         */
        bool isEncrypted() const;
        /**
         * @brief Logins to the account.
         * @brief This method also loads the account's metadata into memory.
         * @param password The password for the account. If unencrypted, this param must be an empty string
         * @return True if successful, else false
         */
        bool login(const std::string& password);
        /**
         * @brief Gets the metadata for the account.
         * @return The account metadata 
         */
        const AccountMetadata& getMetadata() const;
        /**
         * @brief Sets the metadata for the account.
         * @param metadata The new account metadata 
         */
        void setMetadata(const AccountMetadata& metadata);
        /**
         * @brief Gets the groups for the account.
         * @return The account's groups 
         */
        const std::unordered_map<unsigned int, Group>& getGroups() const;
        /**
         * @brief Gets the tags for the account.
         * @return The account's tags 
         */
        const std::vector<std::string>& getTags() const;
        /**
         * @brief Gets the transactions for the account.
         * @return The account's transactions 
         */
        const std::unordered_map<unsigned int, Transaction>& getTransactions() const;
        /**
         * @brief Gets the transaction reminders for the account.
         * @brief Each reminder is a tuple of the following: (std::string TransactionDescription, double TransactionAmount, std::string When)
         * @return The account's transaction reminders
         */
        std::vector<std::tuple<std::string, double, std::string>> getTransactionReminders() const;
        /**
         * @brief Gets the next available group id.
         * @return The next available group id 
         */
        unsigned int getNextAvailableGroupId() const;
        /**
         * @brief Gets the next available transaction id.
         * @return The next available transaction id 
         */
        unsigned int getNextAvailableTransactionId() const;
        /**
         * @brief Changes the password of the account.
         * @param password new password for the account. If removing a password from the account,  
         */
        void changePassword(const std::string& password);
        /**
         * @brief Gets the total income amount for the transactions given.
         * @param transactionIds The ids of transactions to consider
         * @return The total income amount 
         */
        double getIncome(const std::vector<unsigned int>& transactionIds = {}) const;
        /**
         * @brief Gets the total expense amount for the transactions given.
         * @param transactionIds The ids of transactions to consider
         * @return The total expense amount 
         */
        double getExpense(const std::vector<unsigned int>& transactionIds = {}) const;
        /**
         * @brief Gets the total amount for the transactions given.
         * @param transactionIds The ids of transactions to consider
         * @return The total amount 
         */
        double getTotal(const std::vector<unsigned int>& transactionIds = {}) const;
        /**
         * @brief Gets the total income amount for the transactions given for a Group.
         * @param group The group to consider
         * @param transactionIds The ids of transactions to consider
         * @return The total income amount 
         */
        double getGroupIncome(const Group& group, const std::vector<unsigned int>& transactionIds = {}) const;
        /**
         * @brief Gets the total expense amount for the transactions given for a Group.
         * @param group The group to consider
         * @param transactionIds The ids of transactions to consider
         * @return The total expense amount 
         */
        double getGroupExpense(const Group& group, const std::vector<unsigned int>& transactionIds = {}) const;
        /**
         * @brief Gets the total amount for the transactions given for a Group.
         * @param group The group to consider
         * @param transactionIds The ids of transactions to consider
         * @return The total amount 
         */
        double getGroupTotal(const Group& group, const std::vector<unsigned int>& transactionIds = {}) const;
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
         * @param id The id of the group to remove
         * @return A pair with a boolean representing true if deletion was successful (else false) and a vector of ids of transactions that belonged to said group
         */
        std::pair<bool, std::vector<unsigned int>> deleteGroup(unsigned int id);
        /**
         * @brief Adds a transaction to the account
         * @param transaction The transaction to add
         * @return A pair with a boolean representing true if addition was successful (else false) and a vector of strings of new tags
         */
        std::pair<bool, std::vector<std::string>> addTransaction(const Transaction& transaction);
        /**
         * @brief Updates a transaction in the account
         * @param transaction The transaction to update
         * @return A pair with a boolean representing true if update was successful (else false) and a vector of strings of new tags
         */
        std::pair<bool, std::vector<std::string>> updateTransaction(const Transaction& transaction);
        /**
         * @brief Updates a source transaction in the account
         * @param transaction The transaction to update
         * @param updateGenerated Whether or not to update generated transactions associated with the source
         * @return A pair with a boolean representing true if update was successful (else false) and a vector of strings of new tags
         */
        std::pair<bool, std::vector<std::string>> updateSourceTransaction(const Transaction& transaction, bool updateGenerated);
        /**
         * @brief Deletes a transaction from the account.
         * @param id The id of the transaction to delete
         * @return True if successful, else false
         */
        bool deleteTransaction(unsigned int id);
        /**
         * @brief Deletes a source transaction from the account.
         * @param id The id of the transaction to delete
         * @param deleteGenerated Whether or not to delete generated transactions associated with the source
         * @return True if successful, else false
         */
        bool deleteSourceTransaction(unsigned int id, bool deleteGenerated);
        /**
         * @brief Deletes generated repeat transactions from the account.
         * @param sourceId The id of the source transaction 
         * @return True if successful, else false
         */
        bool deleteGeneratedTransactions(unsigned int sourceId);
        /**
         * @brief Syncs repeat transactions in the account.
         * @return True if transactions were modified, else false 
         */
        bool syncRepeatTransactions();
        /**
         * @brief Sends a transfer to another account and creates an expense transaction for this account with the transfer.
         * @param transfer The transfer to send
         * @param color The color to use for the transfer transaction
         * @return The new expense transaction if successful, else std::nullopt
         */
        std::optional<Transaction> sendTransfer(const Transfer& transfer, const Color& color);
        /**
         * @brief Receives a transfer from another account and creates an income transaction for this account with the transfer.
         * @param transfer The transfer to receive
         * @param color The color to use for the transfer transaction
         * @return The new income transaction if successful, else std::nullopt
         */
        std::optional<Transaction> receiveTransfer(const Transfer& transfer, const Color& color);
        /**
         * @brief Imports transactions from a file.
         * @param path The path to the file to import
         * @param defaultTransactionColor The default color for transactions
         * @param defaultGroupColor The default color for groups
         * @return ImportResult 
         */
        ImportResult importFromFile(const std::filesystem::path& path, const Color& defaultTransactionColor, const Color& defaultGroupColor);
        /**
         * @brief Exports the account to a CSV file.
         * @param path THe path to the CSV file
         * @param exportMode ExportMode
         * @param filetedIds The list of transaction ids to use for ExportMode::CurrentView
         */
        bool exportToCSV(const std::filesystem::path& path, ExportMode exportMode = ExportMode::All, std::vector<unsigned int> filteredIds = {});
        /**
         * @brief Exports the account to a PDF file.
         * @param path The path to the PDF file
         * @param password The password to use to lock the PDF file. Empty string for no password on the PDF file
         * @param exportMode ExportMode
         * @param filetedIds The list of transaction ids to use for ExportMode::CurrentView
         */
        bool exportToCSV(const std::filesystem::path& path, const std::string& password, ExportMode exportMode = ExportMode::All, std::vector<unsigned int> filteredIds = {});
        /**
         * @brief Generates a graph.
         * @param type GraphType
         * @param darkMode Whether or not to draw the graph in dark mode
         * @param filteredIds The list of transaction ids to use in generating the graph
         * @param width The width of the graph
         * @param height The height of the graph
         * @param showLegend Whether or not to show the graph's legend
         * @return The byte vector of the graph image
         */
        std::vector<std::uint8_t> generateGraph(GraphType type, bool darkMode, std::vector<unsigned int> filteredIds, int width = -1, int height = -1, bool showLegend = true);
        /**
         * @brief Gets whether or not the object is valid or not.
         * @return True if valid (m_loggedIn), else false 
         */
        operator bool() const;

    private:
        /**
         * Populates the m_transactionReminders list.
         */
        void calculateTransactionReminders();
        /**
         * @brief Imports transactions from a CSV file.
         * @param path The path to the file to import
         * @param defaultTransactionColor The default color for transactions
         * @param defaultGroupColor The default color for groups
         * @return ImportResult 
         */
        ImportResult importFromCSV(const std::filesystem::path& path, const Color& defaultTransactionColor, const Color& defaultGroupColor);
        /**
         * @brief Imports transactions from an OFX file.
         * @param path The path to the file to import
         * @param defaultTransactionColor The default color for transactions
         * @return ImportResult 
         */
        ImportResult importFromOFX(const std::filesystem::path& path, const Color& defaultTransactionColor);
        /**
         * @brief Imports transactions from a CSV file.
         * @param path The path to the file to import
         * @param defaultTransactionColor The default color for transactions
         * @param defaultGroupColor The default color for groups
         * @return ImportResult 
         */
        ImportResult importFromQIF(const std::filesystem::path& path, const Color& defaultTransactionColor, const Color& defaultGroupColor);
        std::filesystem::path m_path;
        bool m_loggedIn;
        mutable Database::SqlDatabase m_database;
        AccountMetadata m_metadata;
        std::unordered_map<unsigned int, Group> m_groups;
        std::vector<std::string> m_tags;
        std::unordered_map<unsigned int, Transaction> m_transactions;
    };
}

#endif //ACCOUNT_H