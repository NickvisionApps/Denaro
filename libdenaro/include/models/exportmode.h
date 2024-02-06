#ifndef EXPORTMODE_H
#define EXPORTMODE_H

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief Modes for exporting account information. 
     */
    enum class ExportMode
    {
        All = 0,
        CurrentView
    };
}

#endif //EXPORTMODE_H