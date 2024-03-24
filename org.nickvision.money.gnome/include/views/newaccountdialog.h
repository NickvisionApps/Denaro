#ifndef NEWACCOUNTDIALOG_H
#define NEWACCOUNTDIALOG_H

#include <memory>
#include <adwaita.h>
#include <libnick/events/event.h>
#include <libnick/events/parameventargs.h>
#include "controllers/newaccountdialogcontroller.h"

namespace Nickvision::Money::GNOME::Views
{
    /**
     * @brief A dialog for creating a new account.
     */
    class NewAccountDialog 
    {
    public:
        /**
         * @brief Creates a new NewAccountDialog.
         * @param controller The NewAccountDialogController
         * @param parent The GtkWindow object of the parent window
         * @return NewAccountDialog* (The caller is NOT responsible for deleting the returned pointer)
         */
        static NewAccountDialog* create(const std::shared_ptr<Shared::Controllers::NewAccountDialogController>& controller, GtkWindow* parent);
        /**
         * @brief Destructs a NewAccountDialog. 
         */
        ~NewAccountDialog();
        /**
         * @brief Gets the event when the dialog is finished (i.e. the user has created the account)
         * @return The finished event
         */
        Nickvision::Events::Event<Nickvision::Events::ParamEventArgs<std::shared_ptr<Shared::Controllers::NewAccountDialogController>>>& finished();
        /**
         * @brief Presents the NewAccountDialog.
         * @param parent The GtkWindow object of the parent window
         */
        void present() const;

    private:
        /**
         * @brief Constructs a NewAccountDialog.
         * @param controller NewAccountDialogController
         * @param parent The GtkWindow object of the parent window
         */
        NewAccountDialog(const std::shared_ptr<Shared::Controllers::NewAccountDialogController>& controller, GtkWindow* parent);
        /**
         * @brief Handles when the dialog is closed. 
         */
        void onClosed();
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
        Nickvision::Events::Event<Nickvision::Events::ParamEventArgs<std::shared_ptr<Shared::Controllers::NewAccountDialogController>>> m_finished;
        GtkBuilder* m_builder;
        GtkWindow* m_parent;
        AdwDialog* m_dialog;
        int m_currentPageNumber;
    };
}

#endif //NEWACCOUNTDIALOG_H