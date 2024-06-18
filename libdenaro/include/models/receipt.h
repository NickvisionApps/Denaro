#ifndef RECEIPT_H
#define RECEIPT_H

#include <cstdint>
#include <filesystem>
#include <string>
#include <vector>
#include "receipttype.h"

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief A model for a receipt.
     * @brief A receipt supports binary files of type JPEG, PNG, and PDF.
     */
    class Receipt
    {
    public:
        /**
         * @brief Constructs a Receipt. 
         */
        Receipt();
        /**
         * @brief Constructs a Receipt.
         * @brief If the file does not exist, an empty receipt will be constructed.
         * @param pathToFile The path to a file to load into the receipt
         */
        Receipt(const std::filesystem::path& pathToFile);
        /**
         * @brief Constructs a Receipt.
         * @param bytes A vector of bytes representing the Receipt object. bytes[0] should contain the ReceiptType byte.
         * @throw std::invalid_argument Thrown if the vector of bytes is not a valid Receipt object
         */
        Receipt(const std::vector<std::uint8_t>& bytes);
        /**
         * @brief Constructs a Receipt.
         * @param base64 A base64 encoded string representing the Receipt object.
         * @throw std::invalid_argument Thrown if the base64 string is not a valid Receipt object
         */
        Receipt(const std::string& base64);
        /**
         * @brief Gets whether or not the Receipt object represents an empty value.
         * @return True if empty, else false 
         */
        bool empty() const;
        /**
         * @brief Gets the type of the receipt.
         * @return The receipt type 
         */
        ReceiptType getType() const;
        /**
         * @brief Gets the base64 encoded string representing this Receipt object.
         * @return The base64 string of this Receipt 
         */
        const std::string& toString() const;
        /**
         * @brief Saves this Receipt to a file on disk.
         * @brief If the pathToFile does not contain the correct extension for this Receipt's type, it will be added by this method.
         * @param pathToFile The path to the file to save the receipt to
         * @param overwrite Whether or not to overwrite an existing file @ pathToFile
         * @return True if successful, else false 
         */
        bool saveToDisk(const std::filesystem::path& pathToFile, bool overwrite = true) const;
        /**
         * @brief Gets whether or not the object is valid or not.
         * @return True if valid (!empty), else false 
         */
        operator bool() const;

    private:
        ReceiptType m_type;
        std::vector<std::uint8_t> m_bytes;
        std::string m_base64;
    };
}

#endif //RECEIPT_H