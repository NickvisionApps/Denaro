#ifndef ACCOUNTMETADATA_H
#define ACCOUNTMETADATA_H

#include <string>
#include "accounttype.h"
#include "customcurrency.h"
#include "remindersthreshold.h"
#include "sortby.h"
#include "transactiontype.h"

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief A model of metadata for an account. 
     */
    class AccountMetadata
    {
    public:
        /**
         * @brief Constructs an AccountMetadata.
         * @param name The name of the account
         * @param type The type of the account 
         */
        AccountMetadata(const std::string& name, AccountType type);
        /**
         * @brief Gets the name of the account.
         * @return The name of the account 
         */
        const std::string& getName() const;
        /**
         * @brief Sets the name of the account.
         * @param name The new name of the account 
         */
        void setName(const std::string& name);
        /**
         * @brief Gets the type of the account.
         * @return The type of the account 
         */
        AccountType getType() const;
        /**
         * @brief Sets the type of the account.
         * @param type The new type of the account 
         */
        void setType(AccountType type);
        /**
         * @brief Gets whether or not to use the custom currency provided by the metadata.
         * @return True to use the custom currency, else false
         */
        bool getUseCustomCurrency() const;
        /**
         * @brief Sets whether or not to use the custom currency provided by the metadata.
         * @param useCustomCurrency True to use the custom currency, else false
         */
        void setUseCustomCurrency(bool useCustomCurrency);
        /**
         * @brief Gets the custom currency of the account.
         * @brief getUseCustomCurrency() should be checked first to determine whether or not to utilize this custom currency.
         * @return The custom currency of the account
         */
        const CustomCurrency& getCustomCurrency() const;
        /**
         * @brief Sets the custom currency of the account.
         * @param customCurrency The new custom currency of the account 
         */
        void setCustomCurrency(const CustomCurrency& customCurrency);
        /**
         * @brief Gets the default transaction type of the account.
         * @return The default transaction type of the account 
         */
        TransactionType getDefaultTransactionType() const;
        /**
         * @brief Sets the default transaction type of the account.
         * @param defaultTransactionType The new default transaction type of the account 
         */
        void setDefaultTransactionType(TransactionType defaultTransactionType);
        /**
         * @brief Gets the threshold for showing transaction reminders of the account.
         * @return The threshold for showing transaction reminders of the account 
         */
        RemindersThreshold getTransactionRemindersThreshold() const;
        /**
         * @brief Sets the threshold for showing transaction reminders of the account.
         * @param remindersThreshold The new threshold for showing transaction reminders of the account 
         */
        void setTransactionRemindersThreshold(RemindersThreshold remindersThreshold);
        /**
         * @brief Gets whether or not to show the groups list on the account view.
         * @return True to show groups list, else false 
         */
        bool getShowGroupsList() const;
        /**
         * @brief Sets whether or not to show the groups list on the account view.
         * @param showGroupsList True to show groups list, else false 
         */
        void setShowGroupsList(bool showGroupsList);
        /**
         * @brief Gets whether or not to show the tags list on the account view.
         * @return True to show tags list, else false 
         */
        bool getShowTagsList() const;
        /**
         * @brief Sets whether or not to show the tags list on the account view.
         * @param showTagsList True to show tags list, else false 
         */
        void setShowTagsList(bool showTagsList);
        /**
         * @brief Gets the way in which to sort transactions on the account view.
         * @return The SortBy value 
         */
        SortBy getSortTransactionsBy() const;
        /**
         * @brief Sets the way in which to sort transactions on the account view.
         * @param sortTransactionBy The new SortBy value 
         */
        void setSortTransactionsBy(SortBy sortTransactionsBy);
        /**
         * @brief Gets whether or not to sort transactions from first to last on the account view.
         * @return True to sort first to last, else false 
         */
        bool getSortFirstToLast() const;
        /**
         * @brief Sets whether or not to sort transactions from first to last on the account view.
         * @param sortFirstToLast True to sort first to last, else false 
         */
        void setSortFirstToLast(bool sortFirstToLast);

    private:
        std::string m_name;
        AccountType m_type;
        bool m_useCustomCurrency;
        CustomCurrency m_customCurrency;
        TransactionType m_defaultTransactionType;
        RemindersThreshold m_transactionRemindersThreshold;
        bool m_showGroupsList;
        bool m_showTagsList;
        SortBy m_sortTransactionsBy;
        bool m_sortFirstToLast;
    };
}

#endif //ACCOUNTMETADATA_H