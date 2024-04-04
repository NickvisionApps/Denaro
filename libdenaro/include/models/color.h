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
        Color(unsigned char r, unsigned char g, unsigned char b);
        /**
         * @brief Constructs a Color.
         * @param r The R color value
         * @param g The G color value
         * @param a The B color value
         * @param a The A color value
         */
        Color(unsigned char r, unsigned char g, unsigned char b, unsigned char a);
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
         * @brief Gets the R color value.
         * @return The R color value 
         */
        unsigned char getR() const;
        /**
         * @brief Sets the R color value.
         * @param r The new R color value 
         */
        void setR(unsigned char r);
        /**
         * @brief Gets the G color value.
         * @return The G color value 
         */
        unsigned char getG() const;
        /**
         * @brief Sets the G color value.
         * @param g The new G color value 
         */
        void setG(unsigned char g);
        /**
         * @brief Gets the B color value.
         * @return The B color value 
         */
        unsigned char getB() const;
        /**
         * @brief Sets the B color value.
         * @param b The new B color value 
         */
        void setB(unsigned char b);
        /**
         * @brief Gets the A color value.
         * @return The A color value 
         */
        unsigned char getA() const;
        /**
         * @brief Sets the A color value.
         * @param a The new A color value 
         */
        void setA(unsigned char a);
        /**
         * @brief Gets the hex value for this Color object.
         * @param alpha Whether or not to include the alpha value in the hex value
         * @return The hex value 
         */
        unsigned int toHex(bool alpha = true) const;
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
        unsigned char m_r;
        unsigned char m_g;
        unsigned char m_b;
        unsigned char m_a;
    };
}

#endif //COLOR_H