#ifndef DASHBOARDGROUP_H
#define DASHBOARDGROUP_H

#include <string>
#include <unordered_map>
#include <utility>
#include "currency.h"

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
        const std::unordered_map<Currency, std::pair<double, std::string>>& getData() const;
        /**
         * @brief Adds an amount to the dashboard group.
         * @param amount The amount to add
         * @param currency The currency of the amount
         * @param accountName The name of the account the amount is from
         */
        void addAmount(double amount, const Currency& currency, const std::string& accountName);
        
    private:
        std::unordered_map<Currency, std::pair<double, std::string>> m_data;
    };
}
#endif //DASHBOARDGROUP_H
