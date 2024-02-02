#ifndef ACCOUNTTYPE_H
#define ACCOUNTTYPE_H

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief Types of an account.
     */
    enum class AccountType
    {
        Checking = 0,
        Savings,
        Business
    };
}

#endif //ACCOUNTTYPE_H