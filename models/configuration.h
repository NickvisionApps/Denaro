#ifndef CONFIGURATION_H
#define CONFIGURATION_H

#include <string>

namespace NickvisionMoney::Models
{
    class Configuration
    {
    public:
        Configuration();
        bool rememberLastOpenedAccount() const;
        void setRememberLastOpenedAccount(bool rememberLastOpenedAccount);
        const std::string& getLastOpenedAccount() const;
        void setLastOpenedAccount(const std::string& lastOpenedAccount);
        void save() const;

    private:
        std::string m_configDir;
        bool m_rememberLastOpenedAccount;
        std::string m_lastOpenedAccount;
    };
}

#endif // CONFIGURATION_H
