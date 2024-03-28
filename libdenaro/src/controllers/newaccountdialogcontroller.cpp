#include "controllers/newaccountdialogcontroller.h"
#include <libnick/app/aura.h>
#include <libnick/filesystem/userdirectories.h>

using namespace Nickvision::App;
using namespace Nickvision::Filesystem;
using namespace Nickvision::Money::Shared::Models;

namespace Nickvision::Money::Shared::Controllers
{
    NewAccountDialogController::NewAccountDialogController()
        : m_path{ UserDirectories::getDocuments() / "New.nmoney"},
        m_metadata{ "", AccountType::Checking },
        m_overwriteExisting{ false }
    {
        
    }

    const std::string& NewAccountDialogController::getId() const
    {
        return Aura::getActive().getAppInfo().getId();
    }

    const std::filesystem::path& NewAccountDialogController::getFilePath() const
    {
        return m_path;
    }

    const AccountMetadata& NewAccountDialogController::getMetadata() const
    {
        return m_metadata;
    }

    std::filesystem::path NewAccountDialogController::getFolder() const
    {
        return m_path.parent_path();
    }

    bool NewAccountDialogController::setFolder(const std::filesystem::path& folder)
    {
        if(folder.empty() || (std::filesystem::exists(folder / m_path.filename()) && !m_overwriteExisting))
        {
            return false;
        }
        m_path = folder / m_path.filename();
        return true;
    }

    bool NewAccountDialogController::setName(const std::string& name)
    {
        if(name.empty() || (std::filesystem::exists(m_path.stem() / (name + ".nmoney")) && !m_overwriteExisting))
        {
            return false;
        }
        m_metadata.setName(name);
        m_path.replace_filename(name + ".nmoney");
        return true;
    }

    const std::string& NewAccountDialogController::getPassword() const
    {
        return m_password;
    }

    void NewAccountDialogController::setPassword(const std::string& password)
    {
        m_password = password;
    }

    bool NewAccountDialogController::getOverwriteExisting() const
    {
        return m_overwriteExisting;
    }

    void NewAccountDialogController::setOverwriteExisting(bool overwriteExisting)
    {
        m_overwriteExisting = overwriteExisting;
    }

    void NewAccountDialogController::setAccountType(AccountType accountType)
    {
        m_metadata.setType(accountType);
    }

    void NewAccountDialogController::setDefaultTransactionType(TransactionType defaultTransactionType)
    {
        m_metadata.setDefaultTransactionType(defaultTransactionType);
    }

    void NewAccountDialogController::setTransactionRemindersThreshold(RemindersThreshold transactionReminderThreshold)
    {
        m_metadata.setTransactionRemindersThreshold(transactionReminderThreshold);
    }

    void NewAccountDialogController::setCustomCurrencyOff()
    {
        m_metadata.setUseCustomCurrency(false);
        m_metadata.setCustomCurrency({});   
    }

    CurrencyCheckStatus NewAccountDialogController::setCustomCurrency(const std::string& symbol, const std::string& code, char decimalSeparator, char groupSeparator, int decimalDigits, AmountStyle amountStyle)
    {
        Currency currency{ symbol, code };
        currency.setDecimalSeparator(decimalSeparator);
        currency.setGroupSeparator(groupSeparator);
        currency.setDecimalDigits(decimalDigits);
        currency.setAmountStyle(amountStyle);
        CurrencyCheckStatus status{ currency.validate() };
        if(status == CurrencyCheckStatus::Valid)
        {
            m_metadata.setUseCustomCurrency(true);
            m_metadata.setCustomCurrency(currency);
        }
        return status;
    }

    const std::filesystem::path& NewAccountDialogController::getImportFile() const
    {
        return m_importFile;
    }

    bool NewAccountDialogController::setImportFile(const std::filesystem::path& importFile)
    {
        if(std::filesystem::exists(importFile))
        {
            m_importFile = importFile;
            return true;
        }
        return false;
    }
}