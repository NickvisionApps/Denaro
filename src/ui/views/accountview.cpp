#include "accountview.hpp"

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::UI::Views;

AccountView::AccountView(AdwTabView* parent, const AccountViewController& controller) : m_controller{ controller }
{
    //Main Box
    m_boxMain = gtk_box_new(GTK_ORIENTATION_VERTICAL, 0);
    //Tab Page
    m_gobj = adw_tab_view_append(parent, m_boxMain);
    adw_tab_page_set_title(m_gobj, m_controller.getAccountPath().c_str());
}

AdwTabPage* AccountView::gobj()
{
    return m_gobj;
}
