#ifndef CONFIGURATION_H
#define CONFIGURATION_H

#include <string>
#include <vector>
#include <libnick/app/configurationbase.h>
#include "insertseparator.h"
#include "recentaccount.h"
#include "theme.h"

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief A model for the configuration of the application.
     */
    class Configuration : public Nickvision::App::ConfigurationBase
    {
    public:
        /**
         * @brief Constructs a Configuration.
         * @param key The key to pass to the ConfigurationBase
         */
        Configuration(const std::string& key);
        /**
         * @brief Gets the preferred theme for the application.
         * @return The preferred theme
         */
        Theme getTheme() const;
        /**
         * @brief Sets the preferred theme for the application.
         * @param theme The new preferred theme
         */
        void setTheme(Theme theme);
        /**
         * @brief Gets whether or not to automatically check for application updates.
         * @return True to automatically check for updates, else false
         */
        bool getAutomaticallyCheckForUpdates() const;
        /**
         * @brief Sets whether or not to automatically check for application updates.
         * @param check Whether or not to automatically check for updates
         */
        void setAutomaticallyCheckForUpdates(bool check);
        /**
         * @brief Gets the list of valid RecentAccount objects.
         * @brief This method may reorder the recent account lists if a recent is invalid and save the Configuration object to disk.
         * @return The list of valid RecentAccount objects
         */
        std::vector<RecentAccount> getAndSyncRecentAccounts();
        /**
         * @brief Adds a new recent account to the list.
         * @param recent The recent account to add
         */
        void addRecentAccount(const RecentAccount& recent);
        /**
         * @brief Removes a recent account from the list.
         * @param recent The recent account to remove
         */
        void removeRecentAccount(const RecentAccount& recent);
        /**
         * @brief Gets the default color for transactions.
         * @brief In format: rgb(r, g, b)
         * @return The default color for transactions 
         */
        std::string getTransactionDefaultColor() const;
        /**
         * @brief Sets the default color for transactions.
         * @param transaction The new default color in the format: rgb(r, g, b) 
         */
        void setTransactionDefaultColor(const std::string& transaction);

    private:
        /**
         * @brief Gets a numbered recent account.
         * @param index The number of the recent account to get
         * @return The numbered RecentAccount
         */
        RecentAccount getRecentAccount(int index) const;
        /**
         * @brief Sets a numbered recent account.
         * @param index The number of the recent account
         * @param recent The RecentAccount object 
         */
        void setRecentAccount(int index, const RecentAccount& recent);
    };
}

#endif //CONFIGURATION_H