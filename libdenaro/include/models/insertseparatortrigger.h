#ifndef INSERTSEPARATOR_H
#define INSERTSEPARATOR_H

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief Triggers that allow for inserting decimal separators.
     */
    enum class InsertSeparatorTrigger
    {
        Off = 0,
        NumpadOnly,
        PeriodComma
    };
}

#endif //INSERTSEPARATOR_H