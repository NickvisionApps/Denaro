#include "SettingsPage.xaml.h"
#if __has_include("SettingsPage.g.cpp")
#include "SettingsPage.g.cpp"
#endif
#include <libnick/localization/gettext.h>

using namespace ::Nickvision::Money::Shared::Controllers;
using namespace ::Nickvision::Money::Shared::Models;
using namespace winrt::Microsoft::UI::Xaml;
using namespace winrt::Microsoft::UI::Xaml::Controls;
using namespace winrt::Microsoft::UI::Xaml::Media;

namespace winrt::Nickvision::Money::WinUI::implementation 
{
    SettingsPage::SettingsPage()
        : m_constructing{ true }
    {
        InitializeComponent();
        //Localize Strings
        LblTitle().Text(winrt::to_hstring(_("Settings")));
        LblUserInterface().Text(winrt::to_hstring(_("User Interface")));
        RowTheme().Title(winrt::to_hstring(_("Theme")));
        CmbTheme().Items().Append(winrt::box_value(winrt::to_hstring(_p("Theme", "Light"))));
        CmbTheme().Items().Append(winrt::box_value(winrt::to_hstring(_p("Theme", "Dark"))));
        CmbTheme().Items().Append(winrt::box_value(winrt::to_hstring(_p("Theme", "System"))));
        RowAutomaticallyCheckForUpdates().Title(winrt::to_hstring(_("Automatically Check for Updates")));
        TglAutomaticallyCheckForUpdates().OnContent(winrt::box_value(winrt::to_hstring(_("On"))));
        TglAutomaticallyCheckForUpdates().OffContent(winrt::box_value(winrt::to_hstring(_("Off"))));
        LblColors().Text(winrt::to_hstring(_("Colors")));
        RowDefaultTransaction().Title(winrt::to_hstring(_("Default Transaction Color")));
        RowDefaultTransaction().Description(winrt::to_hstring(_("A change in this setting will only be applied to newly added transactions.")));
        RowDefaultTransfer().Title(winrt::to_hstring(_("Default Transfer Color")));
        RowDefaultTransfer().Description(winrt::to_hstring(_("A change in this setting will only be applied to new transfers.")));
        RowDefaultGroup().Title(winrt::to_hstring(_("Default Group Color")));
        RowDefaultGroup().Description(winrt::to_hstring(_("A change in this setting will only be applied to newly added groups.")));
        RowCheckingAccount().Title(winrt::to_hstring(_("Checking Account Color")));
        RowSavingsAccount().Title(winrt::to_hstring(_("Savings Account Color")));
        RowBusinessAccount().Title(winrt::to_hstring(_("Business Account Color")));
        LblLocale().Text(winrt::to_hstring(_("Locale")));
        RowInsertSeparator().Title(winrt::to_hstring(_("Insert Decimal Separator")));
        RowInsertSeparator().Description(winrt::to_hstring(_("For the selected keys, the account's decimal separator will be inserted instead of the keyboard's value.")));
        CmbInsertSeparator().Items().Append(winrt::box_value(winrt::to_hstring(_("Off"))));
        CmbInsertSeparator().Items().Append(winrt::box_value(winrt::to_hstring(_("Numpad period only"))));
        CmbInsertSeparator().Items().Append(winrt::box_value(winrt::to_hstring(_("Period and Comma"))));
    }

    void SettingsPage::SetController(const std::shared_ptr<PreferencesViewController>& controller)
    {
        m_controller = controller;
        //Load
        m_constructing = true;
        CmbTheme().SelectedIndex(static_cast<int>(m_controller->getTheme()));
        TglAutomaticallyCheckForUpdates().IsOn(m_controller->getAutomaticallyCheckForUpdates());
        ClrDefaultTransaction().Color(Windows::UI::ColorHelper::FromArgb(m_controller->getTransactionDefaultColor().getA(), m_controller->getTransactionDefaultColor().getR(), m_controller->getTransactionDefaultColor().getG(), m_controller->getTransactionDefaultColor().getB()));
        ClrDefaultTransfer().Color(Windows::UI::ColorHelper::FromArgb(m_controller->getTransferDefaultColor().getA(), m_controller->getTransferDefaultColor().getR(), m_controller->getTransferDefaultColor().getG(), m_controller->getTransferDefaultColor().getB()));
        ClrDefaultGroup().Color(Windows::UI::ColorHelper::FromArgb(m_controller->getGroupDefaultColor().getA(), m_controller->getGroupDefaultColor().getR(), m_controller->getGroupDefaultColor().getG(), m_controller->getGroupDefaultColor().getB()));
        ClrCheckingAccount().Color(Windows::UI::ColorHelper::FromArgb(m_controller->getAccountCheckingColor().getA(), m_controller->getAccountCheckingColor().getR(), m_controller->getAccountCheckingColor().getG(), m_controller->getAccountCheckingColor().getB()));
        ClrSavingsAccount().Color(Windows::UI::ColorHelper::FromArgb(m_controller->getAccountSavingsColor().getA(), m_controller->getAccountSavingsColor().getR(), m_controller->getAccountSavingsColor().getG(), m_controller->getAccountSavingsColor().getB()));
        ClrBusinessAccount().Color(Windows::UI::ColorHelper::FromArgb(m_controller->getAccountBusinessColor().getA(), m_controller->getAccountBusinessColor().getR(), m_controller->getAccountBusinessColor().getG(), m_controller->getAccountBusinessColor().getB()));
        CmbInsertSeparator().SelectedIndex(static_cast<int>(m_controller->getInsertSeparator()));
        m_constructing = false;
    }

    void SettingsPage::OnCmbChanged(const IInspectable& sender, const SelectionChangedEventArgs& args)
    {
        ApplyChanges();
    }

    void SettingsPage::OnSwitchToggled(const IInspectable& sender, const RoutedEventArgs& args)
    {
        ApplyChanges();
    }

    void SettingsPage::OnColorChanged(const ColorPicker& sender, const ColorChangedEventArgs& args)
    {
        RctDefaultTransaction().Fill(SolidColorBrush{ ClrDefaultTransaction().Color() });
        RctDefaultTransfer().Fill(SolidColorBrush{ ClrDefaultTransfer().Color() });
        RctDefaultGroup().Fill(SolidColorBrush{ ClrDefaultGroup().Color() });
        RctCheckingAccount().Fill(SolidColorBrush{ ClrCheckingAccount().Color() });
        RctSavingsAccount().Fill(SolidColorBrush{ ClrSavingsAccount().Color() });
        RctBusinessAccount().Fill(SolidColorBrush{ ClrBusinessAccount().Color() });
        ApplyChanges();
    }

    void SettingsPage::ApplyChanges()
    {
        if(!m_constructing)
        {
            if(m_controller->getTheme() != static_cast<Theme>(CmbTheme().SelectedIndex()))
            {
                m_controller->setTheme(static_cast<Theme>(CmbTheme().SelectedIndex()));
            }
            m_controller->setAutomaticallyCheckForUpdates(TglAutomaticallyCheckForUpdates().IsOn());
            m_controller->setTransactionDefaultColor({ ClrDefaultTransaction().Color().A, ClrDefaultTransaction().Color().R, ClrDefaultTransaction().Color().G, ClrDefaultTransaction().Color().B });
            m_controller->setTransferDefaultColor({ ClrDefaultTransfer().Color().A, ClrDefaultTransfer().Color().R, ClrDefaultTransfer().Color().G, ClrDefaultTransfer().Color().B });
            m_controller->setGroupDefaultColor({ ClrDefaultGroup().Color().A, ClrDefaultGroup().Color().R, ClrDefaultGroup().Color().G, ClrDefaultGroup().Color().B });
            m_controller->setAccountCheckingColor({ ClrCheckingAccount().Color().A, ClrCheckingAccount().Color().R, ClrCheckingAccount().Color().G, ClrCheckingAccount().Color().B });
            m_controller->setAccountSavingsColor({ ClrSavingsAccount().Color().A, ClrSavingsAccount().Color().R, ClrSavingsAccount().Color().G, ClrSavingsAccount().Color().B });
            m_controller->setAccountBusinessColor({ ClrBusinessAccount().Color().A, ClrBusinessAccount().Color().R, ClrBusinessAccount().Color().G, ClrBusinessAccount().Color().B });
            m_controller->setInsertSeparator(static_cast<InsertSeparatorTrigger>(CmbInsertSeparator().SelectedIndex()));
            m_controller->saveConfiguration();
        }
    }
}
