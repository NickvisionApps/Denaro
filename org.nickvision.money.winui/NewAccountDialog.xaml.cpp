#include "NewAccountDialog.xaml.h"
#if __has_include("NewAccountDialog.g.cpp")
#include "NewAccountDialog.g.cpp"
#endif
#include <libnick/localization/gettext.h>
#include "helpers/currencyhelpers.h"

using namespace ::Nickvision::Money::Shared;
using namespace ::Nickvision::Money::Shared::Controllers;
using namespace ::Nickvision::Money::Shared::Models;
using namespace winrt::Microsoft::UI::Xaml;
using namespace winrt::Microsoft::UI::Xaml::Controls;
using namespace winrt::Windows::Storage;
using namespace winrt::Windows::Storage::Pickers;

namespace winrt::Nickvision::Money::WinUI::implementation 
{
    NewAccountDialog::NewAccountDialog()
    {
        InitializeComponent();
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
        LblSystemCurrencyDescription().Text(winrt::to_hstring(_("Your system reported that your currency is")));
        LblSystemCurrency().Text(winrt::to_hstring(CurrencyHelpers::getSystemCurrency().toString()));
        RowCustomCurrency().Title(winrt::to_hstring(_("Use Custom Currency")));
        TglCustomCurrency().OnContent(winrt::box_value(winrt::to_hstring(_("On"))));
        TglCustomCurrency().OffContent(winrt::box_value(winrt::to_hstring(_("Off"))));
        RowCustomSymbol().Title(winrt::to_hstring(_("Symbol")));
        TxtCustomSymbol().PlaceholderText(winrt::to_hstring(_("Enter symbol here")));
        RowCustomCode().Title(winrt::to_hstring(_("Code")));
        TxtCustomCode().PlaceholderText(winrt::to_hstring(_("Enter code here")));
        RowCustomDecimalSeparator().Title(winrt::to_hstring(_("Decimal Separator")));
        CmbCustomDecimalSeparator().Items().Append(winrt::box_value(winrt::to_hstring(_("."))));
        CmbCustomDecimalSeparator().Items().Append(winrt::box_value(winrt::to_hstring(_(","))));
        CmbCustomDecimalSeparator().Items().Append(winrt::box_value(winrt::to_hstring(_("Custom"))));
        RowCustomGroupSeparator().Title(winrt::to_hstring(_("Group Separator")));
        CmbCustomGroupSeparator().Items().Append(winrt::box_value(winrt::to_hstring(_("."))));
        CmbCustomGroupSeparator().Items().Append(winrt::box_value(winrt::to_hstring(_(","))));
        CmbCustomGroupSeparator().Items().Append(winrt::box_value(winrt::to_hstring(_("'"))));
        CmbCustomGroupSeparator().Items().Append(winrt::box_value(winrt::to_hstring(_("None"))));
        CmbCustomGroupSeparator().Items().Append(winrt::box_value(winrt::to_hstring(_("Custom"))));
        RowCustomDecimalDigits().Title(winrt::to_hstring(_("Decimal Digits")));
        CmbCustomDecimalDigits().Items().Append(winrt::box_value(winrt::to_hstring(_("Two"))));
        CmbCustomDecimalDigits().Items().Append(winrt::box_value(winrt::to_hstring(_("Three"))));
        CmbCustomDecimalDigits().Items().Append(winrt::box_value(winrt::to_hstring(_("Four"))));
        CmbCustomDecimalDigits().Items().Append(winrt::box_value(winrt::to_hstring(_("Five"))));
        CmbCustomDecimalDigits().Items().Append(winrt::box_value(winrt::to_hstring(_("Six"))));
        RowCustomAmountStyle().Title(winrt::to_hstring(_("Amount Style")));
        RowCustomAmountStyle().Description(winrt::to_hstring(_("$ is the currency's symbol.")));
        CmbCustomAmountStyle().Items().Append(winrt::box_value(winrt::to_hstring("$100")));
        CmbCustomAmountStyle().Items().Append(winrt::box_value(winrt::to_hstring("100$")));
        CmbCustomAmountStyle().Items().Append(winrt::box_value(winrt::to_hstring("$ 100")));
        CmbCustomAmountStyle().Items().Append(winrt::box_value(winrt::to_hstring("100 $")));
        SelectorItemImport().Text(winrt::to_hstring(_("Import")));
        LblImportFileDescription().Text(winrt::to_hstring(_("Upload a supported file to use to import existing information into the new account. Denaro supports CSV, OFX, and QIF files.")));
        RowImportFile().Title(winrt::to_hstring(_("Import File")));
        LblImportFile().Text(winrt::to_hstring(_("No File Selected")));
        ToolTipService::SetToolTip(BtnSelectImportFile(), winrt::box_value(winrt::to_hstring(_("Select File"))));
        ToolTipService::SetToolTip(BtnClearImportFile(), winrt::box_value(winrt::to_hstring(_("Clear File"))));
    }

    void NewAccountDialog::SetController(const std::shared_ptr<NewAccountDialogController>& controller, HWND hwnd)
    {
        m_controller = controller;
        m_hwnd = hwnd;
        //Load
        IsPrimaryButtonEnabled(false);
        ViewStack().CurrentPage(L"Storage");
        TxtAccountName().Text(winrt::to_hstring(m_controller->getMetadata().getName()));
        TxtAccountPassword().Password(winrt::to_hstring(m_controller->getPassword()));
        LblAccountFolder().Text(winrt::to_hstring(m_controller->getFolder().filename().string()));
        TglOverwrite().IsOn(m_controller->getOverwriteExisting());
        CmbAccountType().SelectedIndex(static_cast<int>(m_controller->getMetadata().getType()));
        CmbTransactionType().SelectedIndex(static_cast<int>(m_controller->getMetadata().getDefaultTransactionType()));
        CmbTransactionReminders().SelectedIndex(static_cast<int>(m_controller->getMetadata().getTransactionRemindersThreshold()));
        TglCustomCurrency().IsOn(m_controller->getMetadata().getUseCustomCurrency());
        TxtCustomSymbol().Text(winrt::to_hstring(m_controller->getMetadata().getCustomCurrency().getSymbol()));
        TxtCustomCode().Text(winrt::to_hstring(m_controller->getMetadata().getCustomCurrency().getCode()));
        switch(m_controller->getMetadata().getCustomCurrency().getDecimalSeparator())
        {
        case '.':
            CmbCustomDecimalSeparator().SelectedIndex(0);
            break;
        case ',':
            CmbCustomDecimalSeparator().SelectedIndex(1);
            break;
        default:
            CmbCustomDecimalSeparator().SelectedIndex(2);
            TxtCustomDecimalSeparator().Visibility(Visibility::Visible);
            TxtCustomDecimalSeparator().Text(winrt::to_hstring(m_controller->getMetadata().getCustomCurrency().getDecimalSeparator()));
            break;
        }
        switch(m_controller->getMetadata().getCustomCurrency().getGroupSeparator())
        {
        case '.':
            CmbCustomGroupSeparator().SelectedIndex(0);
            break;
        case ',':
            CmbCustomGroupSeparator().SelectedIndex(1);
            break;
        case '\'':
            CmbCustomGroupSeparator().SelectedIndex(2);
            break;
        case '\0':
            CmbCustomGroupSeparator().SelectedIndex(3);
            break;
        default:
            CmbCustomGroupSeparator().SelectedIndex(4);
            TxtCustomGroupSeparator().Visibility(Visibility::Visible);
            TxtCustomGroupSeparator().Text(winrt::to_hstring(m_controller->getMetadata().getCustomCurrency().getGroupSeparator()));
            break;
        }
        CmbCustomDecimalDigits().SelectedIndex(m_controller->getMetadata().getCustomCurrency().getDecimalDigits() - 2);
        CmbCustomAmountStyle().SelectedIndex(static_cast<int>(m_controller->getMetadata().getCustomCurrency().getAmountStyle()));
        LblImportFile().Text(winrt::to_hstring(m_controller->getImportFile().empty() ? _("No File Selected") : m_controller->getImportFile().filename().string()));
        BtnClearImportFile().Visibility(m_controller->getImportFile().empty() ? Visibility::Collapsed : Visibility::Visible);
    }

    void NewAccountDialog::OnPageSelectionChanged(const SelectorBar& sender, const SelectorBarSelectionChangedEventArgs& args)
    {
        ViewStack().CurrentPage(sender.SelectedItem().Tag().as<winrt::hstring>());
    }

    Windows::Foundation::IAsyncAction NewAccountDialog::SelectAccountFolder(const IInspectable& sender, const RoutedEventArgs& args)
    {
        FolderPicker picker;
        picker.as<::IInitializeWithWindow>()->Initialize(m_hwnd);
        picker.SuggestedStartLocation(PickerLocationId::DocumentsLibrary); 
        picker.FileTypeFilter().Append(L"*");
        StorageFolder folder{ co_await picker.PickSingleFolderAsync() };
        if(folder)
        {
            std::filesystem::path folderPath{ winrt::to_string(folder.Path()) };
            bool result{ m_controller->setFolder(folderPath) };
            LblAccountFolder().Text(winrt::to_hstring(result ? folderPath.filename().string() : m_controller->getFolder().filename().string()));
        }
    }

    Windows::Foundation::IAsyncAction NewAccountDialog::SelectImportFile(const IInspectable& sender, const RoutedEventArgs& args)
    {
        FileOpenPicker picker;
        picker.as<::IInitializeWithWindow>()->Initialize(m_hwnd);
        picker.SuggestedStartLocation(PickerLocationId::DocumentsLibrary);
        picker.FileTypeFilter().Append(L".csv");
        picker.FileTypeFilter().Append(L".ofx");
        picker.FileTypeFilter().Append(L".qif");
        StorageFile file{ co_await picker.PickSingleFileAsync() };
        if(file)
        {
            std::filesystem::path filePath{ winrt::to_string(file.Path()) };
            m_controller->setImportFile(filePath);
            LblImportFile().Text(winrt::to_hstring(filePath.filename().string()));
            BtnClearImportFile().Visibility(Visibility::Visible);
        }
    }

    void NewAccountDialog::ClearImportFile(const IInspectable& sender, const RoutedEventArgs& args)
    {
        m_controller->setImportFile({});
        LblImportFile().Text(winrt::to_hstring(_("No File Selected")));
        BtnClearImportFile().Visibility(Visibility::Collapsed);
    }

    void NewAccountDialog::OnValidateOptions(const IInspectable& sender, const TextChangedEventArgs& args)
    {
        ValidateOptions();
    }

    void NewAccountDialog::OnValidateOptions(const IInspectable& sender, const RoutedEventArgs& args)
    {
        ValidateOptions();
    }

    void NewAccountDialog::OnValidateOptions(const IInspectable& sender, const SelectionChangedEventArgs& args)
    {
        ValidateOptions();
    }

    void NewAccountDialog::ValidateOptions()
    {
        bool valid{ true };
        RowAccountName().Title(winrt::to_hstring(_("Account Name")));
        RowCustomSymbol().Title(winrt::to_hstring(_("Symbol")));
        RowCustomCode().Title(winrt::to_hstring(_("Code")));
        RowCustomDecimalSeparator().Title(winrt::to_hstring(_("Decimal Separator")));
        RowCustomGroupSeparator().Title(winrt::to_hstring(_("Group Separator")));
        m_controller->setPassword(winrt::to_string(TxtAccountPassword().Password()));
        m_controller->setOverwriteExisting(TglOverwrite().IsOn());
        if(!m_controller->setName(winrt::to_string(TxtAccountName().Text())))
        {
            valid = false;
            RowAccountName().Title(winrt::to_hstring(_("Account Name (Invalid)")));
        }
        m_controller->setAccountType(static_cast<AccountType>(CmbAccountType().SelectedIndex()));
        m_controller->setDefaultTransactionType(static_cast<TransactionType>(CmbTransactionType().SelectedIndex()));
        m_controller->setTransactionRemindersThreshold(static_cast<RemindersThreshold>(CmbTransactionReminders().SelectedIndex()));
        if(!TglCustomCurrency().IsOn())
        {
            m_controller->setCustomCurrencyOff();
            GroupCustomCurrency().Visibility(Visibility::Collapsed);
        }
        else
        {
            GroupCustomCurrency().Visibility(Visibility::Visible);
            std::string symbol{ winrt::to_string(TxtCustomSymbol().Text()) };
            std::string code{ winrt::to_string(TxtCustomCode().Text()) };
            char decimalSeparator;
            switch(CmbCustomDecimalSeparator().SelectedIndex())
            {
            case 0:
                TxtCustomDecimalSeparator().Visibility(Visibility::Collapsed);
                decimalSeparator = '.';
                break;
            case 1:
                TxtCustomDecimalSeparator().Visibility(Visibility::Collapsed);
                decimalSeparator = ',';
                break;
            default:
                TxtCustomDecimalSeparator().Visibility(Visibility::Visible);
                decimalSeparator = winrt::to_string(TxtCustomDecimalSeparator().Text())[0];
                break;
            }
            char groupSeparator;
            switch(CmbCustomGroupSeparator().SelectedIndex())
            {
            case 0:
                TxtCustomGroupSeparator().Visibility(Visibility::Collapsed);
                groupSeparator = '.';
                break;
            case 1:
                TxtCustomGroupSeparator().Visibility(Visibility::Collapsed);
                groupSeparator = ',';
                break;
            case 2:
                TxtCustomGroupSeparator().Visibility(Visibility::Collapsed);
                groupSeparator = '\'';
                break;
            case 3:
                TxtCustomGroupSeparator().Visibility(Visibility::Collapsed);
                groupSeparator = '\0';
                break;
            default:
                TxtCustomGroupSeparator().Visibility(Visibility::Visible);
                groupSeparator = winrt::to_string(TxtCustomGroupSeparator().Text())[0];
                break;
            }
            int decimalDigits{ CmbCustomDecimalDigits().SelectedIndex() + 2 };
            AmountStyle amountStyle{ static_cast<AmountStyle>(CmbCustomAmountStyle().SelectedIndex()) };
            CurrencyCheckStatus status{ m_controller->setCustomCurrency(symbol, code, decimalSeparator, groupSeparator, decimalDigits, amountStyle) };
            valid = valid && status == CurrencyCheckStatus::Valid;
            if(status == CurrencyCheckStatus::EmptySymbol)
            {
                RowCustomSymbol().Title(winrt::to_hstring(_("Symbol (Empty)")));
            }
            else if(status == CurrencyCheckStatus::EmptyCode)
            {
                RowCustomCode().Title(winrt::to_hstring(_("Code (Empty)")));
            }
            else if(status == CurrencyCheckStatus::EmptyDecimalSeparator)
            {
                RowCustomDecimalSeparator().Title(winrt::to_hstring(_("Decimal Separator (Empty)")));
            }
            else if(status == CurrencyCheckStatus::SameSeparators)
            {
                RowCustomDecimalSeparator().Title(winrt::to_hstring(_("Decimal Separator (Invalid)")));
                RowCustomGroupSeparator().Title(winrt::to_hstring(_("Group Separator (Invalid)")));
            }
            else if(status == CurrencyCheckStatus::SameSymbolAndDecimalSeparator)
            {
                RowCustomSymbol().Title(winrt::to_hstring(_("Symbol (Invalid)")));
                RowCustomDecimalSeparator().Title(winrt::to_hstring(_("Decimal Separator (Invalid)")));
            }
            else if(status == CurrencyCheckStatus::SameSymbolAndGroupSeparator)
            {
                RowCustomSymbol().Title(winrt::to_hstring(_("Symbol (Invalid)")));
                RowCustomGroupSeparator().Title(winrt::to_hstring(_("Group Separator (Invalid)")));
            }
        }
        IsPrimaryButtonEnabled(valid);
    }
}