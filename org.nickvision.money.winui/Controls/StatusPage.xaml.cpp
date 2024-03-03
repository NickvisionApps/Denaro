#include "Controls/StatusPage.xaml.h"
#if __has_include("Controls/StatusPage.g.cpp")
#include "Controls/StatusPage.g.cpp"
#endif
#include "Helpers/WinUI.h"

using namespace winrt::Microsoft::UI::Xaml;
using namespace winrt::Microsoft::UI::Xaml::Data;
using namespace winrt::Windows::Foundation::Collections;

namespace winrt::Nickvision::Money::WinUI::Controls::implementation 
{
    DependencyProperty StatusPage::m_glyphProperty = DependencyProperty::Register(L"Glyph", winrt::xaml_typename<winrt::hstring>(), winrt::xaml_typename<Nickvision::Money::WinUI::Controls::StatusPage>(), PropertyMetadata{ winrt::box_value(L""), PropertyChangedCallback{ &StatusPage::OnPropertyChanged } });
    DependencyProperty StatusPage::m_useAppIconProperty = DependencyProperty::Register(L"UseAppIcon", winrt::xaml_typename<bool>(), winrt::xaml_typename<Nickvision::Money::WinUI::Controls::StatusPage>(), PropertyMetadata{ winrt::box_value(false), PropertyChangedCallback{ &StatusPage::OnPropertyChanged } });
    DependencyProperty StatusPage::m_titleProperty = DependencyProperty::Register(L"Title", winrt::xaml_typename<winrt::hstring>(), winrt::xaml_typename<Nickvision::Money::WinUI::Controls::StatusPage>(), PropertyMetadata{ winrt::box_value(L""), PropertyChangedCallback{ &StatusPage::OnPropertyChanged } });
    DependencyProperty StatusPage::m_descriptionProperty = DependencyProperty::Register(L"Description", winrt::xaml_typename<winrt::hstring>(), winrt::xaml_typename<Nickvision::Money::WinUI::Controls::StatusPage>(), PropertyMetadata{ winrt::box_value(L""), PropertyChangedCallback{ &StatusPage::OnPropertyChanged } });
    DependencyProperty StatusPage::m_childProperty = DependencyProperty::Register(L"Child", winrt::xaml_typename<IInspectable>(), winrt::xaml_typename<Nickvision::Money::WinUI::Controls::StatusPage>(), PropertyMetadata{ nullptr, PropertyChangedCallback{ &StatusPage::OnPropertyChanged } });
    DependencyProperty StatusPage::m_isCompactProperty = DependencyProperty::Register(L"IsCompact", winrt::xaml_typename<bool>(), winrt::xaml_typename<Nickvision::Money::WinUI::Controls::StatusPage>(), PropertyMetadata{ winrt::box_value(false), PropertyChangedCallback{ &StatusPage::OnPropertyChanged } });

    StatusPage::StatusPage()
    {
        InitializeComponent();
        Title(L"");
        Description(L"");
        Child(nullptr);
        IsCompact(false);
    }
    
    winrt::hstring StatusPage::Glyph() const
    {
        return winrt::unbox_value<winrt::hstring>(GetValue(m_glyphProperty));
    }

    void StatusPage::Glyph(const winrt::hstring& glyph)
    {
        SetValue(m_glyphProperty, winrt::box_value(glyph));
        if(!glyph.empty())
        {
            GlyphIcon().Visibility(Visibility::Visible);
            AppIcon().Visibility(Visibility::Collapsed);
        }
        m_propertyChanged(*this, PropertyChangedEventArgs{ L"Glyph" });
    }
    
    bool StatusPage::UseAppIcon() const
    {
        return winrt::unbox_value<bool>(GetValue(m_useAppIconProperty));
    }

    void StatusPage::UseAppIcon(bool useAppIcon)
    {
        SetValue(m_useAppIconProperty, winrt::box_value(useAppIcon));
        if(useAppIcon)
        {
            GlyphIcon().Visibility(Visibility::Collapsed);
            AppIcon().Visibility(Visibility::Visible);
        }
        m_propertyChanged(*this, PropertyChangedEventArgs{ L"UseAppIcon" });
    }

    winrt::hstring StatusPage::Title() const
    {
        return winrt::unbox_value<winrt::hstring>(GetValue(m_titleProperty));
    }

    void StatusPage::Title(const winrt::hstring& title)
    {
        SetValue(m_titleProperty, winrt::box_value(title));
        LblTitle().Visibility(!title.empty() ? Visibility::Visible : Visibility::Collapsed);
        m_propertyChanged(*this, PropertyChangedEventArgs{ L"Title" });
    }

    winrt::hstring StatusPage::Description() const
    {
        return winrt::unbox_value<winrt::hstring>(GetValue(m_descriptionProperty));
    }

    void StatusPage::Description(const winrt::hstring& description)
    {
        SetValue(m_descriptionProperty, winrt::box_value(description));
        LblDescription().Visibility(!description.empty() ? Visibility::Visible : Visibility::Collapsed);
        m_propertyChanged(*this, PropertyChangedEventArgs{ L"Description" });
    }

    IInspectable StatusPage::Child() const
    {
        return GetValue(m_childProperty);
    }

    void StatusPage::Child(const IInspectable& child)
    {
        SetValue(m_childProperty, child);
        FrameChild().Visibility(child ? Visibility::Visible : Visibility::Collapsed);
        m_propertyChanged(*this, PropertyChangedEventArgs{ L"Child" });
    }

    bool StatusPage::IsCompact() const
    {
        return winrt::unbox_value<bool>(GetValue(m_isCompactProperty));
    }

    void StatusPage::IsCompact(bool isCompact)
    {
        SetValue(m_isCompactProperty, winrt::box_value(isCompact));
        if(isCompact)
        {
            StackPanel().Spacing(6);
            GlyphIcon().FontSize(30);
            AppIcon().Width(64);
            AppIcon().Height(64);
            LblTitle().Style(WinUIHelpers::LookupAppResource<Microsoft::UI::Xaml::Style>(L"SubtitleTextBlockStyle"));
        }
        else
        {
            StackPanel().Spacing(12);
            GlyphIcon().FontSize(60);
            AppIcon().Width(128);
            AppIcon().Height(128);
            LblTitle().Style(WinUIHelpers::LookupAppResource<Microsoft::UI::Xaml::Style>(L"TitleTextBlockStyle"));
        }
        m_propertyChanged(*this, PropertyChangedEventArgs{ L"IsCompact" });
    }

    winrt::event_token StatusPage::PropertyChanged(const PropertyChangedEventHandler& handler)
    {
        return m_propertyChanged.add(handler);
    }

    void StatusPage::PropertyChanged(const winrt::event_token& token)
    {
        m_propertyChanged.remove(token);
    }

    const DependencyProperty& StatusPage::GlyphProperty()
    {
        return m_glyphProperty;
    }

    const DependencyProperty& StatusPage::UseAppIconProperty()
    {
        return m_useAppIconProperty;
    }

    const DependencyProperty& StatusPage::TitleProperty()
    {
        return m_titleProperty;
    }

    const DependencyProperty& StatusPage::DescriptionProperty()
    {
        return m_descriptionProperty;
    }

    const DependencyProperty& StatusPage::ChildProperty()
    {
        return m_childProperty;
    }

    const DependencyProperty& StatusPage::IsCompactProperty()
    {
        return m_isCompactProperty;
    }

    void StatusPage::OnPropertyChanged(const DependencyObject& d, const DependencyPropertyChangedEventArgs& args)
    {
        if(Nickvision::Money::WinUI::Controls::StatusPage statusPage{ d.try_as<Nickvision::Money::WinUI::Controls::StatusPage>() })
        {
            StatusPage* ptr{ winrt::get_self<StatusPage>(statusPage) };
            if(args.Property() == m_glyphProperty)
            {
                ptr->m_propertyChanged(*ptr, PropertyChangedEventArgs{ L"Glyph" });
            }
            else if(args.Property() == m_useAppIconProperty)
            {
                ptr->m_propertyChanged(*ptr, PropertyChangedEventArgs{ L"UseAppIcon" });
            }
            else if(args.Property() == m_titleProperty)
            {
                ptr->m_propertyChanged(*ptr, PropertyChangedEventArgs{ L"Title" });
            }
            else if(args.Property() == m_descriptionProperty)
            {
                ptr->m_propertyChanged(*ptr, PropertyChangedEventArgs{ L"Description" });
            }
            else if(args.Property() == m_childProperty)
            {
                ptr->m_propertyChanged(*ptr, PropertyChangedEventArgs{ L"Child" });
            }
            else if(args.Property() == m_isCompactProperty)
            {
                ptr->m_propertyChanged(*ptr, PropertyChangedEventArgs{ L"IsCompact" });
            }
        }
    }
}