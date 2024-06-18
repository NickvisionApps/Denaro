#include "models/dashboardgroup.h"
#include <format>
#include <libnick/helpers/codehelpers.h>
#include <libnick/localization/gettext.h>
#include "helpers/currencyhelpers.h"

namespace Nickvision::Money::Shared::Models
{
    DashboardGroup::DashboardGroup()
    {

    }

    const std::unordered_map<Currency, std::pair<double, std::string>>& DashboardGroup::getData() const
    {
        return m_data;
    }

    void DashboardGroup::addAmount(double amount, const Currency& currency, const std::string& accountName)
    {
        if(!m_data.contains(currency))
        {
            m_data.emplace(currency, std::make_pair(amount, std::vformat(_("{} from {}"), std::make_format_args(CodeHelpers::unmove(CurrencyHelpers::toAmountString(amount, currency)), accountName))));
            return;
        }
        m_data.at(currency).first += amount;
        m_data.at(currency).second += "\n" + std::vformat(_("{} from {}"), std::make_format_args(CodeHelpers::unmove(CurrencyHelpers::toAmountString(amount, currency)), accountName));
    }
}
