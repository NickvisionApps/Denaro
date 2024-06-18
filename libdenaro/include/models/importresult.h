#ifndef IMPORTRESULT_H
#define IMPORTRESULT_H

#include <string>
#include <vector>

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief A model of the result from importing a file. 
     */
    class ImportResult
    {
    public:
        /**
         * @brief Constructs an ImportResult. 
         */
        ImportResult() = default;
        /**
         * @brief Gets whether or not the ImportResult object represents an empty value.
         * @return True if empty, else false 
         */
        bool empty() const;
        /**
         * @brief Gets the list of new transaction ids from the import.
         * @return The list of new transaction ids 
         */
        const std::vector<int>& getNewTransactionIds() const;
        /**
         * @brief Gets the list of new group ids from the import.
         * @return The list of new group ids 
         */
        const std::vector<int>& getNewGroupIds() const;
        /**
         * @brief Gets the list of new tags from the import.
         * @return The list of new tags
         */
        const std::vector<std::string>& getNewTags() const;
        /**
         * @brief Adds a transaction id to the new transaction ids list.
         * @brief This method will only add the transaction id if it doesn't already exist in the list.
         * @param id The transaction id to add 
         */
        void addTransaction(int id);
        /**
         * @brief Adds a group id to the new group ids list.
         * @brief This method will only add the group id if it doesn't already exist in the list.
         * @param id The group id to add 
         */
        void addGroup(int id);
        /**
         * @brief Adds a list of tags to the new tags list. 
         * @brief This method will only add tags if they doesn't already exist in the list.
         * @param tags The tags to add
         */
        void addTags(const std::vector<std::string>& tags);
        /**
         * @brief Gets whether or not the object is valid or not.
         * @return True if valid (!empty), else false 
         */
        operator bool() const;

    private:
        std::vector<int> m_newTransactions;
        std::vector<int> m_newGroups;
        std::vector<std::string> m_newTags;
    };
}

#endif //IMPORTRESULT_H