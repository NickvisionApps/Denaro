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
        /**
         * @brief Navigates to a page.
         * @param pageName The name of the page to navigate to
         */
        void go(const std::string& pageName);
        /**
         * @brief Handles when the basic account information is changed.
         */
        void onAccountInfoChanged();
        /**
         * @brief Handles when the custom currency is changed.
         */
        void onCurrencyChange();
        /**
         * @brief Handles when the new password is changed.
         */
        void onNewPasswordChange();
        /**
         * @brief Changes the account password.
         */
        void changePassword();
        /**
         * @brief Removes the account password.
         */
        void removePassword();
        std::shared_ptr<Shared::Controllers::AccountSettingsDialogController> m_controller;
    };
}

#endif //ACCOUNTSETTINGSDIALOG_H
