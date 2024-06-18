#ifndef REMINDERSTHRESHOLD_H
#define REMINDERSTHRESHOLD_H

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief Thresholds for when to show a reminder. 
     */
    enum class RemindersThreshold
    {
        Never = 0,
        OneDayBefore,
        OneWeekBefore,
        OneMonthBefore,
        TwoMonthsBefore
    };
}

#endif //REMINDERSTHRESHOLD_H