#!/bin/sh

INSTALL_PREFIX="/usr"
if [ -n "${1}" ]
then
    INSTALL_PREFIX="${1}"
fi
echo Install prefix: "${INSTALL_PREFIX}"

if [ "$(basename "$(pwd)")" = "NickvisionMoney.GNOME" ]
then
    cd ..
fi

echo "Installing Occitan translation... (dotnet bug workaround)"
mkdir -p "${INSTALL_PREFIX}"/opt/org.nickvision.money/oc
cp ./NickvisionMoney.GNOME/oc-workaround/NickvisionMoney.Shared.resources.dll \
    "${INSTALL_PREFIX}"/opt/org.nickvision.money/oc/

echo "Installing icons..."
mkdir -p "${INSTALL_PREFIX}"/share/icons/hicolor/scalable/apps
for icon in org.nickvision.money.svg org.nickvision.money-devel.svg
do
    cp ./NickvisionMoney.Shared/Resources/${icon}               \
       "${INSTALL_PREFIX}"/share/icons/hicolor/scalable/apps/
done
mkdir -p "${INSTALL_PREFIX}"/share/icons/hicolor/symbolic/apps
for icon in bank-symbolic.svg                       \
                larger-brush-symbolic.svg           \
                money-none-symbolic.svg             \
                moon-symbolic.svg                   \
                org.nickvision.money-symbolic.svg   \
                sun-alt-symbolic.svg                \
                wallet2-symbolic.svg
do
    cp ./NickvisionMoney.Shared/Resources/${icon}               \
       "${INSTALL_PREFIX}"/share/icons/hicolor/symbolic/apps/
done

echo "Installing GResource..."
mkdir -p "${INSTALL_PREFIX}"/share/org.nickvision.money
glib-compile-resources ./NickvisionMoney.GNOME/Resources/org.nickvision.money.gresource.xml
mv ./NickvisionMoney.GNOME/Resources/org.nickvision.money.gresource \
   "${INSTALL_PREFIX}"/share/org.nickvision.money/

echo "Installing desktop file..."
mkdir -p "${INSTALL_PREFIX}"/share/applications
cp ./NickvisionMoney.GNOME/org.nickvision.money.desktop \
   "${INSTALL_PREFIX}"/share/applications/

echo "Installing metainfo..."
mkdir -p "${INSTALL_PREFIX}"/share/metainfo
cp ./NickvisionMoney.GNOME/org.nickvision.money.metainfo.xml    \
   "${INSTALL_PREFIX}"/share/metainfo/

echo "Translating desktop file and metainfo..."
python3 ./NickvisionMoney.GNOME/translate_meta.py ${INSTALL_PREFIX}

echo "Installing mime types..."
mkdir -p "${INSTALL_PREFIX}"/share/mime/packages
cp ./NickvisionMoney.GNOME/org.nickvision.money.extension.xml   \
   "${INSTALL_PREFIX}"/share/mime/packages/
update-mime-database "${INSTALL_PREFIX}"/share/mime/

echo "Installing user docs..."
cd NickvisionMoney.Shared/Docs/yelp
for lang in *
do
	mkdir -p $INSTALL_PREFIX/share/help/$lang/denaro/
	cp -r $lang/* $INSTALL_PREFIX/share/help/$lang/denaro/
done

echo "Done!"
