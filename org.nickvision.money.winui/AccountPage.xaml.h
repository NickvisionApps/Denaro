#ifndef ACCOUNTPAGE_H
#define ACCOUNTPAGE_H

#include "includes.h"
#include <memory>
#include "Controls/SettingsRow.g.h"
#include "AccountPage.g.h"
#include "controllers/accountviewcontroller.h"

namespace winrt::Nickvision::Money::WinUI::implementation 
{
    /**
     * @brief The page for displaying an account.
     */
    class AccountPage : public AccountPageT<AccountPage>
    {
    public:
        /**
         * @brief Constructs an AccountPage.
         */
        AccountPage();
        /**
         * @brief Sets the controller for the page.
         * @param controller The AccountViewController 
         */
        void SetController(const std::shared_ptr<::Nickvision::Money::Shared::Controllers::AccountViewController>& controller);

    private:
        std::shared_ptr<::Nickvision::Money::Shared::Controllers::AccountViewController> m_controller;
    };
}

namespace winrt::Nickvision::Money::WinUI::factory_implementation 
{
    class AccountPage : public AccountPageT<AccountPage, implementation::AccountPage>
    {

    };
}

#endif //ACCOUNTPAGE_H