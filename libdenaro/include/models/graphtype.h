#ifndef GRAPHTYPE_H
#define GRAPHTYPE_H

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief Types of graphs that can be generated.
     */
    enum class GraphType
    {
        IncomeExpensePie = 0,
        IncomeExpensePerGroup,
        IncomeExpenseOverTime,
        IncomeByGroup,
        ExpenseByGroup
    };
}

#endif //GRAPHTYPE_H