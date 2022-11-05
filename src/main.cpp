#include <libintl.h>
#include "ui/application.hpp"

using namespace NickvisionMoney::UI;

/**
 * The main functions
 *
 * @param The number of arguments
 * @param The array of arguments
 *
 * @returns The application exit code
 */
int main(int argc, char* argv[])
{
    setlocale(LC_ALL, "");
    bindtextdomain(GETTEXT_PACKAGE, LOCALE_DIR);
    bind_textdomain_codeset(GETTEXT_PACKAGE, "UTF-8");
    textdomain(GETTEXT_PACKAGE);
    
    Application app("org.nickvision.money");
    return app.run(argc, argv);
}
