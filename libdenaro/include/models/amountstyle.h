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
        SymbolNumber = 0,
        /**
         * @brief n$ 
         */
        NumberSymbol,
        /**
         * @brief $ n 
         */
        SymbolSpaceNumber,
        /**
         * @brief n $
         */
        NumberSpaceSymbol
    };
}

#endif //AMOUNTSTYLE_H