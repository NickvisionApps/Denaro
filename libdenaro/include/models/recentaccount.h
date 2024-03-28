#ifndef RECENTACCOUNT_H
#define RECENTACCOUNT_H

#include <filesystem>
#include <string>
#include "accounttype.h"

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief A model of a recent account. 
     */
    class RecentAccount
    {
    public:
        /**
         * @brief Constructs a RecentAccount.
         */
        RecentAccount();
        /**
         * @brief Constructs a RecentAccount.
         * @param path The path of the recent account
         */
        RecentAccount(const std::filesystem::path& path);
        /**
         * @brief Gets the path of the recent account.
         * @return The path of the recent account 
         */
        const std::filesystem::path& getPath() const;
        /**
         * @brief Sets the path of the recent account.
         * @param path The new path of the recent account 
         */
        void setPath(const std::filesystem::path& path);
        /**
         * @brief Gets the name of the recent account.
         * @return The name of the recent account 
         */
        const std::string& getName() const;
        /**
         * @brief Sets the name of the recent account.
         * @param name The new name of the recent account 
         */
        void setName(const std::string& name);
        /**
         * @brief Gets the type of the recent account.
         * @return The type of the recent account 
         */
        AccountType getType() const;
        /**
         * @brief Sets the type of the recent account.
         * @param type The new type of the recent account 
         */
        void setType(AccountType type);
        /**
         * @brief Gets whether or not the object represents an empty RecentAccount.
         * @brief An empty RecentAccount is one that has no path or one whose path does not exist.
         * @return True if empty, else false
         */
        bool empty() const;
        /**
         * @brief Gets whether or not this RecentAccount is equal to compare RecentAccount.
         * @param compare The RecentAccount to compare to
         * @return True if this RecentAccount == compare RecentAccount 
         */
        bool operator==(const RecentAccount& compare) const;
        /**
         * @brief Gets whether or not this RecentAccount is not equal to compare RecentAccount.
         * @param compare The RecentAccount to compare to
         * @return True if this RecentAccount != compare RecentAccount 
         */
        bool operator!=(const RecentAccount& compare) const;
        /**
         * @brief Gets whether or not the object is valid or not.
         * @return True if valid (!empty), else false 
         */
        operator bool() const;

    private:
        std::filesystem::path m_path;
        std::string m_name;
        AccountType m_type;
    };
}

#endif //RECENTACCOUNT_H