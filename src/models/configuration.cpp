#include "configuration.hpp"
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <sstream>
#include <adwaita.h>
#include <json/json.h>

using namespace NickvisionMoney::Models;

Configuration::Configuration() : m_configDir{ std::string(g_get_user_config_dir()) + "/Nickvision/NickvisionMoney/" }, m_locale{ setlocale(LC_ALL, nullptr) }, m_theme{ Theme::System }, m_recentAccount1{ "" }, m_recentAccount2{ "" }, m_recentAccount3{ "" }
{
    //Load Config File
    if(!std::filesystem::exists(m_configDir))
    {
        std::filesystem::create_directories(m_configDir);
    }
    std::ifstream configFile{ m_configDir + "config.json" };
    if(configFile.is_open())
    {
        Json::Value json;
        configFile >> json;
        m_theme = static_cast<Theme>(json.get("Theme", 0).asInt());
        m_recentAccount1 = json.get("RecentAccount1", "").asString();
        m_recentAccount2 = json.get("RecentAccount2", "").asString();
        m_recentAccount3 = json.get("RecentAccount3", "").asString();
    }
}

const std::locale& Configuration::getLocale() const
{
    return m_locale;
}

Theme Configuration::getTheme() const
{
    return m_theme;
}

void Configuration::setTheme(Theme theme)
{
    m_theme = theme;
}

const std::string& Configuration::getRecentAccount1() const
{
    return m_recentAccount1;
}

const std::string& Configuration::getRecentAccount2() const
{
    return m_recentAccount2;
}

const std::string& Configuration::getRecentAccount3() const
{
    return m_recentAccount3;
}

void Configuration::addRecentAccount(const std::string& newRecentAccount)
{
    if (newRecentAccount == m_recentAccount1)
    {
        return;
    }
    else if (newRecentAccount == m_recentAccount2)
    {
        std::string temp1 = m_recentAccount1;
        m_recentAccount1 = m_recentAccount2;
        m_recentAccount2 = temp1;
    }
    else if (newRecentAccount == m_recentAccount3)
    {
        std::string temp1 = m_recentAccount1;
        std::string temp2 = m_recentAccount2;
        m_recentAccount1 = m_recentAccount3;
        m_recentAccount2 = temp1;
        m_recentAccount3 = temp2;
    }
    else
    {
        m_recentAccount3 = m_recentAccount2;
        m_recentAccount2 = m_recentAccount1;
        m_recentAccount1 = newRecentAccount;
    }
}

void Configuration::save() const
{
    std::ofstream configFile{ m_configDir + "config.json" };
    if(configFile.is_open())
    {
        Json::Value json;
        json["Theme"] = static_cast<int>(m_theme);
        json["RecentAccount1"] = m_recentAccount1;
        json["RecentAccount2"] = m_recentAccount2;
        json["RecentAccount3"] = m_recentAccount3;
        configFile << json;
    }
}
