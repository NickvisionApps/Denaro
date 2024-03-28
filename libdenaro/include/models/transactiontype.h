#ifndef TRANSACTIONTYPE_H
#define TRANSACTIONTYPE_H

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief Types of a transaction. 
     */
    enum class TransactionType
    {
        Income = 0,
        Expense
    };
}

#endif //TRANSACTIONTYPE_H