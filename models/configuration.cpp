#include "configuration.h"
#include <filesystem>
#include <fstream>
#include <unistd.h>
#include <pwd.h>
#include <json/json.h>

namespace NickvisionMoney::Models
{
    Configuration::Configuration() : m_configDir(std::string(getpwuid(getuid())->pw_dir) + "/.config/Nickvision/NickvisionMoney/"), m_rememberLastOpenedAccount(true), m_lastOpenedAccount("")
    {
        if (!std::filesystem::exists(m_configDir))
        {
            std::filesystem::create_directories(m_configDir);
        }
        std::ifstream configFile(m_configDir + "config.json");
        if (configFile.is_open())
        {
            Json::Value json;
            try
            {
                configFile >> json;
                setRememberLastOpenedAccount(json.get("RememberLastOpenedAccount", true).asBool());
                setLastOpenedAccount(json.get("LastOpenedAccount", "").asString());
            }
            catch (...) { }
        }
    }

    bool Configuration::rememberLastOpenedAccount() const
    {
        return m_rememberLastOpenedAccount;
    }

    void Configuration::setRememberLastOpenedAccount(bool rememberLastOpenedAccount)
    {
        m_rememberLastOpenedAccount = rememberLastOpenedAccount;
    }

    const std::string& Configuration::getLastOpenedAccount() const
    {
        return m_lastOpenedAccount;
    }

    void Configuration::setLastOpenedAccount(const std::string& lastOpenedAccount)
    {
        m_lastOpenedAccount = lastOpenedAccount;
    }

    void Configuration::save() const
    {
        std::ofstream configFile(m_configDir + "config.json");
        if (configFile.is_open())
        {
            Json::Value json;
            json["RememberLastOpenedAccount"] = rememberLastOpenedAccount();
            json["LastOpenedAccount"] = getLastOpenedAccount();
            configFile << json;
        }
    }
}
