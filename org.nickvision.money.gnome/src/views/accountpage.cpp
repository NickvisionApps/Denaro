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
        //Load overview amounts
        adw_preferences_group_set_title(ADW_PREFERENCES_GROUP(gtk_builder_get_object(m_builder, "overviewGroup")), m_controller->getMetadata().getName().c_str());
        //Load account total
        Color totalColor{ "#3584e4" };
        GdkPixbuf* totalColorBuf{ gdk_pixbuf_new(GDK_COLORSPACE_RGB, false, 8, 1, 1) };
        gdk_pixbuf_fill(totalColorBuf, totalColor.toHex(false));
        GdkTexture* totalColorTexture{ gdk_texture_new_for_pixbuf(totalColorBuf) };
        adw_avatar_set_custom_image(ADW_AVATAR(gtk_builder_get_object(m_builder, "overviewTotalAvatar")), GDK_PAINTABLE(totalColorTexture));
        gtk_label_set_label(GTK_LABEL(gtk_builder_get_object(m_builder, "overviewTotalLabel")), m_controller->getTotalAmountString().c_str());
        g_object_unref(totalColorTexture);
        g_object_unref(totalColorBuf);
        //Load account income
        Color incomeColor{ "#26a269" };
        GdkPixbuf* incomeColorBuf{ gdk_pixbuf_new(GDK_COLORSPACE_RGB, false, 8, 1, 1) };
        gdk_pixbuf_fill(incomeColorBuf, incomeColor.toHex(false));
        GdkTexture* incomeColorTexture{ gdk_texture_new_for_pixbuf(incomeColorBuf) };
        adw_avatar_set_custom_image(ADW_AVATAR(gtk_builder_get_object(m_builder, "overviewIncomeAvatar")), GDK_PAINTABLE(incomeColorTexture));
        gtk_label_set_label(GTK_LABEL(gtk_builder_get_object(m_builder, "overviewIncomeLabel")), m_controller->getIncomeAmountString().c_str());
        g_object_unref(incomeColorTexture);
        g_object_unref(incomeColorBuf);
        //Load account expense
        Color expenseColor{ "#c01c28" };
        GdkPixbuf* expenseColorBuf{ gdk_pixbuf_new(GDK_COLORSPACE_RGB, false, 8, 1, 1) };
        gdk_pixbuf_fill(expenseColorBuf, expenseColor.toHex(false));
        GdkTexture* expenseColorTexture{ gdk_texture_new_for_pixbuf(expenseColorBuf) };
        adw_avatar_set_custom_image(ADW_AVATAR(gtk_builder_get_object(m_builder, "overviewExpenseAvatar")), GDK_PAINTABLE(expenseColorTexture));
        gtk_label_set_label(GTK_LABEL(gtk_builder_get_object(m_builder, "overviewExpenseLabel")), m_controller->getExpenseAmountString().c_str());
        g_object_unref(expenseColorTexture);
        g_object_unref(expenseColorBuf);
        //Load overview groups
        for(const std::pair<Group, std::string>& pair : m_controller->getGroups())
        {
            GdkPixbuf* colorBuf{ gdk_pixbuf_new(GDK_COLORSPACE_RGB, false, 8, 1, 1) };
            gdk_pixbuf_fill(colorBuf, pair.first.getColor().toHex(false));
            GdkTexture* colorTexture{ gdk_texture_new_for_pixbuf(colorBuf) };
            AdwAvatar* colorAvatar{ ADW_AVATAR(adw_avatar_new(8, nullptr, false)) };
            gtk_widget_set_valign(GTK_WIDGET(colorAvatar), GTK_ALIGN_CENTER);
            adw_avatar_set_custom_image(ADW_AVATAR(colorAvatar), GDK_PAINTABLE(colorTexture));
            AdwActionRow* row{ ADW_ACTION_ROW(adw_action_row_new()) };
            adw_preferences_row_set_title(ADW_PREFERENCES_ROW(row), pair.first.getName().c_str());
            if(!pair.first.getDescription().empty())
            {
                adw_action_row_set_subtitle(row, pair.first.getDescription().c_str());
            }
            adw_action_row_add_prefix(ADW_ACTION_ROW(row), GTK_WIDGET(colorAvatar));
            adw_action_row_add_suffix(ADW_ACTION_ROW(row), gtk_label_new(pair.second.c_str()));
            if(pair.first.getName() != _("Ungrouped"))
            {
                GtkButton* editButton{ GTK_BUTTON(gtk_button_new_from_icon_name("document-edit-symbolic")) };
                gtk_widget_set_valign(GTK_WIDGET(editButton), GTK_ALIGN_CENTER);
                gtk_widget_add_css_class(GTK_WIDGET(editButton), "flat");
                adw_action_row_add_suffix(ADW_ACTION_ROW(row), GTK_WIDGET(editButton));
                adw_action_row_set_activatable_widget(ADW_ACTION_ROW(row), GTK_WIDGET(editButton));
            }
            adw_preferences_group_add(ADW_PREFERENCES_GROUP(gtk_builder_get_object(m_builder, "groupsGroup")), GTK_WIDGET(row));
            g_object_unref(colorTexture);
            g_object_unref(colorBuf);
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
