#ifndef SETTINGSPAGE_H
#define SETTINGSPAGE_H

#include "includes.h"
#include <memory>
#include "Controls/SettingsRow.g.h"
#include "SettingsPage.g.h"
#include "controllers/preferencesviewcontroller.h"

namespace winrt::Nickvision::Money::WinUI::implementation 
{
    /**
     * @brief The settings page for the application. 
     */
    class SettingsPage : public SettingsPageT<SettingsPage>
    {
    public:
        /**
         * @brief Constructs a SettingsPage.
         */
        SettingsPage();
        /**
         * @brief Sets the controller for the page.
         * @param controller The PreferencesViewController 
         */
        void SetController(const std::shared_ptr<::Nickvision::Money::Shared::Controllers::PreferencesViewController>& controller);
        /**
         * @brief Handles when the theme preference is changed. 
         */
        void OnThemeChanged(const IInspectable& sender, const Microsoft::UI::Xaml::Controls::SelectionChangedEventArgs& args);
        /**
         * @brief Handles when the update preference is changed. 
         */
        void OnUpdatesToggled(const IInspectable& sender, const Microsoft::UI::Xaml::RoutedEventArgs& args);

    private:
        /**
         * @brief Applies changes the application's configuration.
         */
        void ApplyChanges();
        std::shared_ptr<::Nickvision::Money::Shared::Controllers::PreferencesViewController> m_controller;
        bool m_constructing;
    };
}

namespace winrt::Nickvision::Money::WinUI::factory_implementation 
{
    class SettingsPage : public SettingsPageT<SettingsPage, implementation::SettingsPage>
    {

    };
}

#endif //SETTINGSPAGE_H