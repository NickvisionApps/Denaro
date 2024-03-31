#include "views/accountpage.h"
#include <libnick/localization/gettext.h>
#include "helpers/builder.h"

using namespace Nickvision::Money::Shared::Controllers;
using namespace Nickvision::Money::Shared::Models;

namespace Nickvision::Money::GNOME::Views
{
    AccountPage::AccountPage(const std::shared_ptr<AccountViewController>& controller, GtkWindow* parent)
        : m_controller(controller),
        m_builder{ BuilderHelpers::fromBlueprint("account_page") },
        m_parent{ parent },
        m_page{ ADW_VIEW_STACK(gtk_builder_get_object(m_builder, "root")) }
    {
        //Load
        adw_preferences_group_set_title(ADW_PREFERENCES_GROUP(gtk_builder_get_object(m_builder, "overviewGroup")), m_controller->getMetadata().getName().c_str());
        gtk_label_set_label(GTK_LABEL(gtk_builder_get_object(m_builder, "overviewTotalLabel")), m_controller->getTotalAmountString().c_str());
        gtk_label_set_label(GTK_LABEL(gtk_builder_get_object(m_builder, "overviewIncomeLabel")), m_controller->getIncomeAmountString().c_str());
        gtk_label_set_label(GTK_LABEL(gtk_builder_get_object(m_builder, "overviewExpenseLabel")), m_controller->getExpenseAmountString().c_str());
        for(const std::pair<std::string, std::string>& pair : m_controller->getGroupBalanceStrings())
        {
            AdwActionRow* row{ ADW_ACTION_ROW(adw_action_row_new()) };
            adw_preferences_row_set_title(ADW_PREFERENCES_ROW(row), pair.first.c_str());
            adw_action_row_add_suffix(ADW_ACTION_ROW(row), gtk_label_new(pair.second.c_str()));
            adw_preferences_group_add(ADW_PREFERENCES_GROUP(gtk_builder_get_object(m_builder, "groupsGroup")), GTK_WIDGET(row));
        }   
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
