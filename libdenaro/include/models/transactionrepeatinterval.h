#ifndef TRANSACTIONREPEATINTERVAL_H
#define TRANSACTIONREPEATINTERVAL_H

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief Repeat intervals for a transaction. 
     */
    enum class TransactionRepeatInterval
    {
        Never = 0,
        Daily = 1,
        Weekly = 2,
        Biweekly = 7,
        Monthly = 3,
        Quarterly = 4,
        Yearly = 5,
        Biyearly = 6
    };
}

#endif //TRANSACTIONREPEATINTERVAL_H