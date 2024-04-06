#ifndef CONFIGURATION_H
#define CONFIGURATION_H

#include <string>
#include <vector>
#include <libnick/app/configurationbase.h>
#include "color.h"
#include "insertseparatortrigger.h"
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
         * @return The list of valid RecentAccount objects
         */
        std::vector<RecentAccount> getRecentAccounts();
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
         * @return The default color for transactions 
         */
        Color getTransactionDefaultColor() const;
        /**
         * @brief Sets the default color for transactions.
         * @param color The new default color
         */
        void setTransactionDefaultColor(const Color& color);
        /**
         * @brief Gets the default color for transfers.
         * @return The default color for transfers 
         */
        Color getTransferDefaultColor() const;
        /**
         * @brief Sets the default color for transfers.
         * @param color The new default color
         */
        void setTransferDefaultColor(const Color& color);
        /**
         * @brief Gets the default color for groups.
         * @return The default color for groups 
         */
        Color getGroupDefaultColor() const;
        /**
         * @brief Sets the default color for groups.
         * @param color The new default color
         */
        void setGroupDefaultColor(const Color& color);
        /**
         * @brief Gets the trigger for inserting decimal separators.
         * @return InsertSeparatorTrigger
         */
        InsertSeparatorTrigger getInsertSeparator() const;
        /**
         * @brief Sets the trigger for inserting decimal separators.
         * @param separator InsertSeparatorTrigger
         */
        void setInsertSeparator(InsertSeparatorTrigger separator);

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