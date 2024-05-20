#ifndef ACCOUNTSETTINGSDIALOGCONTROLLER_H
#define ACCOUNTSETTINGSDIALOGCONTROLLER_H

#include <memory>
#include "models/account.h"

namespace Nickvision::Money::Shared::Controllers
{
    class AccountSettingsDialogController
    {
    public:
        AccountSettingsDialogController(const std::shared_ptr<Models::Account>& account);

    private:
        std::shared_ptr<Models::Account> m_account;
    };
}

#endif //ACCOUNTSETTINGSDIALOGCONTROLLER_H
