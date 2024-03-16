#include "controllers/dashboardviewcontroller.h"

namespace Nickvision::Money::Shared::Controllers
{
    DashboardViewController::DashboardViewController(const std::vector<std::shared_ptr<AccountViewController>>& accountViewControllers)
        : m_accountViewControllers{ accountViewControllers }
    {
        
    }
}