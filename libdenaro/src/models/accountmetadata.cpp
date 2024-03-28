#include "models/accountmetadata.h"

namespace Nickvision::Money::Shared::Models
{
    AccountMetadata::AccountMetadata(const std::string& name, AccountType type)
        : m_name{ name },
        m_type{ type },
        m_useCustomCurrency{ false },
        m_defaultTransactionType{ TransactionType::Income },
        m_transactionRemindersThreshold{ RemindersThreshold::OneDayBefore },
        m_showGroupsList{ true },
        m_showTagsList{ true },
        m_sortTransactionsBy{ SortBy::Date },
        m_sortFirstToLast{ false }
    {

    }

    const std::string& AccountMetadata::getName() const
    {
        return m_name;
    }

    void AccountMetadata::setName(const std::string& name)
    {
        m_name = name;
    }

    AccountType AccountMetadata::getType() const
    {
        return m_type;
    }

    void AccountMetadata::setType(AccountType type)
    {
        m_type = type;
    }

    bool AccountMetadata::getUseCustomCurrency() const
    {
        return m_useCustomCurrency;
    }

    void AccountMetadata::setUseCustomCurrency(bool useCustomCurrency)
    {
        m_useCustomCurrency = useCustomCurrency;
    }

    const Currency& AccountMetadata::getCustomCurrency() const
    {
        return m_customCurrency;
    }

    void AccountMetadata::setCustomCurrency(const Currency& customCurrency)
    {
        m_customCurrency = customCurrency;
    }

    TransactionType AccountMetadata::getDefaultTransactionType() const
    {
        return m_defaultTransactionType;
    }

    void AccountMetadata::setDefaultTransactionType(TransactionType defaultTransactionType)
    {
        m_defaultTransactionType = defaultTransactionType;
    }

    RemindersThreshold AccountMetadata::getTransactionRemindersThreshold() const
    {
        return m_transactionRemindersThreshold;
    }

    void AccountMetadata::setTransactionRemindersThreshold(RemindersThreshold remindersThreshold)
    {
        m_transactionRemindersThreshold = remindersThreshold;
    }

    bool AccountMetadata::getShowGroupsList() const
    {
        return m_showGroupsList;
    }

    void AccountMetadata::setShowGroupsList(bool showGroupsList)
    {
        m_showGroupsList = showGroupsList;
    }

    bool AccountMetadata::getShowTagsList() const
    {
        return m_showTagsList;
    }
    
    void AccountMetadata::setShowTagsList(bool showTagsList)
    {
        m_showTagsList = showTagsList;
    }

    SortBy AccountMetadata::getSortTransactionsBy() const
    {
        return m_sortTransactionsBy;
    }

    void AccountMetadata::setSortTransactionsBy(SortBy sortTransactionsBy)
    {
        m_sortTransactionsBy = sortTransactionsBy;
    }

    bool AccountMetadata::getSortFirstToLast() const
    {
        return m_sortFirstToLast;
    }

    void AccountMetadata::setSortFirstToLast(bool sortFirstToLast)
    {
        m_sortFirstToLast = sortFirstToLast;
    }
}