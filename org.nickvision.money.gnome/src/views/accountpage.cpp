#include "views/accountpage.h"
#include "helpers/builder.h"

using namespace Nickvision::Money::Shared::Controllers;

namespace Nickvision::Money::GNOME::Views
{
    AccountPage::AccountPage(const std::shared_ptr<AccountViewController>& controller, GtkWindow* parent)
        : m_controller(controller),
        m_builder{ BuilderHelpers::fromBlueprint("account_page") },
        m_parent{ parent },
        m_page{ ADW_VIEW_STACK(gtk_builder_get_object(m_builder, "root")) }
    {

    }

    AccountPage::~AccountPage()
    {
        g_object_unref(m_builder);
    }

    AdwViewStack* AccountPage::gobj()
    {
        return m_page;
    }

    const std::string& AccountPage::getTitle() const
    {
        return m_controller->getMetadata().getName();
    }
}
