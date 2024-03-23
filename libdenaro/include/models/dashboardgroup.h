#ifndef DASHBOARDGROUP_H
#define DASHBOARDGROUP_H

#include <format>
#include <string>
#include <libnick/localization/gettext.h>
#include <unordered_map>
#include <utility>
#include "currency.h"
#include "helpers/currencyhelpers.h"

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief A model of a dashboard group. 
     */
    class DashboardGroup
    {
    public:
        /**
         * @brief Constructs a DashboardGroup.
         */
        DashboardGroup();
        /**
         * @brief Gets the data of the dashboard group.
         * @return A map of currency and amount, description pairs
         */
        const std::unordered_map<Nickvision::Money::Shared::Models::Currency, std::pair<double, std::string>>& getData() const;
        /**
         * @brief Adds an amount to the dashboard group.
         * @param currency The currency of the amount
         * @param amount The amount to add
         * @param accountName The name of the account the amount is from
         */
        void addAmount(const Nickvision::Money::Shared::Models::Currency& currency, double amount, const std::string& accountName);
        
    private:
        std::unordered_map<Nickvision::Money::Shared::Models::Currency, std::pair<double, std::string>> m_data;
    };
}
#endif //DASHBOARDGROUP_H
