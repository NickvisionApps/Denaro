#ifndef AMOUNTSTYLE_H
#define AMOUNTSTYLE_H

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief Styles for displaying an amount. 
     */
    enum class AmountStyle
    {
        /**
         * @brief $n 
         */
        SignNumber = 0,
        /**
         * @brief n$ 
         */
        NumberSign,
        /**
         * @brief $ n 
         */
        SignSpaceNumber,
        /**
         * @brief n $
         */
        NumberSpaceSign
    };
}

#endif //AMOUNTSTYLE_H