#ifndef DASHBOARDVIEWCONTROLLER_H
#define DASHBOARDVIEWCONTROLLER_H

#include <memory>
#include <vector>
#include "accountviewcontroller.h"

namespace Nickvision::Money::Shared::Controllers
{
    /**
     * @brief A controller for a DashboardView. 
     */
    class DashboardViewController
    {
    public:
        /**
         * @brief Construct a DashboardViewController.
         * @param accountViewControllers A vector of the controllers for the accounts to display on the dashboard
         */
        DashboardViewController(const std::vector<std::shared_ptr<AccountViewController>>& accountViewControllers);
        
    private:
        std::vector<std::shared_ptr<AccountViewController>> m_accountViewControllers;
    };
}

#endif //DASHBOARDVIEWCONTROLLER_H