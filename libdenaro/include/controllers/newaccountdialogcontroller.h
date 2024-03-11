#ifndef NEWACCOUNTDIALOGCONTROLLER_H
#define NEWACCOUNTDIALOGCONTROLLER_H

#include <filesystem>
#include <string>
#include <vector>

namespace Nickvision::Money::Shared::Controllers
{
    /**
     * @brief A controller for a NewAccountDialog. 
     */
    class NewAccountDialogController
    {
    public:
        /**
         * @brief Constructs a NewAccountDialogController.
         */
        NewAccountDialogController(const std::vector<std::filesystem::path>& openAccounts);

    private:
        std::vector<std::filesystem::path> m_openAccounts;
    };
}

#endif //NEWACCOUNTDIALOGCONTROLLER_H