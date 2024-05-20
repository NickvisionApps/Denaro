#include "controllers/accountsettingsdialogcontroller.h"

using namespace Nickvision::Money::Shared::Models;

namespace Nickvision::Money::Shared::Controllers
{
    AccountSettingsDialogController::AccountSettingsDialogController(const std::shared_ptr<Account>& account)
        : m_account{ account }
    {

    }
}
