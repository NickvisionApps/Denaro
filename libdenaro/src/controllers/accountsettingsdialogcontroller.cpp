#include "controllers/accountsettingsdialogcontroller.h"

using namespace Nickvision::Money::Shared::Models;

namespace Nickvision::Money::Shared::Controllers
{
    AccountSettingsDialogController::AccountSettingsDialogController(const std::shared_ptr<Account>& account)
        : m_account{ account }
    {

    }

    bool AccountSettingsDialogController::setName(const std::string& name)
    {
        if(name.empty())
        {
            return false;
        }
        AccountMetadata metadata{ m_account->getMetadata() };
        metadata.setName(name);
        m_account->setMetadata(metadata);
        return true;
    }

    bool AccountSettingsDialogController::setPassword(const std::string& password)
    {
        return m_account->changePassword(password);
    }

    void AccountSettingsDialogController::setAccountType(Models::AccountType accountType)
    {
        AccountMetadata metadata{ m_account->getMetadata() };
        metadata.setType(accountType);
        m_account->setMetadata(metadata);
    }
    
    void AccountSettingsDialogController::setDefaultTransactionType(Models::TransactionType defaultTransactionType)
    {
        AccountMetadata metadata{ m_account->getMetadata() };
        metadata.setDefaultTransactionType(defaultTransactionType);
        m_account->setMetadata(metadata);
    }
    
    void AccountSettingsDialogController::setCustomCurrencyOff()
    {
        AccountMetadata metadata{ m_account->getMetadata() };
        metadata.setUseCustomCurrency(false);
        metadata.setCustomCurrency({});
        m_account->setMetadata(metadata);
    }
    
    Models::CurrencyCheckStatus AccountSettingsDialogController::setCustomCurrency(const std::string& symbol, const std::string& code, char decimalSeparator, char groupSeparator, int decimalDigits, Models::AmountStyle amountStyle)
    {
        Currency currency{ symbol, code };
        currency.setDecimalSeparator(decimalSeparator);
        currency.setGroupSeparator(groupSeparator);
        currency.setDecimalDigits(decimalDigits);
        currency.setAmountStyle(amountStyle);
        CurrencyCheckStatus status{ currency.validate() };
        if(status == CurrencyCheckStatus::Valid)
        {
            AccountMetadata metadata{ m_account->getMetadata() };
            metadata.setUseCustomCurrency(true);
            metadata.setCustomCurrency(currency);
            m_account->setMetadata(metadata);
        }
        return status;
    }
}
