#!/bin/bash
# This script should be called for the root of the repo

PREFIX="/usr"

if [ ! -z $1 ]
then
	PREFIX=$1
fi
echo Install prefix: $PREFIX

echo Installing icons...
mkdir -p $PREFIX/share/icons/hicolor/scalable/apps
cp NickvisionMoney.Shared/Resources/org.nickvision.money.svg $PREFIX/share/icons/hicolor/scalable/apps/
cp NickvisionMoney.Shared/Resources/org.nickvision.money-devel.svg $PREFIX/share/icons/hicolor/scalable/apps/
mkdir -p $PREFIX/share/icons/hicolor/symbolic/apps
cp NickvisionMoney.Shared/Resources/bank-symbolic.svg $PREFIX/share/icons/hicolor/symbolic/apps/
cp NickvisionMoney.Shared/Resources/larger-brush-symbolic.svg $PREFIX/share/icons/hicolor/symbolic/apps/
cp NickvisionMoney.Shared/Resources/money-none-symbolic.svg $PREFIX/share/icons/hicolor/symbolic/apps/
cp NickvisionMoney.Shared/Resources/org.nickvision.money-symbolic.svg $PREFIX/share/icons/hicolor/symbolic/apps/
cp NickvisionMoney.Shared/Resources/wallet2-symbolic.svg $PREFIX/share/icons/hicolor/symbolic/apps/

echo Installing GResource...
mkdir -p $PREFIX/share/org.nickvision.money
glib-compile-resources NickvisionMoney.GNOME/Resources/org.nickvision.money.gresource.xml
mv NickvisionMoney.GNOME/Resources/org.nickvision.money.gresource $PREFIX/share/org.nickvision.money/

echo Installing desktop file...
mkdir -p $PREFIX/share/applications
cp NickvisionMoney.GNOME/org.nickvision.money.desktop $PREFIX/share/applications/

echo Installing metainfo...
mkdir -p $PREFIX/share/metainfo
cp NickvisionMoney.GNOME/org.nickvision.money.metainfo.xml $PREFIX/share/metainfo/

echo Done!