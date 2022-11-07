#include "configuration.hpp"
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <locale>
#include <sstream>
#include <adwaita.h>
#include <json/json.h>

using namespace NickvisionMoney::Models;

Configuration::Configuration() : m_configDir{ std::string(g_get_user_config_dir()) + "/Nickvision/NickvisionMoney/" }, m_theme{ Theme::System }, m_currencySymbol{ "" }, m_displayCurrencySymbolOnRight{ false }
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
        std::locale locale{ setlocale(LC_ALL, nullptr) };
        std::stringstream builder;
        builder.imbue(locale);
        builder << std::put_money("1.0");
        std::string monetaryValue{ builder.str() };
        m_currencySymbol = json.get("CurrencySymbolV2", "").asString();
        if(m_currencySymbol.empty())
        {
            m_currencySymbol = std::use_facet<std::moneypunct<char>>(locale).curr_symbol();
        }
        m_displayCurrencySymbolOnRight = json.get("DisplayCurrencySymbolOnRightV2", monetaryValue.substr(0, 1) == "1").asBool();
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
    if(currencySymbol.empty())
    {
        std::locale locale{ setlocale(LC_ALL, nullptr) };
        m_currencySymbol = std::use_facet<std::moneypunct<char>>(locale).curr_symbol();
        return;
    }
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
        json["CurrencySymbolV2"] = m_currencySymbol;
        json["DisplayCurrencySymbolOnRightV2"] = m_displayCurrencySymbolOnRight;;
        configFile << json;
    }
}
