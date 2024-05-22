#ifndef ACCOUNTSETTINGSDIALOGCONTROLLER_H
#define ACCOUNTSETTINGSDIALOGCONTROLLER_H

#include <memory>
#include <string>
#include "models/account.h"
#include "models/currencycheckstatus.h"

namespace Nickvision::Money::Shared::Controllers
{
    /**
     * @brief A controller for an AccountSettingsDialog.
     */
    class AccountSettingsDialogController
    {
    public:
        /**
         * @brief Constructs an AccountSettingsDialogController.
         * @param account The Account model
         */
        AccountSettingsDialogController(const std::shared_ptr<Models::Account>& account);
        /**
         * @brief Gets the metadata of the new account.
         * @return The metadata of the new account
         */
        const Models::AccountMetadata& getMetadata() const;
        /**
         * @brief Gets the string describing the system's reported currency.
         * @return The reported currency string
         */
        std::string getReportedCurrencyString() const;
        /**
         * @brief Gets whether or not the account is encrypted (a.k.a password-protected).
         * @return True if encrypted, else false
         */
        bool isEncrypted() const;
        /**
         * @brief Sets the name of the account.
         * @param name The new name of the account
         * @return True if the name was set, else false (example, the name was empty)
         */
        bool setName(const std::string& name);
         /**
         * @brief Sets a password for the account.
         * @param password The new password
         * @return True if the password was changed, else false
         */
        bool setPassword(const std::string& password);
        /**
         * @brief Sets the account type of the account.
         * @param accountType The new account type of the account
         */
        void setAccountType(Models::AccountType accountType);
        /**
         * @brief Sets the default transaction type of the account.
         * @param defaultTransactionType The new default transaction type of the account
         */
        void setDefaultTransactionType(Models::TransactionType defaultTransactionType);
        /**
         * @brief Sets the transaction reminders threshold of the new account.
         * @param transactionReminderThreshold The new transaction reminders threshold of the new account
         */
        void setTransactionRemindersThreshold(Models::RemindersThreshold transactionReminderThreshold);
        /**
         * @brief Turns off the custom currency for the account
         */
        void setCustomCurrencyOff();
         /**
         * @brief Sets the custom currency of the account.
         * @param symbol The symbol of the new custom currency
         * @param code The code of the new custom currency
         * @param decimalSeparator The decimal separator of the new custom currency
         * @param groupSeparator The group separator of the new custom currency
         * @param decimalDigits The decimal digits of the new custom currency
         * @param amountStyle The amount style of the new custom currency
         * @return CurrencyCheckStatus
         */
        Models::CurrencyCheckStatus setCustomCurrency(const std::string& symbol, const std::string& code, char decimalSeparator, char groupSeparator, int decimalDigits, Models::AmountStyle amountStyle);

    private:
        std::shared_ptr<Models::Account> m_account;
    };
}

#endif //ACCOUNTSETTINGSDIALOGCONTROLLER_H
