using NickvisionMoney.Shared.Models;
using NickvisionMoney.Shared.Helpers;
using System;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// The ShortcutsDialog for the application
/// </summary>
public class ShortcutsDialog
{
    private readonly Gtk.Builder _builder;
    private readonly Gtk.ShortcutsWindow _window;

    public ShortcutsDialog(Localizer localizer, string appName, Gtk.Window parent)
    {
    	string xml = $@"<?xml version='1.0' encoding='UTF-8'?>
    <interface>
        <object class='GtkShortcutsWindow' id='dialog'>
            <property name='default-width'>600</property>
            <property name='default-height'>500</property>
            <property name='modal'>true</property>
            <property name='resizable'>true</property>
            <property name='destroy-with-parent'>false</property>
            <property name='hide-on-close'>true</property>
            <child>
                <object class='GtkShortcutsSection'>
                    <child>
                        <object class='GtkShortcutsGroup'>
                            <property name='title'>{ localizer["Account"] }</property>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["NewAccount"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;N</property>
                                </object>
                            </child>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["OpenAccount"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;O</property>
                                </object>
                            </child>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["CloseAccount.GTK"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;W</property>
                                </object>
                            </child>
                        </object>
                    </child>
                    <child>
                        <object class='GtkShortcutsGroup'>
                            <property name='title'>{ localizer["AccountActions.GTK"] }</property>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["Transfer"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;T</property>
                                </object>
                            </child>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["ImportFromFile"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;I</property>
                                </object>
                            </child>
                        </object>
                    </child>
                    <child>
                        <object class='GtkShortcutsGroup'>
                            <property name='title'>{ localizer["Group"] }</property>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["NewGroup"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;G</property>
                                </object>
                            </child>
                        </object>
                    </child>
                    <child>
                        <object class='GtkShortcutsGroup'>
                            <property name='title'>{ localizer["Transaction"] }</property>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["NewTransaction"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;&lt;Shift&gt;N</property>
                                </object>
                            </child>
                        </object>
                    </child>
                    <child>
                        <object class='GtkShortcutsGroup'>
                            <property name='title'>{ localizer["Application.Shortcut"] }</property>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["Preferences"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;comma</property>
                                </object>
                            </child>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["KeyboardShortcuts"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;question</property>
                                </object>
                            </child>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ string.Format(localizer["About"], appName) }</property>
                                    <property name='accelerator'>F1</property>
                                </object>
                            </child>
                        </object>
                    </child>
                </object>
            </child>
        </object>
    </interface>";

        _builder = Gtk.Builder.NewFromString(xml, -1);
        _window = (Gtk.ShortcutsWindow)_builder.GetObject("dialog")!;
        _window.SetTransientFor(parent);
    }

    public void Show() => _window.Show();
}