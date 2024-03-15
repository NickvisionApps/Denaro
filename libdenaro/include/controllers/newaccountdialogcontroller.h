#ifndef NEWACCOUNTDIALOGCONTROLLER_H
#define NEWACCOUNTDIALOGCONTROLLER_H

#include <filesystem>
#include <string>
#include <vector>
#include "models/accountmetadata.h"
#include "models/accounttype.h"
#include "models/amountstyle.h"
#include "models/currency.h"
#include "models/currencycheckstatus.h"
#include "models/remindersthreshold.h"
#include "models/transactiontype.h"

namespace Nickvision::Money::Shared::Controllers
{
    /**
     * @brief A controller for a NewAccountDialog. 
     */
    class NewAccountDialogController
    {
    public:
        /**
         * @brief Constructs a NewAccountDialogController.
         */
        NewAccountDialogController();
        /**
         * @brief Gets the application's id.
         * @return The application's id 
         */
        const std::string& getId() const;
        /**
         * @brief Gets the file path of the new account.
         * @return The file path of the new account
         */
        const std::filesystem::path& getFilePath() const;
        /**
         * @brief Gets the metadata of the new account.
         * @return The metadata of the new account
         */
        const Models::AccountMetadata& getMetadata() const;
        /**
         * @brief Gets the folder of the new account path.
         * @return The folder of the new account path
         */
        std::filesystem::path getFolder() const;
        /**
         * @brief Sets the folder of the new account path.
         * @param folder The folder of the new account path
         * @return True if the folder was set, else false (the new folder results in an existing account path and overwrite is off)
         */
        bool setFolder(const std::filesystem::path& folder);
        /**
         * @brief Sets the name of the new account.
         * @param name The new name of the new account
         * @return True if the name was set, else false (the new name results in an existing account path and overwrite is off)
         */
        bool setName(const std::string& name);
        /**
         * @brief Gets the password of the new account.
         * @return The password of the new account
         */
        const std::string& getPassword() const;
        /**
         * @brief Sets the password of the new account.
         * @param password The new password of the new account
         */
        void setPassword(const std::string& password);
        /**
         * @brief Gets whether or not to overwrite an existing account.
         * @return True to overwrite, else false
         */
        bool getOverwriteExisting() const;
        /**
         * @brief Sets whether or not to overwrite an existing account.
         * @param overwriteExisting True to overwrite, else false
         */
        void setOverwriteExisting(bool overwriteExisting);
        /**
         * @brief Sets the account type of the new account.
         * @param accountType The new account type of the new account
         */
        void setAccountType(Models::AccountType accountType);
        /**
         * @brief Sets the default transaction type of the new account.
         * @param defaultTransactionType The new default transaction type of the new account
         */
        void setDefaultTransactionType(Models::TransactionType defaultTransactionType);
        /**
         * @brief Sets the transaction reminders threshold of the new account.
         * @param transactionReminderThreshold The new transaction reminders threshold of the new account
         */
        void setTransactionRemindersThreshold(Models::RemindersThreshold transactionReminderThreshold);
        /**
         * @brief Sets off the custom currency on the new account. 
         */
        void setCustomCurrencyOff();
        /**
         * @brief Sets the custom currency of the new account.
         * @param symbol The symbol of the new custom currency
         * @param code The code of the new custom currency
         * @param decimalSeparator The decimal separator of the new custom currency
         * @param groupSeparator The group separator of the new custom currency
         * @param decimalDigits The decimal digits of the new custom currency
         * @param amountStyle The amount style of the new custom currency
         * @return CurrencyCheckStatus
         */
        Models::CurrencyCheckStatus setCustomCurrency(const std::string& symbol, const std::string& code, char decimalSeparator, char groupSeparator, int decimalDigits, Models::AmountStyle amountStyle);
        /**
         * @brief Gets the path of a file to import into the new account when created.
         * @return The path of the file to import
         */
        const std::filesystem::path& getImportFile() const;
        /**
         * @brief Sets the path of a file to import into the new account when created.
         * @param importFile The path of the file to import
         * @return True if the import file was set, else false (the file does not exist)
         */
        bool setImportFile(const std::filesystem::path& importFile);

    private:
        std::filesystem::path m_path;
        Models::AccountMetadata m_metadata;
        std::string m_password;
        bool m_overwriteExisting;
        std::filesystem::path m_importFile;
    };
}

#endif //NEWACCOUNTDIALOGCONTROLLER_H