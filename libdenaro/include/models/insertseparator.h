#ifndef INSERTSEPARATOR_H
#define INSERTSEPARATOR_H

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief Keys that allow for inserting decimal separator.
     */
    enum class InsertSeparator
    {
        Off = 0,
        NumpadOnly,
        PeriodComma
    };
}

#endif //INSERTSEPARATOR_H