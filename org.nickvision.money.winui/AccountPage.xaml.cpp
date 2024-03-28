#include "AccountPage.xaml.h"
#if __has_include("AccountPage.g.cpp")
#include "AccountPage.g.cpp"
#endif
#include <libnick/localization/gettext.h>

using namespace ::Nickvision::Money::Shared::Controllers;
using namespace winrt::Microsoft::UI::Xaml;
using namespace winrt::Microsoft::UI::Xaml::Controls;

namespace winrt::Nickvision::Money::WinUI::implementation 
{
    AccountPage::AccountPage()
    {
        InitializeComponent();
    }

    void AccountPage::SetController(const std::shared_ptr<AccountViewController>& controller)
    {
        m_controller = controller;
        //Load
        LblTitle().Text(winrt::to_hstring(m_controller->getMetadata().getName()));
    }
}
