#ifndef ACCOUNTVIEWCONTROLLER_H
#define ACCOUNTVIEWCONTROLLER_H

#include <filesystem>
#include <memory>
#include <string>
#include <utility>
#include <vector>
#include "models/account.h"
#include "models/recentaccount.h"

namespace Nickvision::Money::Shared::Controllers
{
    /**
     * @brief A controller for an AccountView. 
     */
    class AccountViewController
    {
    public:
        /**
         * @brief Constructs an AccountViewController.
         * @param path The path to the account file
         * @param password The password for the account
         * @throws std::runtime_error Thrown if the account cannot be logged into.
         */
        AccountViewController(const std::filesystem::path& path, const std::string& password);
        /**
         * @brief Gets the path of the account database file.
         * @return The path of the account database file
         */
        const std::filesystem::path& getPath() const;
        /**
         * @brief Gets the metadata for the account. 
         * @return The account metadata
         */
        const Models::AccountMetadata& getMetadata() const;
        /**
         * @brief Gets the RecentAccount representation of this account.
         * @return The RecentAccount representation of this account. 
         */
        Models::RecentAccount toRecentAccount() const;
        /**
         * @brief Gets the total amount of the account as a string.
         * @return The total amount string
         */
        std::string getTotalAmountString() const;
        /**
         * @brief Gets the income amount of the account as a string.
         * @return The income amount string
         */
        std::string getIncomeAmountString() const;
        /**
         * @brief Gets the expense amount of the account as a string.
         * @return The expense amount string
         */
        std::string getExpenseAmountString() const;
        /**
         * @brief Gets the transaction reminders for the account.
         * @return The transaction reminders
         */
        std::vector<Models::TransactionReminder> getTransactionReminders() const;
        /**
         * @brief Gets the groups of the account.
         * @return (Group GroupName, std::string BalanceString)
         */
        std::vector<std::pair<Models::Group, std::string>> getGroups() const;

    private:
        std::unique_ptr<Models::Account> m_account;
    };
}

#endif //ACCOUNTVIEWCONTROLLER_H
