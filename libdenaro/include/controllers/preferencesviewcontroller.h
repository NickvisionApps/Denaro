#ifndef PREFERENCESVIEWCONTROLLER_H
#define PREFERENCESVIEWCONTROLLER_H

#include <string>
#include "models/color.h"
#include "models/insertseparatortrigger.h"
#include "models/theme.h"

namespace Nickvision::Money::Shared::Controllers
{
    /**
     * @brief A controller for a PreferencesView.
     */
    class PreferencesViewController
    {
    public:
        /**
         * @brief Constructs a PreferencesViewController.
         */
        PreferencesViewController() = default;
        /**
         * @brief Gets the application's id.
         * @return The app id
         */
        const std::string& getId() const;
        /**
         * @brief Gets the preferred theme for the application.
         * @return The preferred theme
         */
        Models::Theme getTheme() const;
        /**
         * @brief Sets the preferred theme for the application.
         * @param theme The new preferred theme
         */
        void setTheme(Models::Theme theme);
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
         * @brief Gets the default M for transactions.
         * @return The default M for transactions 
         */
        Models::Color getTransactionDefaultColor() const;
        /**
         * @brief Sets the default color for transactions.
         * @param color The new default color
         */
        void setTransactionDefaultColor(const Models::Color& color);
        /**
         * @brief Gets the default color for transfers.
         * @return The default color for transfers 
         */
        Models::Color getTransferDefaultColor() const;
        /**
         * @brief Sets the default color for transfers.
         * @param color The new default color
         */
        void setTransferDefaultColor(const Models::Color& color);
        /**
         * @brief Gets the default color for groups.
         * @return The default color for groups 
         */
        Models::Color getGroupDefaultColor() const;
        /**
         * @brief Sets the default color for groups.
         * @param color The new default color
         */
        void setGroupDefaultColor(const Models::Color& color);
        /**
         * @brief Gets the trigger for inserting decimal separators.
         * @return InsertSeparatorTrigger
         */
        Models::InsertSeparatorTrigger getInsertSeparator() const;
        /**
         * @brief Sets the trigger for inserting decimal separators.
         * @param separator InsertSeparatorTrigger
         */
        void setInsertSeparator(Models::InsertSeparatorTrigger separator);
        /**
         * @brief Saves the current configuration to disk.
         */
        void saveConfiguration();
    };
}

#endif //PREFERENCESVIEWCONTROLLER_H