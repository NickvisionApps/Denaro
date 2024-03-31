#include "controllers/accountviewcontroller.h"
#include <stdexcept>
#include <libnick/localization/gettext.h>
#include "helpers/currencyhelpers.h"

using namespace Nickvision::Money::Shared::Models;

namespace Nickvision::Money::Shared::Controllers
{
    AccountViewController::AccountViewController(const std::filesystem::path& path, const std::string& password)
        : m_account{ std::make_unique<Account>(path) }
    {
        if(!m_account->login(password))
        {
            throw std::runtime_error{ _("Unable to login to the account. The provided password may be invalid.") };
        }
    }

    const std::filesystem::path& AccountViewController::getPath() const
    {
        return m_account->getPath();
    }

    const AccountMetadata& AccountViewController::getMetadata() const
    {
        return m_account->getMetadata();
    }

    std::string AccountViewController::getTotalAmountString() const
    {
        return CurrencyHelpers::toAmountString(m_account->getTotal(), m_account->getCurrency());
    }

    std::string AccountViewController::getIncomeAmountString() const
    {
        return CurrencyHelpers::toAmountString(m_account->getIncome(), m_account->getCurrency());
    }

    std::string AccountViewController::getExpenseAmountString() const
    {
        return CurrencyHelpers::toAmountString(m_account->getExpense(), m_account->getCurrency());
    }

    std::string AccountViewController::getUngroupedAmountString() const
    {
        return CurrencyHelpers::toAmountString(m_account->getGroupTotal({ -1 }), m_account->getCurrency());
    }

    RecentAccount AccountViewController::toRecentAccount() const
    {
        RecentAccount recent{ m_account->getPath() };
        recent.setName(m_account->getMetadata().getName());
        recent.setType(m_account->getMetadata().getType());
        return recent;
    }
}
