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

    void PreferencesViewController::saveConfiguration()
    {
        Aura::getActive().getConfig<Configuration>("config").save();
    }
}