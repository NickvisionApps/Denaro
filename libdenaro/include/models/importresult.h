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
        const std::vector<unsigned int>& getNewTransactionIds() const;
        /**
         * @brief Gets the list of new group ids from the import.
         * @return The list of new group ids 
         */
        const std::vector<unsigned int>& getNewGroupIds() const;
        /**
         * @brief Gets the list of new tags from the import.
         * @return The list of new tags
         */
        const std::vector<std::string>& getNewTags() const;
        /**
         * @brief Adds an id to the new transaction ids list.
         * @brief This method will only add the id if it doesn't already exist in the list.
         * @param id The id to add 
         */
        void addTransactionId(unsigned int id);
        /**
         * @brief Adds an id to the new group ids list.
         * @brief This method will only add the id if it doesn't already exist in the list.
         * @param id The id to add 
         */
        void addGroupId(unsigned int id);
        /**
         * @brief Adds a list of tags to the new tags list.
         * @brief This method will only add non-existing tags. Existing tags will be skipped over to avoid duplicates.
         * @param tags The list of tags to add 
         */
        void addTags(const std::vector<std::string>& tags);
        /**
         * @brief Gets whether or not the object is valid or not.
         * @return True if valid (!empty), else false 
         */
        operator bool() const;

    private:
        std::vector<unsigned int> m_newTransactionIds;
        std::vector<unsigned int> m_newGroupIds;
        std::vector<std::string> m_newTags;
    };
}

#endif //IMPORTRESULT_H