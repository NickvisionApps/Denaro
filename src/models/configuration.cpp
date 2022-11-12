#include "configuration.hpp"
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <sstream>
#include <adwaita.h>
#include <json/json.h>

using namespace NickvisionMoney::Models;

Configuration::Configuration() : m_configDir{ std::string(g_get_user_config_dir()) + "/Nickvision/NickvisionMoney/" }, m_locale{ setlocale(LC_ALL, nullptr) }, m_theme{ Theme::System }
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

std::string Configuration::getCurrencySymbol() const
{
    return std::use_facet<std::moneypunct<char>>(m_locale).curr_symbol();
}

bool Configuration::getDisplayCurrencySymbolOnRight() const
{
    std::stringstream builder;
    builder.imbue(m_locale);
    builder << std::showbase << std::put_money("1.0");
    std::string monetaryValue{ builder.str() };
    return monetaryValue.substr(0, 1) != getCurrencySymbol();
}

void Configuration::save() const
{
    std::ofstream configFile{ m_configDir + "config.json" };
    if(configFile.is_open())
    {
        Json::Value json;
        json["Theme"] = static_cast<int>(m_theme);
        configFile << json;
    }
}
