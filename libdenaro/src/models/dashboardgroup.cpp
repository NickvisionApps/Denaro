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
            m_data.emplace(currency, std::make_pair(amount, currency.getSymbol() + " " + std::to_string(amount) + " from " + accountName));
            return;
        }
        m_data.at(currency).first += amount;
        m_data.at(currency).second = "\n" + currency.getSymbol() + " " + std::to_string(m_data.at(currency).first) + " from " + accountName;
    }
}
