#ifndef ACCOUNTSETTINGSDIALOGCONTROLLER_H
#define ACCOUNTSETTINGSDIALOGCONTROLLER_H

#include <memory>
#include "models/account.h"

namespace Nickvision::Money::Shared::Controllers
{
    class AccountSettingsDialogController
    {
    public:
        AccountSettingsDialogController(const std::shared_ptr<Models::Account>& account);
        /**
         * @brief Sets the name of the account.
         * @param name The new name of the account
         * @return True if the name was set, else false (example, the name was empty)
         */
        bool setName(const std::string& name);
         /**
         * @brief Sets a password for the account.
         * @param password The new password
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
