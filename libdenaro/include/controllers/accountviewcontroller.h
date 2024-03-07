#ifndef ACCOUNTVIEWCONTROLLER_H
#define ACCOUNTVIEWCONTROLLER_H

#include <filesystem>
#include <memory>
#include <string>
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
        const Models::AccountMetadata getMetadata() const;
        /**
         * @brief Gets the RecentAccount representation of this account.
         * @return The RecentAccount representation of this account. 
         */
        Models::RecentAccount toRecentAccount() const;

    private:
        std::shared_ptr<Models::Account> m_account;
    };
}

#endif //ACCOUNTVIEWCONTROLLER_H