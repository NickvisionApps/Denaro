#include "views/accountsettingsdialog.h"
#include <libnick/localization/gettext.h>

using namespace Nickvision::Money::GNOME::Helpers;
using namespace Nickvision::Money::Shared::Controllers;

namespace Nickvision::Money::GNOME::Views
{
    AccountSettingsDialog::AccountSettingsDialog(const std::shared_ptr<AccountSettingsDialogController>& controller, GtkWindow* parent)
        : DialogBase{ parent, "account_settings_dialog" },
        m_controller{ controller }
    {

    }
}
