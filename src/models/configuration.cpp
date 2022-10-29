#include "configuration.hpp"
#include <filesystem>
#include <fstream>
#include <adwaita.h>
#include <json/json.h>

using namespace NickvisionMoney::Models;

Configuration::Configuration() : m_configDir{ std::string(g_get_user_config_dir()) + "/Nickvision/NickvisionMoney/" }, m_theme{ Theme::System }, m_currencySymbol{ "$" }, m_displayCurrencySymbolOnRight{ false }
{
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
        m_currencySymbol = json.get("CurrencySymbol", "$").asString();
        m_displayCurrencySymbolOnRight = json.get("DisplayCurrencySymbolOnRight", false).asBool();
    }
}

Theme Configuration::getTheme() const
{
    return m_theme;
}

void Configuration::setTheme(Theme theme)
{
    m_theme = theme;
}

const std::string& Configuration::getCurrencySymbol() const
{
    return m_currencySymbol;
}

void Configuration::setCurrencySymbol(const std::string& currencySymbol)
{
    m_currencySymbol = currencySymbol;
}

bool Configuration::getDisplayCurrencySymbolOnRight() const
{
    return m_displayCurrencySymbolOnRight;
}

void Configuration::setDisplayCurrencySymbolOnRight(bool displayCurrencySymbolOnRight)
{
    m_displayCurrencySymbolOnRight = displayCurrencySymbolOnRight;
}

void Configuration::save() const
{
    std::ofstream configFile{ m_configDir + "config.json" };
    if(configFile.is_open())
    {
        Json::Value json;
        json["Theme"] = static_cast<int>(m_theme);
        json["CurrencySymbol"] = m_currencySymbol;
        json["DisplayCurrencySymbolOnRight"] = m_displayCurrencySymbolOnRight;;
        configFile << json;
    }
}
