#ifndef NEWACCOUNTDIALOG_H
#define NEWACCOUNTDIALOG_H

#include "includes.h"
#include <memory>
#include "Controls/SettingsRow.g.h"
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
         * @param hwnd The HWND of the parent window
         */
        void SetController(const std::shared_ptr<::Nickvision::Money::Shared::Controllers::NewAccountDialogController>& controller, HWND hwnd);
        /**
         * @brief Handles when the dialog is opened.
         * @param sender ContentDialog
         * @param args ContentDialogOpenedEventArgs 
         */
        void OnOpened(const Microsoft::UI::Xaml::Controls::ContentDialog& sender, const Microsoft::UI::Xaml::Controls::ContentDialogOpenedEventArgs& args);
        /**
         * @brief Handles when the page selection changes. 
         * @param sender SelectorBar
         * @param args SelectorBarSelectionChangedEventArgs
         */
        void OnPageSelectionChanged(const Microsoft::UI::Xaml::Controls::SelectorBar& sender, const Microsoft::UI::Xaml::Controls::SelectorBarSelectionChangedEventArgs& args);
        /**
         * @brief Prompts the user to select the folder where to store the account.
         * @param sender IInspectable
         * @param args RoutedEventArgs
         */
        Windows::Foundation::IAsyncAction SelectAccountFolder(const IInspectable& sender, const Microsoft::UI::Xaml::RoutedEventArgs& args);
        /**
         * @brief Prompts the user to select an import file.
         * @param sender IInspectable
         * @param args RoutedEventArgs
         */
        Windows::Foundation::IAsyncAction SelectImportFile(const IInspectable& sender, const Microsoft::UI::Xaml::RoutedEventArgs& args);
        /**
         * @brief Clears the import file.
         * @param sender IInspectable
         * @param args RoutedEventArgs
         */
        void ClearImportFile(const IInspectable& sender, const Microsoft::UI::Xaml::RoutedEventArgs& args);
        /**
         * @brief Validates the options for the dialog.
         * @param sender IInspectable
         * @param args TextChangedEventArgs
         */
        void OnValidateOptions(const IInspectable& sender, const Microsoft::UI::Xaml::Controls::TextChangedEventArgs& args);
        /**
         * @brief Validates the options for the dialog.
         * @param sender IInspectable
         * @param args RoutedEventArgs
         */
        void OnValidateOptions(const IInspectable& sender, const Microsoft::UI::Xaml::RoutedEventArgs& args);
        /**
         * @brief Validates the options for the dialog.
         * @param sender IInspectable
         * @param args SelectionChangedEventArgs
         */
        void OnValidateOptions(const IInspectable& sender, const Microsoft::UI::Xaml::Controls::SelectionChangedEventArgs& args);

    private:
        /**
         * @brief Validates the options for the dialog.
         */
        void ValidateOptions();
        std::shared_ptr<::Nickvision::Money::Shared::Controllers::NewAccountDialogController> m_controller;
        HWND m_hwnd;
        bool m_constructing;
    };
}

namespace winrt::Nickvision::Money::WinUI::factory_implementation 
{
    class NewAccountDialog : public NewAccountDialogT<NewAccountDialog, implementation::NewAccountDialog>
    {

    };
}

#endif //NEWACCOUNTDIALOG_H