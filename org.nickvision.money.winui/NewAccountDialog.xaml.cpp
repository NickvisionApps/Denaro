#include "NewAccountDialog.xaml.h"
#if __has_include("NewAccountDialog.g.cpp")
#include "NewAccountDialog.g.cpp"
#endif
#include <libnick/localization/gettext.h>

using namespace ::Nickvision::Money::Shared::Controllers;
using namespace winrt::Microsoft::UI::Xaml;
using namespace winrt::Microsoft::UI::Xaml::Controls;

namespace winrt::Nickvision::Money::WinUI::implementation 
{
    NewAccountDialog::NewAccountDialog()
    {
        InitializeComponent();
        IsPrimaryButtonEnabled(false);
        ViewStack().CurrentPage(L"Storage");
        //Localize Strings
        Title(winrt::box_value(winrt::to_hstring(_("New Account"))));
        CloseButtonText(winrt::to_hstring(_("Close")));
        PrimaryButtonText(winrt::to_hstring(_("Create")));
        SelectorItemStorage().Text(winrt::to_hstring(_("Storage")));
        RowAccountName().Title(winrt::to_hstring(_("Account Name")));
        TxtAccountName().PlaceholderText(winrt::to_hstring(_("Enter name here")));
        RowAccountPassword().Title(winrt::to_hstring(_("Account Password")));
        TxtAccountPassword().PlaceholderText(winrt::to_hstring(_("Enter password here")));
        RowAccountFolder().Title(winrt::to_hstring(_("Folder")));
        ToolTipService::SetToolTip(BtnSelectAccountFolder(), winrt::box_value(winrt::to_hstring(_("Select Folder"))));
        RowOverwrite().Title(winrt::to_hstring(_("Overwrite Existing Accounts")));
        TglOverwrite().OnContent(winrt::box_value(winrt::to_hstring(_("On"))));
        TglOverwrite().OffContent(winrt::box_value(winrt::to_hstring(_("Off"))));
        SelectorItemOptions().Text(winrt::to_hstring(_("Options")));
        RowAccountType().Title(winrt::to_hstring(_("Account Type")));
        CmbAccountType().Items().Append(winrt::box_value(winrt::to_hstring(_("Checking"))));
        CmbAccountType().Items().Append(winrt::box_value(winrt::to_hstring(_("Saving"))));
        CmbAccountType().Items().Append(winrt::box_value(winrt::to_hstring(_("Business"))));
        RowTransactionType().Title(winrt::to_hstring(_("Default Transaction Type")));
        CmbTransactionType().Items().Append(winrt::box_value(winrt::to_hstring(_("Income"))));
        CmbTransactionType().Items().Append(winrt::box_value(winrt::to_hstring(_("Expense"))));
        RowTransactionReminders().Title(winrt::to_hstring(_("Transaction Reminders Threshold")));
        ToolTipService::SetToolTip(RowTransactionReminders(), winrt::box_value(winrt::to_hstring(_("Determines how far in advanced Denaro should remind users of upcoming transactions in the account."))));
        CmbTransactionReminders().Items().Append(winrt::box_value(winrt::to_hstring(_("Never"))));
        CmbTransactionReminders().Items().Append(winrt::box_value(winrt::to_hstring(_("One Day Before"))));
        CmbTransactionReminders().Items().Append(winrt::box_value(winrt::to_hstring(_("One Week Before"))));
        CmbTransactionReminders().Items().Append(winrt::box_value(winrt::to_hstring(_("One Month Before"))));
        CmbTransactionReminders().Items().Append(winrt::box_value(winrt::to_hstring(_("Two Months Before"))));
        SelectorItemCurrency().Text(winrt::to_hstring(_("Currency")));
        SelectorItemImport().Text(winrt::to_hstring(_("Import")));
        LblImportFile().Text(winrt::to_hstring(_("Upload a supported file to use to import existing information into the new account. Denaro supports CSV, OFX, and QIF files.")));
    }

    void NewAccountDialog::SetController(const std::shared_ptr<NewAccountDialogController>& controller)
    {
        m_controller = controller;
        //Load
        LblAccountFolder().Text(winrt::to_hstring(m_controller->getFolder().filename().string()));
        TglOverwrite().IsOn(m_controller->getOverwriteExisting());
        CmbAccountType().SelectedIndex(static_cast<int>(m_controller->getMetadata().getType()));
        CmbTransactionType().SelectedIndex(static_cast<int>(m_controller->getMetadata().getDefaultTransactionType()));
        CmbTransactionReminders().SelectedIndex(static_cast<int>(m_controller->getMetadata().getTransactionRemindersThreshold()));
    }

    void NewAccountDialog::OnPageSelectionChanged(const SelectorBar& sender, const SelectorBarSelectionChangedEventArgs& args)
    {
        ViewStack().CurrentPage(sender.SelectedItem().Tag().as<winrt::hstring>());
    }
}