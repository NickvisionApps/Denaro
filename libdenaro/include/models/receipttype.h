#ifndef RECEIPTTYPE_H
#define RECEIPTTYPE_H

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief Types of receipts that can be stored in a transaction. 
     */
    enum class ReceiptType : unsigned char
    {
        JPEG = 'J',
        PNG = 'P',
        PDF = 'D',
        Unknown = 'U'
    };
}

#endif //RECEIPTTYPE_H