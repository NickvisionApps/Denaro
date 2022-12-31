using NickvisionMoney.Shared.Controllers;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Transfer
/// </summary>
public partial class TransferDialog
{
    private delegate void SignalCallback(nint gObject, string response, nint data);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial ulong g_signal_connect_data(nint instance, string detailed_signal, [MarshalAs(UnmanagedType.FunctionPtr)] SignalCallback c_handler, nint data, nint destroy_data, int connect_flags);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_add_response(nint dialog, string id, string label);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint adw_message_dialog_new(nint parent, string heading, string body);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_set_close_response(nint dialog, string response);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_set_default_response(nint dialog, string response);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_set_extra_child(nint dialog, nint child);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void adw_message_dialog_set_response_appearance(nint dialog, string response, int appearance);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool gtk_widget_is_visible(nint widget);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_widget_show(nint widget);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_destroy(nint window);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_set_default_size(nint window, int x, int y);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_set_hide_on_close(nint window, [MarshalAs(UnmanagedType.I1)] bool setting);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_window_set_modal(nint window, [MarshalAs(UnmanagedType.I1)] bool modal);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_file_chooser_get_file(nint chooser);

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial string g_file_get_path(nint file);

    private readonly TransferDialogController _controller;
    private readonly Gtk.Window _parentWindow;
    private readonly nint _dialog;
    private readonly Gtk.Box _boxMain;
    private readonly Gtk.Label _lblDestination;
    private readonly Gtk.Label _lblSelectedAccount;
    private readonly Gtk.Button _btnSelectAccount;
    private readonly Gtk.Box _boxSelectedAccount;
    private readonly Adw.Clamp _clampSelectedAccount;
    private readonly Gtk.Box _boxTransferAccount;
    private readonly Gtk.Label _lblCurrency;
    private readonly Gtk.Entry _txtAmount;
    private readonly Adw.ActionRow _rowAmount;
    private readonly Adw.PreferencesGroup _grpAmount;

    /// <summary>
    /// Constructs a TransferDialog
    /// </summary>
    /// <param name="controller">TransferDialogController</param>
    /// <param name="parentWindow">Gtk.Window</param>
    public TransferDialog(TransferDialogController controller, Gtk.Window parentWindow)
    {
        _controller = controller;
        _parentWindow = parentWindow;
        //Dialog Settings
        _dialog = adw_message_dialog_new(parentWindow.Handle, _controller.Localizer["Transfer"], _controller.Localizer["TransferDescription"]);
        gtk_window_set_default_size(_dialog, 360, -1);
        gtk_window_set_hide_on_close(_dialog, true);
        adw_message_dialog_add_response(_dialog, "cancel", _controller.Localizer["Cancel"]);
        adw_message_dialog_set_close_response(_dialog, "cancel");
        adw_message_dialog_add_response(_dialog, "ok", _controller.Localizer["OK"]);
        adw_message_dialog_set_default_response(_dialog, "ok");
        adw_message_dialog_set_response_appearance(_dialog, "ok", 1); // ADW_RESPONSE_SUGGESTED
        g_signal_connect_data(_dialog, "response", (nint sender, [MarshalAs(UnmanagedType.LPStr)] string response, nint data) => _controller.Accepted = response == "ok", IntPtr.Zero, IntPtr.Zero, 0);
        //Main Box
        _boxMain = Gtk.Box.New(Gtk.Orientation.Vertical, 10);
        //Destination Label
        _lblDestination = Gtk.Label.New(_controller.Localizer["DestinationAccount", "Field"]);
        _lblDestination.AddCssClass("title-4");
        _lblDestination.SetMarginTop(6);
        //Transfer Account Label
        _lblSelectedAccount = Gtk.Label.New(_controller.Localizer["NoAccountSelected"]);
        _lblSelectedAccount.SetValign(Gtk.Align.Center);
        _lblSelectedAccount.SetEllipsize(Pango.EllipsizeMode.Start);
        _lblSelectedAccount.SetMarginStart(20);
        //Select Account Button
        _btnSelectAccount = Gtk.Button.NewFromIconName("document-open-symbolic");
        _btnSelectAccount.AddCssClass("flat");
        _btnSelectAccount.SetValign(Gtk.Align.Center);
        _btnSelectAccount.SetTooltipText(_controller.Localizer["DestinationAccount", "Placeholder"]);
        _btnSelectAccount.OnClicked += OnSelectAccount;
        //Selected Account Box
        _boxSelectedAccount = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
        _boxSelectedAccount.SetHalign(Gtk.Align.Center);
        _boxSelectedAccount.Append(_lblSelectedAccount);
        _boxSelectedAccount.Append(_btnSelectAccount);
        //Selected Account Clamp
        _clampSelectedAccount = Adw.Clamp.New();
        _clampSelectedAccount.SetMaximumSize(280);
        _clampSelectedAccount.SetChild(_boxSelectedAccount);
        //Transfer Account Box
        _boxTransferAccount = Gtk.Box.New(Gtk.Orientation.Vertical, 0);
        _boxTransferAccount.AddCssClass("card");
        _boxTransferAccount.Append(_lblDestination);
        _boxTransferAccount.Append(_clampSelectedAccount);
        _boxMain.Append(_boxTransferAccount);
        //Amount
        _lblCurrency = Gtk.Label.New(NumberFormatInfo.CurrentInfo.CurrencySymbol);
        _txtAmount = Gtk.Entry.New();
        _txtAmount.SetValign(Gtk.Align.Center);
        _txtAmount.SetPlaceholderText(_controller.Localizer["Amount", "Placeholder"]);
        _txtAmount.SetInputPurpose(Gtk.InputPurpose.Number);
        _rowAmount = Adw.ActionRow.New();
        _rowAmount.SetTitle(_controller.Localizer["Amount", "Field"]);
        _rowAmount.AddSuffix(_lblCurrency);
        _rowAmount.AddSuffix(_txtAmount);
        _grpAmount = Adw.PreferencesGroup.New();
        _grpAmount.Add(_rowAmount);
        _boxMain.Append(_grpAmount);
        //Layout
        adw_message_dialog_set_extra_child(_dialog, _boxMain.Handle);
    }

    /// <summary>
    /// Runs the dialog
    /// </summary>
    /// <returns>True if the dialog was accepted, else false</returns>
    public async Task<bool> RunAsync()
    {
        gtk_widget_show(_dialog);
        gtk_window_set_modal(_dialog, true);
        while(gtk_widget_is_visible(_dialog))
        {
            await Task.Delay(100);
        }
        if(_controller.Accepted)
        {
            gtk_window_set_modal(_dialog, false);
            var status = _controller.UpdateTransfer(_lblSelectedAccount.GetText(), _txtAmount.GetText());
            if(status != TransferCheckStatus.Valid)
            {
                _lblDestination.RemoveCssClass("error");
                _lblSelectedAccount.RemoveCssClass("error");
                _btnSelectAccount.RemoveCssClass("error");
                _lblDestination.SetText(_controller.Localizer["DestinationAccount", "Field"]);
                _rowAmount.RemoveCssClass("error");
                _rowAmount.SetTitle(_controller.Localizer["Amount", "Field"]);
                //Mark Error
                if(status == TransferCheckStatus.InvalidDestPath)
                {
                    _lblDestination.AddCssClass("error");
                    _lblSelectedAccount.AddCssClass("error");
                    _btnSelectAccount.AddCssClass("error");
                    _lblDestination.SetText(_controller.Localizer["DestinationAccount", "Invalid"]);
                }
                else if(status == TransferCheckStatus.InvalidAmount)
                {
                    _rowAmount.AddCssClass("error");
                    _rowAmount.SetTitle(_controller.Localizer["Amount", "Invalid"]);
                }
                return await RunAsync();
            }
        }
        gtk_window_destroy(_dialog);
        return _controller.Accepted;
    }

    /// <summary>
    /// Occurs when Select Destination Account button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void OnSelectAccount(Gtk.Button sender, EventArgs e)
    {
        var openFileDialog = Gtk.FileChooserNative.New(_controller.Localizer["SelectAccount"], _parentWindow, Gtk.FileChooserAction.Open, _controller.Localizer["Open"], _controller.Localizer["Cancel"]);
        openFileDialog.SetModal(true);
        var filter = Gtk.FileFilter.New();
        filter.SetName(_controller.Localizer["AccountFileFilter", "GTK"]);
        filter.AddPattern("*.nmoney");
        openFileDialog.AddFilter(filter);
        openFileDialog.OnResponse += (sender, e) =>
        {
            if (e.ResponseId == (int)Gtk.ResponseType.Accept)
            {
                var path = g_file_get_path(gtk_file_chooser_get_file(openFileDialog.Handle));
                _lblSelectedAccount.SetText(path);
            }
        };
        openFileDialog.Show();
    }
}