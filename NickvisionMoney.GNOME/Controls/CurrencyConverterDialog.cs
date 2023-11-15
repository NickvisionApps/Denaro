using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionMoney.GNOME.Controls;

public class CurrencyConverterDialog : Adw.Window
{
    private readonly string _iconName;
    private readonly bool _useNativeDigits;
    private string[]? _currencies;

    [Gtk.Connect] private readonly Adw.ToastOverlay _toastOverlay;
    [Gtk.Connect] private readonly Gtk.Button _switchButton;
    [Gtk.Connect] private readonly Gtk.Box _loadingBox;
    [Gtk.Connect] private readonly Adw.ComboRow _sourceCurrencyRow;
    [Gtk.Connect] private readonly Adw.ComboRow _resultCurrencyRow;
    [Gtk.Connect] private readonly Adw.EntryRow _sourceAmountRow;
    [Gtk.Connect] private readonly Adw.EntryRow _resultAmountRow;
    [Gtk.Connect] private readonly Gtk.Button _copyResultButton;

    /// <summary>
    /// Constructs a CurrencyConverterDialog
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">The name of the icon for the dialog</param>
    /// <param name="useNativeDigits">Whether or not to use native digits when displaying the result amount</param>
    private CurrencyConverterDialog(Gtk.Builder builder, Gtk.Window parent, string iconName, bool useNativeDigits) : base(builder.GetPointer("_root"), false)
    {
        _iconName = iconName;
        _useNativeDigits = useNativeDigits;
        builder.Connect(this);
        //Dialog Settings
        SetTransientFor(parent);
        SetIconName(_iconName);
        _switchButton.OnClicked += (sender, e) =>
        {
            var temp = _sourceCurrencyRow.GetSelected();
            _sourceCurrencyRow.SetSelected(_resultCurrencyRow.GetSelected());
            _resultCurrencyRow.SetSelected(temp);
        };
        _sourceCurrencyRow.OnNotify += async (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                _sourceAmountRow.SetTitle(_currencies[_sourceCurrencyRow.GetSelected()]);
                await OnAmountRowChangedAsync();
            }
        };
        _sourceAmountRow.OnNotify += async (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                await OnAmountRowChangedAsync();
            }
        };
        _resultCurrencyRow.OnNotify += async (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                _resultAmountRow.SetTitle(_currencies[_resultCurrencyRow.GetSelected()]);
                await OnAmountRowChangedAsync();
            }
        };
        _copyResultButton.OnClicked += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(_resultAmountRow.GetText()))
            {
                _resultAmountRow.GetClipboard().SetText(_resultAmountRow.GetText());
                _toastOverlay.AddToast(Adw.Toast.New(_("Result was copied to clipboard.")));
            }
        };
    }

    /// <summary>
    /// Constructs a CurrencyConverterDialog
    /// </summary>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">The name of the icon for the dialog</param>
    /// <param name="useNativeDigits">Whether or not to use native digits when displaying the result amount</param>
    public CurrencyConverterDialog(Gtk.Window parent, string iconName, bool useNativeDigits) : this(Builder.FromFile("currency_converter_dialog.ui"), parent, iconName, useNativeDigits)
    {
    }

    /// <summary>
    /// Presents the dialog
    /// </summary>
    public async Task PresentAsync()
    {
        base.Present();
        _loadingBox.SetVisible(true);
        _currencies = (await CurrencyConversionService.GetConversionRatesAsync("USD") ?? new Dictionary<string, decimal>()).Keys
            .OrderByDescending(x => x == "USD")
            .ThenByDescending(x => x == "EUR")
            .ThenByDescending(x => x == "JPY")
            .ThenByDescending(x => x == "GBP")
            .ThenByDescending(x => x == "CNY")
            .ThenByDescending(x => x == "CAD")
            .ThenByDescending(x => x == "AUD")
            .ToArray();
        if (_currencies.Length == 0)
        {
            var messageDialog = Adw.MessageDialog.New(this, _("Error"), _("Unable to load currency data. Please try again. If the error still persists, report a bug."));
            messageDialog.SetIconName(_iconName);
            messageDialog.AddResponse("close", _("Close"));
            messageDialog.SetDefaultResponse("close");
            messageDialog.SetCloseResponse("close");
            messageDialog.OnResponse += async (ss, exx) =>
            {
                messageDialog.Destroy();
                Close();
            };
            messageDialog.Present();
        }
        else
        {
            var currenciesList = Gtk.StringList.New(_currencies);
            _loadingBox.SetVisible(false);
            _sourceCurrencyRow.SetModel(currenciesList);
            _resultCurrencyRow.SetModel(currenciesList);
            _resultCurrencyRow.SetSelected(1);
            _sourceAmountRow.SetText("1");
            _sourceAmountRow.SetPosition(-1);
        }
    }

    /// <summary>
    /// Occurs when the source amount row changes
    /// </summary>
    private async Task OnAmountRowChangedAsync()
    {
        _sourceAmountRow.RemoveCssClass("error");
        if (string.IsNullOrEmpty(_sourceAmountRow.GetText()))
        {
            _resultAmountRow.SetText("");
        }
        else
        {
            _loadingBox.SetVisible(true);
            if (decimal.TryParse(_sourceAmountRow.GetText(), out var amount))
            {
                var res = await CurrencyConversionService.ConvertAsync(_currencies![_sourceCurrencyRow.GetSelected()], amount, _currencies[_resultCurrencyRow.GetSelected()]);
                if (res != null)
                {
                    _resultAmountRow.SetText(res.ResultAmount.ToAmountString(CultureInfo.CurrentCulture, _useNativeDigits, false, true));
                }
            }
            else
            {
                _sourceAmountRow.AddCssClass("error");
                _resultAmountRow.SetText("");
            }
            _loadingBox.SetVisible(false);
        }
    }
}