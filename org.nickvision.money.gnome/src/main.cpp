#include "application.h"

using namespace Nickvision::Money::GNOME;

int main(int argc, char* argv[])
{
    Application app{ argc, argv };
    return app.run();
}