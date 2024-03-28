#ifndef SORTBY_H
#define SORTBY_H

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief Ways for sorting transactions. 
     */
    enum class SortBy
    {
        Id = 0,
        Date,
        Amount
    };
}

#endif //SORTBY_H