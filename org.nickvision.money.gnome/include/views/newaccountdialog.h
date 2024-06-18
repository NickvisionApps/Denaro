#ifndef NEWACCOUNTDIALOG_H
#define NEWACCOUNTDIALOG_H

#include <memory>
#include <adwaita.h>
#include <libnick/events/event.h>
#include <libnick/events/parameventargs.h>
#include "controllers/newaccountdialogcontroller.h"
#include "helpers/dialogbase.h"

namespace Nickvision::Money::GNOME::Views
{
    /**
     * @brief A dialog for creating a new account.
     */
    class NewAccountDialog : public Helpers::DialogBase
    {
    public:
        /**
         * @brief Constructs a NewAccountDialog.
         * @param controller NewAccountDialogController
         * @param parent The GtkWindow object of the parent window
         */
        NewAccountDialog(const std::shared_ptr<Shared::Controllers::NewAccountDialogController>& controller, GtkWindow* parent);
        /**
         * @brief Gets the event when the user has created the account
         * @return The created event
         */
        Nickvision::Events::Event<Nickvision::Events::ParamEventArgs<std::shared_ptr<Shared::Controllers::NewAccountDialogController>>>& created();

    private:
        /**
         * @brief Navigates to the previous page.
         */
        void goBack();
        /**
         * @brief Navigates to the next page.
         */
        void goForward();
        /**
         * @brief Handles when the account name is changed.
         */
        void onAccountNameChanged();
        /**
         * @brief Prompts the user to select the folder where to store the account.
         */
        void selectAccountFolder();
        /**
         * @brief Handles when the account overwrite is changed.
         */
        void onAccountOverwriteChanged();
        /**
         * @brief Handles when the custom currency is changed.
         */
        void onCurrencyChange();
        /**
         * @brief Prompts the user to select an import file.
         */
        void selectImportFile();
        /**
         * @brief Clears the import file.
         */
        void clearImportFile();
        /**
         * @brief Finishes the dialog.
         */
        void finish();
        std::shared_ptr<Shared::Controllers::NewAccountDialogController> m_controller;
        Nickvision::Events::Event<Nickvision::Events::ParamEventArgs<std::shared_ptr<Shared::Controllers::NewAccountDialogController>>> m_created;
        int m_currentPageNumber;
    };
}

#endif //NEWACCOUNTDIALOG_H
