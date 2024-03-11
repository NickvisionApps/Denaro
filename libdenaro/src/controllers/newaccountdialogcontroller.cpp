#include "controllers/newaccountdialogcontroller.h"

namespace Nickvision::Money::Shared::Controllers
{
    NewAccountDialogController::NewAccountDialogController(const std::vector<std::filesystem::path>& openAccounts)
        : m_openAccounts(openAccounts)
    {
        
    }
}