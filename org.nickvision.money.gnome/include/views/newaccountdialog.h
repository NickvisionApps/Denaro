#ifndef NEWACCOUNTDIALOG_H
#define NEWACCOUNTDIALOG_H

#include <memory>
#include <adwaita.h>
#include "controllers/newaccountdialogcontroller.h"

namespace Nickvision::Money::GNOME::Views
{
    class NewAccountDialog 
    {
    public:
        /**
         * @brief Constructs a NewAccountDialog.
         * @param controller NewAccountDialogController
         * @param parent The GtkWindow object of the parent window
         */
        NewAccountDialog(const std::shared_ptr<Shared::Controllers::NewAccountDialogController>& controller, GtkWindow* parent);
        /**
         * @brief Destructs a NewAccountDialog. 
         */
        ~NewAccountDialog();
        /**
         * @brief Gets the NewAccountDialogController.
         * @return NewAccountDialogController
         */
        const std::shared_ptr<Shared::Controllers::NewAccountDialogController>& getController() const;
        /**
         * @brief Shows the NewAccountDialog and waits for it to close.
         * @return True if the dialog was finished, else false if canceled by the user
         */
        bool run();

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
        GtkBuilder* m_builder;
        GtkWindow* m_parent;
        AdwWindow* m_dialog;
        bool m_finished;
        int m_currentPageNumber;
    };
}

#endif //NEWACCOUNTDIALOG_H