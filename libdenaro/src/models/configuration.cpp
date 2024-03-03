#include "models/configuration.h"

namespace Nickvision::Money::Shared::Models
{
    Configuration::Configuration(const std::string& key)
        : ConfigurationBase{ key }
    {
    }

    Theme Configuration::getTheme() const
    {
        return static_cast<Theme>(m_json.get("Theme", static_cast<int>(Theme::System)).asInt());
    }

    void Configuration::setTheme(Theme theme)
    {
        m_json["Theme"] = static_cast<int>(theme);
    }

    bool Configuration::getAutomaticallyCheckForUpdates() const
    {
#ifdef _WIN32
        bool def{ true };
#elif defined(__linux__)
        bool def{ false };
#endif
        return m_json.get("AutomaticallyCheckForUpdates", def).asBool();
    }

    void Configuration::setAutomaticallyCheckForUpdates(bool check)
    {
        m_json["AutomaticallyCheckForUpdates"] = check;
    }

    std::vector<RecentAccount> Configuration::getRecentAccounts()
    {
        std::vector<RecentAccount> recents;
        bool update{ false };
        RecentAccount one{ getRecentAccount(1) };
        if(one)
        {
            recents.push_back(one);
        }
        else
        {
            update = true;
        }
        RecentAccount two{ getRecentAccount(2) };
        if(two)
        {
            recents.push_back(two);
        }
        else
        {
            update = true;
        }
        RecentAccount three{ getRecentAccount(3) };
        if(three)
        {
            recents.push_back(three);
        }
        else
        {
            update = true;
        }
        if(update)
        {
            if(recents.size() == 0)
            {
                setRecentAccount(1, {});
                setRecentAccount(2, {});
                setRecentAccount(3, {});
            }
            else if(recents.size() == 1)
            {
                setRecentAccount(1, recents[0]);
                setRecentAccount(2, {});
                setRecentAccount(3, {});
            }
            else if(recents.size() == 2)
            {
                setRecentAccount(1, recents[0]);
                setRecentAccount(2, recents[1]);
                setRecentAccount(3, {});
            }
        }
        return recents;
    }

    void Configuration::addRecentAccount(const RecentAccount& recent)
    {
        if(recent == getRecentAccount(1))
        {
            setRecentAccount(1, recent);
        }
        else if(recent == getRecentAccount(2))
        {
            setRecentAccount(2, getRecentAccount(1));
            setRecentAccount(1, recent);
        }
        else
        {
            setRecentAccount(3, getRecentAccount(2));
            setRecentAccount(2, getRecentAccount(1));
            setRecentAccount(1, recent);
        }
    }

    void Configuration::removeRecentAccount(const RecentAccount& recent)
    {
        RecentAccount one{ getRecentAccount(1) };
        RecentAccount two{ getRecentAccount(2) };
        RecentAccount three{ getRecentAccount(3) };
        if(one != recent && two != recent && three != recent)
        {
            return;
        }
        setRecentAccount(1, {});
        setRecentAccount(2, {});
        setRecentAccount(3, {});
        if(three != recent)
        {
            addRecentAccount(three);
        }
        if(two != recent)
        {
            addRecentAccount(two);
        }
        if(one != recent)
        {
            addRecentAccount(one);
        }
    }

    Color Configuration::getTransactionDefaultColor() const
    {
        return { m_json.get("TransactionDefaultColor", "53,132,228").asString() };
    }

    void Configuration::setTransactionDefaultColor(const Color& color)
    {
        m_json["TransactionDefaultColor"] = color.toRGBAString(false);
    }

    Color Configuration::getTransferDefaultColor() const
    {
        return { m_json.get("TransferDefaultColor", "192,97,203").asString() };
    }

    void Configuration::setTransferDefaultColor(const Color& color)
    {
        m_json["TransferDefaultColor"] = color.toRGBAString(false);
    }

    Color Configuration::getGroupDefaultColor() const
    {
        return { m_json.get("GroupDefaultColor", "51,209,122").asString() };
    }

    void Configuration::setGroupDefaultColor(const Color& color)
    {
        m_json["GroupDefaultColor"] = color.toRGBAString(false);
    }

    Color Configuration::getAccountCheckingColor() const
    {
        return { m_json.get("AccountCheckingColor", "129,61,156").asString() };
    }

    void Configuration::setAccountCheckingColor(const Color& color)
    {
        m_json["AccountCheckingColor"] = color.toRGBAString(false);
    }

    Color Configuration::getAccountSavingsColor() const
    {
        return { m_json.get("AccountSavingsColor", "53,132,228").asString() };
    }

    void Configuration::setAccountSavingsColor(const Color& color)
    {
        m_json["AccountSavingsColor"] = color.toRGBAString(false);
    }

    Color Configuration::getAccountBusinessColor() const
    {
        return { m_json.get("AccountBusinessColor", "38,162,10").asString() };
    }

    void Configuration::setAccountBusinessColor(const Color& color)
    {
        m_json["AccountBusinessColor"] = color.toRGBAString(false);
    }

    InsertSeparatorTrigger Configuration::getInsertSeparator() const
    {
        return static_cast<InsertSeparatorTrigger>(m_json.get("InsertSeparator", static_cast<int>(InsertSeparatorTrigger::NumpadOnly)).asInt());
    }

    void Configuration::setInsertSeparator(InsertSeparatorTrigger separator)
    {
        m_json["InsertSeparator"] = static_cast<int>(separator);
    }

    bool Configuration::getShowGraphs() const
    {
        return m_json.get("ShowGraphs", true).asBool();
    }

    void Configuration::setShowGraphs(bool showGraphs)
    {
        m_json["ShowGraphs"] = showGraphs;
    }

    RecentAccount Configuration::getRecentAccount(int index) const
    {
        Json::Value recentAccount{ m_json["RecentAccount" + std::to_string(index)] };
        if(recentAccount)
        {
            RecentAccount recent{ recentAccount.get("Path", "").asString() };
            recent.setName(recentAccount.get("Name", "").asString());
            recent.setType(static_cast<AccountType>(recentAccount.get("Type", static_cast<int>(AccountType::Checking)).asInt()));
            return recent;
        }
        return {};
    }

    void Configuration::setRecentAccount(int index, const RecentAccount& recent)
    {
        std::string key{ "RecentAccount" + std::to_string(index) };
        m_json[key]["Path"] = recent.getPath().string();
        m_json[key]["Name"] = recent.getName();
        m_json[key]["Type"] = static_cast<int>(recent.getType());
    }
}