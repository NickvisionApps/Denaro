#ifndef NEWACCOUNTDIALOG_H
#define NEWACCOUNTDIALOG_H

#include "includes.h"
#include <memory>
#include "Controls/SettingsRow.g.h"
#include "Controls/StatusPage.g.h"
#include "Controls/ViewStack.g.h"
#include "Controls/ViewStackPage.g.h"
#include "NewAccountDialog.g.h"
#include "controllers/newaccountdialogcontroller.h"

namespace winrt::Nickvision::Money::WinUI::implementation 
{
    /**
     * @brief A dialog for creating a new account.
     */
    class NewAccountDialog : public NewAccountDialogT<NewAccountDialog>
    {
    public:
        /**
         * @brief Constructs a NewAccountDialog. 
         */
        NewAccountDialog();
        /**
         * @brief Sets the controller for the dialog.
         * @param controller The NewAccountDialog
         */
        void SetController(const std::shared_ptr<::Nickvision::Money::Shared::Controllers::NewAccountDialogController>& controller);
        /**
         * @brief Handles when the page selection changes. 
         * @param sender SelectorBar
         * @param args SelectorBarSelectionChangedEventArgs
         */
        void OnPageSelectionChanged(const Microsoft::UI::Xaml::Controls::SelectorBar& sender, const Microsoft::UI::Xaml::Controls::SelectorBarSelectionChangedEventArgs& args);

    private:
        std::shared_ptr<::Nickvision::Money::Shared::Controllers::NewAccountDialogController> m_controller;
    };
}

namespace winrt::Nickvision::Money::WinUI::factory_implementation 
{
    class NewAccountDialog : public NewAccountDialogT<NewAccountDialog, implementation::NewAccountDialog>
    {

    };
}

#endif //NEWACCOUNTDIALOG_H