using NickvisionMoney.Shared.Controllers;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog for managing a Transfer
/// </summary>
public partial class TransferDialog
{
    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint g_main_context_default();

    [LibraryImport("adwaita-1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_main_context_iteration(nint context, [MarshalAs(UnmanagedType.I1)] bool blocking);

    private readonly TransferDialogController _controller;
    private readonly Gtk.Window _parentWindow;
    private readonly Adw.MessageDialog _dialog;
    private readonly Gtk.Box _boxMain;
    private readonly Gtk.Label _lblDestination;
    private readonly Gtk.Label _lblSelectedAccount;
    private readonly Gtk.Button _btnSelectAccount;
    private readonly Gtk.Box _boxSelectedAccount;
    private readonly Adw.Clamp _clampSelectedAccount;
    private readonly Gtk.Box _boxTransferAccount;
    private readonly Gtk.Label _lblCurrency;
    private readonly Adw.EntryRow _rowAmount;
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
        _dialog = Adw.MessageDialog.New(parentWindow, _controller.Localizer["Transfer"], _controller.Localizer["TransferDescription"]);
        _dialog.SetDefaultSize(360, -1);
        _dialog.SetHideOnClose(true);
        _dialog.AddResponse("cancel", _controller.Localizer["Cancel"]);
        _dialog.SetCloseResponse("cancel");
        _dialog.AddResponse("ok", _controller.Localizer["OK"]);
        _dialog.SetDefaultResponse("ok");
        _dialog.SetResponseAppearance("ok", Adw.ResponseAppearance.Suggested);
        _dialog.OnResponse += (sender, e) => _controller.Accepted = e.Response == "ok";
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
        _lblCurrency = Gtk.Label.New($"{NumberFormatInfo.CurrentInfo.CurrencySymbol} ({RegionInfo.CurrentRegion.ISOCurrencySymbol})");
        _lblCurrency.AddCssClass("dim-label");
        _rowAmount = Adw.EntryRow.New();
        _rowAmount.SetTitle(_controller.Localizer["Amount", "Field"]);
        _rowAmount.SetInputPurpose(Gtk.InputPurpose.Number);
        _rowAmount.AddSuffix(_lblCurrency);
        _grpAmount = Adw.PreferencesGroup.New();
        _grpAmount.Add(_rowAmount);
        _boxMain.Append(_grpAmount);
        //Layout
        _dialog.SetExtraChild(_boxMain);
    }

    /// <summary>
    /// Runs the dialog
    /// </summary>
    /// <returns>True if the dialog was accepted, else false</returns>
    public bool Run()
    {
        _dialog.Show();
        _dialog.SetModal(true);
        while (_dialog.IsVisible())
        {
            g_main_context_iteration(g_main_context_default(), false);
        }
        if(_controller.Accepted)
        {
            _dialog.SetModal(false);
            var status = _controller.UpdateTransfer(_lblSelectedAccount.GetText(), _rowAmount.GetText());
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
                return Run();
            }
        }
        _dialog.Destroy();
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
                var path = openFileDialog.GetFile()!.GetPath();
                _lblSelectedAccount.SetText(path ?? "");
            }
        };
        openFileDialog.Show();
    }
}