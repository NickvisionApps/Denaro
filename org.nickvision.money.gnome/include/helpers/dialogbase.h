#ifndef DIALOGBASE_H
#define DIALOGBASE_H

#include <string>
#include <adwaita.h>

namespace Nickvision::Money::GNOME::Helpers
{
    /**
     * @brief A base class for custom AdwDialogs with blueprints.
     */
    class DialogBase
    {
    public:
        /**
         * @brief Constructs a DialogBase.
         * @param parent GtkWindow*
         * @param fileName The file name for the blueprint file of the dialog
         * @param rootName The name of the AdwDialog component in the blueprint file
         */
        DialogBase(GtkWindow* parent, const std::string& fileName, const std::string& rootName = "root");
        /**
         * @brief Destructs a DialogBase.
         */
        ~DialogBase();
        /**
         * @brief Gets the underlying AdwDialog pointer.
         * @return AdwDialog*
         */
        AdwDialog* get();
        /**
         * @brief Presents the AdwDialog.
         */
        void present() const;

    protected:
        GtkBuilder* m_builder;
        GtkWindow* m_parent;
        AdwDialog* m_dialog;
    };
}

#endif //DIALOGBASE_H
