#ifndef ACCOUNTSETTINGSDIALOG_H
#define ACCOUNTSETTINGSDIALOG_H

#include <memory>
#include <adwaita.h>
#include "controllers/accountsettingsdialogcontroller.h"
#include "helpers/dialogbase.h"

namespace Nickvision::Money::GNOME::Views
{
    /**
     * @brief A dialog for managing account settings.
     */
    class AccountSettingsDialog : public Helpers::DialogBase
    {
    public:
        /**
         * @brief Constructs an AccountSettingsDialog.
         * @param controller AccountSettingsDialogController
         * @param parent The GtkWindow object of the parent window
         */
        AccountSettingsDialog(const std::shared_ptr<Shared::Controllers::AccountSettingsDialogController>& controller, GtkWindow* parent);

    private:
        std::shared_ptr<Shared::Controllers::AccountSettingsDialogController> m_controller;
    };
}

#endif //ACCOUNTSETTINGSDIALOG_H
