#include "models/receipt.h"
#include <fstream>
#include <iterator>
#include <stdexcept>
#include <libnick/helpers/stringhelpers.h>

namespace Nickvision::Money::Shared::Models
{
    Receipt::Receipt()
        : m_type{ ReceiptType::Unknown }
    {
        
    }

    Receipt::Receipt(const std::filesystem::path& pathToFile)
        : m_type{ ReceiptType::Unknown }
    {
        if(std::filesystem::exists(pathToFile))
        {
            //Get type
            std::string ext{ StringHelpers::toLower(pathToFile.extension().string()) };
            if(ext == ".jpeg" || ext == ".jpg")
            {
                m_type = ReceiptType::JPEG;
            }
            else if(ext == ".png")
            {
                m_type = ReceiptType::PNG;
            }
            else if(ext == ".pdf")
            {
                m_type = ReceiptType::PDF;
            }
            else
            {
                m_type = ReceiptType::Unknown;
            }
            //Get bytes
            std::ifstream in{ pathToFile, std::ios_base::binary };
            m_bytes = { std::istreambuf_iterator<char>(in), std::istreambuf_iterator<char>() };
            //Encode type + bytes
            std::vector<std::uint8_t> bytes;
            bytes.push_back(static_cast<std::uint8_t>(m_type));
            bytes.insert(bytes.end(), m_bytes.begin(), m_bytes.end());
            m_base64 = StringHelpers::toBase64(bytes);
        }
    }

    Receipt::Receipt(const std::vector<std::uint8_t>& bytes)
        : m_type{ ReceiptType::Unknown }, 
        m_bytes{ bytes }, 
        m_base64{ StringHelpers::toBase64(bytes) }
    {
        if(!bytes.empty())
        {
            if(m_base64.empty())
            {
                throw std::invalid_argument("Invalid bytes vector");
            }
            //Get type
            if(m_bytes[0] == 'J')
            {
                m_type = ReceiptType::JPEG;
            }
            else if(m_bytes[0] == 'P')
            {
                m_type = ReceiptType::PNG;
            }
            else if(m_bytes[0] == 'D')
            {
                m_type = ReceiptType::PDF;
            }
            else if(m_bytes[0] == 'U')
            {
                m_type = ReceiptType::Unknown;
            }
            else
            {
                throw std::invalid_argument("Invalid bytes vector");
            }
            //Remove type from bytes
            m_bytes.erase(m_bytes.begin());
        }
    }

    Receipt::Receipt(const std::string& base64)
        : m_type{ ReceiptType::Unknown }, 
        m_base64{ base64 }
    {
        if(!m_base64.empty())
        {
            //Get bytes
            m_bytes = StringHelpers::toByteList(base64);
            if(m_bytes.empty())
            {
                throw std::invalid_argument("Invalid receipt base64 string");
            }
            //Get type
            if(m_bytes[0] == 'J')
            {
                m_type = ReceiptType::JPEG;
            }
            else if(m_bytes[0] == 'P')
            {
                m_type = ReceiptType::PNG;
            }
            else if(m_bytes[0] == 'D')
            {
                m_type = ReceiptType::PDF;
            }
            else if(m_bytes[0] == 'U')
            {
                m_type = ReceiptType::Unknown;
            }
            else
            {
                throw std::invalid_argument("Invalid receipt base64 string");
            }
            //Remove type from bytes
            m_bytes.erase(m_bytes.begin());
        }
    }

    bool Receipt::empty() const
    {
        return m_bytes.empty();
    }

    const std::string& Receipt::toString() const
    {
        return m_base64;
    }

    bool Receipt::saveToDisk(const std::filesystem::path& pathToFile, bool overwrite) const
    {
        if(std::filesystem::exists(pathToFile) && !overwrite)
        {
            return false;
        }
        std::filesystem::path path{ pathToFile };
        if(m_type == ReceiptType::JPEG && (path.extension() != ".jpeg" || path.extension() != ".jpg"))
        {
            path.replace_extension(".jpg");
        }
        else if(m_type == ReceiptType::PNG && path.extension() != ".png")
        {
            path.replace_extension(".png");
        }
        else if(m_type == ReceiptType::PDF && path.extension() != ".pdf")
        {
            path.replace_extension(".pdf");
        }
        else if(m_type == ReceiptType::Unknown && path.has_extension())
        {
            path.replace_extension();
        }
        std::ofstream out{ path, std::ios_base::binary | std::ios_base::trunc };
        out.write(reinterpret_cast<const char*>(&m_bytes[0]), m_bytes.size());
        return true;
    }

    Receipt::operator bool() const
    {
        return !empty();
    }
}