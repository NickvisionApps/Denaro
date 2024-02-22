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
         * @brief Gets the color for checking accounts.
         * @return The color for checking accounts 
         */
        Color getAccountCheckingColor() const;
        /**
         * @brief Sets the color for checking accounts.
         * @param color The new checking accounts color
         */
        void setAccountCheckingColor(const Color& color);
        /**
         * @brief Gets the color for savings accounts.
         * @return The color for savings accounts 
         */
        Color getAccountSavingsColor() const;
        /**
         * @brief Sets the color for savings accounts.
         * @param color The new savings accounts color
         */
        void setAccountSavingsColor(const Color& color);
        /**
         * @brief Gets the color for business accounts.
         * @return The color for business accounts 
         */
        Color getAccountBusinessColor() const;
        /**
         * @brief Sets the color for business accounts.
         * @param color The new business accounts color
         */
        void setAccountBusinessColor(const Color& color);
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
        /**
         * @brief Gets whether or not to show graphs by default.
         * @return True to show graphs by default, else false
         */
        bool getShowGraphs() const;
        /**
         * @brief Sets whether or not to show graphs by default.
         * @param showGraphs True to show graphs, else false 
         */
        void setShowGraphs(bool showGraphs);

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