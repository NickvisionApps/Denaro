#include <format>
#include <libnick/localization/gettext.h>
#include "helpers/currencyhelpers.h"
#include "models/dashboardgroup.h"

namespace Nickvision::Money::Shared::Models
{
    DashboardGroup::DashboardGroup()
        : m_data{ }
    {
    }

    const std::unordered_map<Nickvision::Money::Shared::Models::Currency, std::pair<double, std::string>>& DashboardGroup::getData() const
    {
        return m_data;
    }

    void DashboardGroup::addAmount(const Nickvision::Money::Shared::Models::Currency& currency, double amount, const std::string& accountName)
    {
        if(!m_data.contains(currency))
        {
            m_data.emplace(currency, std::make_pair(amount, std::vformat("{} from {}", std::make_format_args(Nickvision::Money::Shared::CurrencyHelpers::toAmountString(amount, currency), accountName))));
            return;
        }
        m_data.at(currency).first += amount;
        m_data.at(currency).second += "\n" + std::vformat("{} from {}", std::make_format_args(Nickvision::Money::Shared::CurrencyHelpers::toAmountString(amount, currency), accountName));
    }
}
