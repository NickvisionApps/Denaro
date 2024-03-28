#ifndef CURRENCYCHECKSTATUS_H
#define CURRENCYCHECKSTATUS_H

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief Statuses for when a currency is validated. 
     */
    enum class CurrencyCheckStatus
    {
        Valid = 1,
        EmptySymbol = 2,
        EmptyCode = 4,
        EmptyDecimalSeparator = 8,
        SameSeparators = 16,
        SameSymbolAndDecimalSeparator = 32,
        SameSymbolAndGroupSeparator = 64
    };
}

#endif //CURRENCYCHECKSTATUS_H