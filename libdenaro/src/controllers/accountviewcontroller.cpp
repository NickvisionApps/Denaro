#include "controllers/accountviewcontroller.h"
#include <stdexcept>
#include <libnick/app/aura.h>
#include <libnick/localization/gettext.h>
#include "helpers/currencyhelpers.h"
#include "models/configuration.h"

using namespace Nickvision::App;
using namespace Nickvision::Money::Shared::Models;

namespace Nickvision::Money::Shared::Controllers
{
    AccountViewController::AccountViewController(const std::filesystem::path& path, const std::string& password)
        : m_account{ std::make_shared<Account>(path) }
    {
        if(!m_account->login(password, Aura::getActive().getConfig<Configuration>("config").getGroupDefaultColor()))
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

    std::shared_ptr<AccountSettingsDialogController> AccountViewController::createAccountSettingsDialogController() const
    {
        return std::make_shared<AccountSettingsDialogController>(m_account);
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

    std::vector<TransactionReminder> AccountViewController::getTransactionReminders() const
    {
        return m_account->getTransactionReminders();
    }

    std::vector<std::pair<Group, std::string>> AccountViewController::getGroups() const
    {
        std::vector<std::pair<Group, std::string>> groups;
        for(const std::pair<const int, Group>& pair : m_account->getGroups())
        {
            groups.push_back({ pair.second, CurrencyHelpers::toAmountString(pair.second.getBalance(), m_account->getCurrency()) });
        }
        std::sort(groups.begin(), groups.end(), [](const std::pair<Group, std::string>& a, const std::pair<Group, std::string>& b) 
        { 
            if(a.first.getId() == -1)
            {
                return true;
            }
            if(b.first.getId() == -1)
            {
                return false;
            }
            return a.first < b.first;
        });
        return groups;
    }

    RecentAccount AccountViewController::toRecentAccount() const
    {
        RecentAccount recent{ m_account->getPath() };
        recent.setName(m_account->getMetadata().getName());
        recent.setType(m_account->getMetadata().getType());
        return recent;
    }
}
