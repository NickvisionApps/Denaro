#include "SettingsPage.xaml.h"
#if __has_include("SettingsPage.g.cpp")
#include "SettingsPage.g.cpp"
#endif
#include <libnick/localization/gettext.h>

using namespace ::Nickvision::Money::Shared::Controllers;
using namespace ::Nickvision::Money::Shared::Models;
using namespace winrt::Microsoft::UI::Xaml;
using namespace winrt::Microsoft::UI::Xaml::Controls;

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
        RowDefaultTransfer().Description(winrt::to_hstring(_("A change in this setting will only be applied to newly added transactions.")));
        RowDefaultGroup().Title(winrt::to_hstring(_("Default Group Color")));
        RowDefaultGroup().Description(winrt::to_hstring(_("A change in this setting will only be applied to newly added groups.")));
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

    void SettingsPage::ApplyChanges()
    {
        if(!m_constructing)
        {
            if(m_controller->getTheme() != static_cast<Theme>(CmbTheme().SelectedIndex()))
            {
                m_controller->setTheme(static_cast<Theme>(CmbTheme().SelectedIndex()));
            }
            m_controller->setAutomaticallyCheckForUpdates(TglAutomaticallyCheckForUpdates().IsOn());
            m_controller->setInsertSeparator(static_cast<InsertSeparatorTrigger>(CmbInsertSeparator().SelectedIndex()));
            m_controller->saveConfiguration();
        }
    }
}
