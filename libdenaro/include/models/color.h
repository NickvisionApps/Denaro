#ifndef COLOR_H
#define COLOR_H

#include <string>

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief A model of a RGBA color value. 
     */
    class Color
    {
    public:
        /**
         * @brief Constructs a Color. 
         */
        Color();
        /**
         * @brief Constructs a Color.
         * @brief The A color value will be set to 255.
         * @param r The R color value
         * @param g The G color value
         * @param a The B color value 
         */
        Color(int r, int g, int b);
        /**
         * @brief Constructs a Color.
         * @param r The R color value
         * @param g The G color value
         * @param a The B color value
         * @param a The A color value
         */
        Color(int r, int g, int b, int a);
        /**
         * @brief Constructs a Color.
         * @brief Strings must be in one of the following formats: "rgb(r,g,b)", "r,g,b", "rgba(r,g,b,a)", "r,g,b,a", "#rrggbb", "#rrggbbaa".
         * @brief If parsing fails, the Color will have an empty value.
         * @param s The string to parse into a Color 
         */
        Color(const std::string& s);
        /**
         * @brief Gets whether or not the Color object represents an empty value.
         * @return True if empty, else false 
         */
        bool empty() const;
        /**
         * @brief Gets the rgb string for this Color object.
         * @param header Whether or not to wrap the string in the header: rgb()
         * @return If header is true, "rgb(r,g,b)" else "r,g,b" 
         */
        std::string toRGBString(bool header) const;
        /**
         * @brief Gets the rgba string for this Color object.
         * @param header Whether or not to wrap the string in the header: rgba()
         * @return If header is true, "rgba(r,g,b,a)" else "r,g,b,a" 
         */
        std::string toRGBAString(bool header) const;
        /**
         * @brief Gets the rgb hex string for this Color object.
         * @return "#rrggbb" 
         */
        std::string toRGBHexString() const;
        /**
         * @brief Gets the rgba hex string for this Color object.
         * @return "#rrggbbaa"
         */
        std::string toRGBAHexString() const;
        /**
         * @brief Gets whether or not this Color is equal to compare Color.
         * @param compare The Color to compare to
         * @return True if this Color == compare Color 
         */
        bool operator==(const Color& compare);
        /**
         * @brief Gets whether or not this Color is not equal to compare Color.
         * @param compare The Color to compare to
         * @return True if this Color != compare Color 
         */
        bool operator!=(const Color& compare);
        /**
         * @brief Gets whether or not the object is valid or not.
         * @return True if valid (!empty), else false 
         */
        operator bool() const;

    private:
        int m_r;
        int m_g;
        int m_b;
        int m_a;
    };
}

#endif //COLOR_H