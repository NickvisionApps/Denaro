#include "controllers/preferencesviewcontroller.h"
#include <libnick/app/aura.h>
#include "models/configuration.h"

using namespace Nickvision::Money::Shared::Models;
using namespace Nickvision::App;

namespace Nickvision::Money::Shared::Controllers
{
    const std::string& PreferencesViewController::getId() const
    {
        return Aura::getActive().getAppInfo().getId();
    }

    Theme PreferencesViewController::getTheme() const
    {
        return Aura::getActive().getConfig<Configuration>("config").getTheme();
    }

    void PreferencesViewController::setTheme(Theme theme)
    {
        Aura::getActive().getConfig<Configuration>("config").setTheme(theme);
    }

    bool PreferencesViewController::getAutomaticallyCheckForUpdates() const
    {
        return Aura::getActive().getConfig<Configuration>("config").getAutomaticallyCheckForUpdates();
    }

    void PreferencesViewController::setAutomaticallyCheckForUpdates(bool check)
    {
        Aura::getActive().getConfig<Configuration>("config").setAutomaticallyCheckForUpdates(check);
    }

    Color PreferencesViewController::getTransactionDefaultColor() const
    {
        return Aura::getActive().getConfig<Configuration>("config").getTransactionDefaultColor();
    }

    void PreferencesViewController::setTransactionDefaultColor(const Color& color)
    {
        Aura::getActive().getConfig<Configuration>("config").setTransactionDefaultColor(color);
    }

    Color PreferencesViewController::getTransferDefaultColor() const
    {
        return Aura::getActive().getConfig<Configuration>("config").getTransferDefaultColor();
    }

    void PreferencesViewController::setTransferDefaultColor(const Color& color)
    {
        Aura::getActive().getConfig<Configuration>("config").setTransferDefaultColor(color);
    }

    Color PreferencesViewController::getGroupDefaultColor() const
    {
        return Aura::getActive().getConfig<Configuration>("config").getGroupDefaultColor();
    }

    void PreferencesViewController::setGroupDefaultColor(const Color& color)
    {
        Aura::getActive().getConfig<Configuration>("config").setGroupDefaultColor(color);
    }

    Color PreferencesViewController::getAccountCheckingColor() const
    {
        return Aura::getActive().getConfig<Configuration>("config").getAccountCheckingColor();
    }

    void PreferencesViewController::setAccountCheckingColor(const Color& color)
    {
        Aura::getActive().getConfig<Configuration>("config").setAccountCheckingColor(color);
    }

    Color PreferencesViewController::getAccountSavingsColor() const
    {
        return Aura::getActive().getConfig<Configuration>("config").getAccountSavingsColor();
    }

    void PreferencesViewController::setAccountSavingsColor(const Color& color)
    {
        Aura::getActive().getConfig<Configuration>("config").setAccountSavingsColor(color);
    }

    Color PreferencesViewController::getAccountBusinessColor() const
    {
        return Aura::getActive().getConfig<Configuration>("config").getAccountBusinessColor();
    }

    void PreferencesViewController::setAccountBusinessColor(const Color& color)
    {
        Aura::getActive().getConfig<Configuration>("config").setAccountBusinessColor(color);
    }

    InsertSeparatorTrigger PreferencesViewController::getInsertSeparator() const
    {
        return Aura::getActive().getConfig<Configuration>("config").getInsertSeparator();
    }

    void PreferencesViewController::setInsertSeparator(InsertSeparatorTrigger trigger)
    {
        Aura::getActive().getConfig<Configuration>("config").setInsertSeparator(trigger);
    }

    void PreferencesViewController::saveConfiguration()
    {
        Aura::getActive().getConfig<Configuration>("config").save();
    }
}