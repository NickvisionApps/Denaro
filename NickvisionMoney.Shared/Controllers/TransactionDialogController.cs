using Docnet.Core;
using Docnet.Core.Converters;
using Docnet.Core.Models;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static NickvisionMoney.Shared.Helpers.Gettext;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// Statuses for when a transaction is validated
/// </summary>
[Flags]
public enum TransactionCheckStatus
{
    Valid = 1,
    EmptyDescription = 2,
    InvalidAmount = 4,
    InvalidRepeatEndDate = 8,
    CannotAccessReceipt = 16
}

/// <summary>
/// A controller for a TransactionDialog
/// </summary>
public class TransactionDialogController : IDisposable
{
    private bool _disposed;
    private readonly string _transactionDefaultColor;
    private readonly Dictionary<uint, Transaction> _transactions;
    private readonly Dictionary<uint, Group> _groups;

    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => AppInfo.Current;
    /// <summary>
    /// The transaction represented by the controller
    /// </summary>
    public Transaction Transaction { get; init; }
    /// <summary>
    /// Whether or not this transaction can be copied
    /// </summary>
    public bool CanCopy { get; init; }
    /// <summary>
    /// Whether or not the dialog is editing a transaction
    /// </summary>
    public bool IsEditing { get; init; }
    /// <summary>
    /// Whether or not there was a request to make a copy of transaction
    /// </summary>
    public bool CopyRequested { get; set; }
    /// <summary>
    /// The original repeat interval of a transaction
    /// </summary>
    public TransactionRepeatInterval OriginalRepeatInterval { get; init; }
    /// <summary>
    /// The CultureInfo to use when displaying a number string
    /// </summary>
    public CultureInfo CultureForNumberString { get; init; }
    /// <summary>
    /// The CultureInfo to use when displaying a date string
    /// </summary>
    public CultureInfo CultureForDateString { get; init; }
    
    /// <summary>
    /// Whether to use native digits
    /// </summary>
    public bool UseNativeDigits => Models.Configuration.Current.UseNativeDigits; // Full name is required to avoid error because of ambiguous reference (there's also SixLabors.ImageSharp.Configuration)
    /// <summary>
    /// Decimal Separator Inserting
    /// <summary>
    public InsertSeparator InsertSeparator => Models.Configuration.Current.InsertSeparator; // Full name is required to avoid error because of ambiguous reference (there's also SixLabors.ImageSharp.Configuration)
    /// <summary>
    /// The list of group names
    /// </summary>
    public List<string> GroupNames => _groups.Values.OrderBy(x => x.Name == _("Ungrouped") ? " " : x.Name).Select(x => x.Name).ToList();

    /// <summary>
    /// Constructs a TransactionDialogController
    /// </summary>
    /// <param name="transaction">The Transaction object represented by the controller</param>
    /// <param name="groups">The list of groups in the account</param>
    /// <param name="transactions">The list of transactions in the account</param>
    /// <param name="canCopy">Whether or not the transaction can be copied</param>
    /// <param name="transactionDefaultColor">A default color for the transaction</param>
    /// <param name="cultureNumber">The CultureInfo to use for the amount string</param>
    /// <param name="cultureDate">The CultureInfo to use for the date string</param>
    internal TransactionDialogController(Transaction transaction, Dictionary<uint, Transaction> transactions, Dictionary<uint, Group> groups, bool canCopy, string transactionDefaultColor, CultureInfo cultureNumber, CultureInfo cultureDate)
    {
        _disposed = false;
        _transactionDefaultColor = transactionDefaultColor;
        _transactions = transactions;
        _groups = groups;
        Transaction = (Transaction)transaction.Clone();
        CanCopy = canCopy;
        IsEditing = canCopy;
        CopyRequested = false;
        OriginalRepeatInterval = Transaction.RepeatInterval;
        CultureForNumberString = cultureNumber;
        CultureForDateString = cultureDate;
        if (string.IsNullOrEmpty(Transaction.RGBA))
        {
            Transaction.RGBA = _transactionDefaultColor;
        }
    }

    /// <summary>
    /// Constructs a TransactionDialogController
    /// </summary>
    /// <param name="id">The id of the new transaction</param>
    /// <param name="groups">The list of groups in the account</param>
    /// <param name="transactions">The list of transactions in the account</param>
    /// <param name="transactionDefaultType">A default type for the transaction</param>
    /// <param name="transactionDefaultColor">A default color for the transaction</param>
    /// <param name="cultureNumber">The CultureInfo to use for the amount string</param>
    /// <param name="cultureDate">The CultureInfo to use for the date string</param>
    internal TransactionDialogController(uint id, Dictionary<uint, Transaction> transactions, Dictionary<uint, Group> groups, TransactionType transactionDefaultType, string transactionDefaultColor, CultureInfo cultureNumber, CultureInfo cultureDate)
    {
        _disposed = false;
        _transactionDefaultColor = transactionDefaultColor;
        _transactions = transactions;
        _groups = groups;
        Transaction = new Transaction(id);
        CanCopy = false;
        IsEditing = false;
        CopyRequested = false;
        OriginalRepeatInterval = Transaction.RepeatInterval;
        CultureForNumberString = cultureNumber;
        CultureForDateString = cultureDate;
        //Set Defaults For New Transaction
        Transaction.Type = transactionDefaultType;
        Transaction.RGBA = transactionDefaultColor;
    }
    
    /// <summary>
    /// Finalizes the TransactionDialogController
    /// </summary>
    ~TransactionDialogController() => Dispose(false);
    
    /// <summary>
    /// The repeat interval index used by GUI
    /// </summary>
    public uint RepeatIntervalIndex => (uint)Transaction.RepeatInterval switch
    {
        0 => (uint)Transaction.RepeatInterval,
        1 => (uint)Transaction.RepeatInterval,
        2 => (uint)Transaction.RepeatInterval,
        7 => 3,
        _ => (uint)Transaction.RepeatInterval + 1
    };

    /// <summary>
    /// Frees resources used by the TransactionDialogController object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the TransactionDialogController object
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            Transaction.Dispose();
        }
        var jpgPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}{Path.DirectorySeparatorChar}Denaro_ViewReceipt_TEMP.jpg";
        if (File.Exists(jpgPath))
        {
            File.Delete(jpgPath);
        }
        _disposed = true;
    }

    /// <summary>
    /// Gets a list of suggestions to finish a description
    /// </summary>
    /// <param name="description">The description to get suggestions for</param>
    /// <returns>The list of suggestions and their subtext and transactions</returns>
    public List<(string, string, Transaction)> GetDescriptionSuggestions(string description)
    {
        return _transactions
            .Where(x => FuzzySharp.Fuzz.PartialRatio(x.Value.Description, description) > 60)
            .GroupBy(x => x.Value.Description)
            .Select(x =>
            {
                var first = x.FirstOrDefault(y => y.Value.GroupId != -1, x.First()).Value;
                return (first.Description, first.GroupId != -1 ? $"{_("Group")}: {_groups[(uint)first.GroupId].Name}" : "", first);
            })
            .OrderByDescending(x => FuzzySharp.Fuzz.PartialRatio(x.Item1, description))
            .Take(5).ToList();
    }

    /// <summary>
    /// Gets the name of a group from a group id
    /// </summary>
    /// <param name="id">The id of the group</param>
    /// <returns>The name of the group</returns>
    public string GetGroupNameFromId(uint id)
    {
        try
        {
            return _groups[id].Name;
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Opens the receipt image of the transaction in the default viewer application
    /// </summary>
    /// <param name="receiptPath">A possible path of a new receipt image</param>
    public async Task OpenReceiptImageAsync(string? receiptPath = null)
    {
        Image? image = null;
        if (receiptPath != null)
        {
            if (File.Exists(receiptPath))
            {
                if (Path.GetExtension(receiptPath).ToLower() == ".jpeg" || Path.GetExtension(receiptPath).ToLower() == ".jpg" || Path.GetExtension(receiptPath).ToLower() == ".png")
                {
                    image = await Image.LoadAsync(receiptPath);
                }
                else if (Path.GetExtension(receiptPath).ToLower() == ".pdf")
                {
                    image = ConvertPDFToJPEG(receiptPath);
                }
            }
        }
        else
        {
            image = Transaction.Receipt;
        }
        if (image != null)
        {
            var jpgPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}{Path.DirectorySeparatorChar}Denaro_ViewReceipt_TEMP.jpg";
            await image.SaveAsJpegAsync(jpgPath);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("explorer", $"\"{jpgPath}\"") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start(new ProcessStartInfo("xdg-open", jpgPath));
            }
        }
        if (receiptPath != null)
        {
            image?.Dispose();
        }
    }

    /// <summary>
    /// Updates the Transaction object
    /// </summary>
    /// <param name="date">The new DateOnly object</param>
    /// <param name="description">The new description</param>
    /// <param name="type">The new TransactionType</param>
    /// <param name="selectedRepeat">The new selected repeat index</param>
    /// <param name="groupName">The new Group name</param>
    /// <param name="rgba">The new rgba string</param>
    /// <param name="useGroupColor">Whether or not to use the group's color instead of the transaction's color</param>
    /// <param name="amountString">The new amount string</param>
    /// <param name="receiptPath">The new receipt image path</param>
    /// <param name="repeatEndDate">The new repeat end date DateOnly object</param>
    /// <param name="notes">The new notes</param>
    /// <returns>TransactionCheckStatus</returns>
    public TransactionCheckStatus UpdateTransaction(DateOnly date, string description, TransactionType type, int selectedRepeat, string groupName, string rgba, bool useGroupColor, string amountString, string? receiptPath, DateOnly? repeatEndDate, string notes)
    {
        TransactionCheckStatus result = 0;
        var amount = 0m;
        if (string.IsNullOrEmpty(description))
        {
            result |= TransactionCheckStatus.EmptyDescription;
        }
        try
        {
            amount = decimal.Parse(amountString.ReplaceNativeDigits(CultureForNumberString), NumberStyles.Currency, CultureForNumberString.NumberFormat);
        }
        catch
        {
            result |= TransactionCheckStatus.InvalidAmount;
        }
        if (amount <= 0)
        {
            result |= TransactionCheckStatus.InvalidAmount;
        }
        if (repeatEndDate.HasValue && repeatEndDate.Value <= date)
        {
            result |= TransactionCheckStatus.InvalidRepeatEndDate;
        }
        if (result != 0)
        {
            return result;
        }
        Transaction.Date = date;
        Transaction.Description = description;
        Transaction.Type = type;
        if (selectedRepeat == 3)
        {
            selectedRepeat = 7;
        }
        else if (selectedRepeat > 3)
        {
            selectedRepeat -= 1;
        }
        Transaction.RepeatInterval = (TransactionRepeatInterval)selectedRepeat;
        Transaction.Amount = amount;
        Transaction.GroupId = groupName == _("Ungrouped") ? -1 : (int)_groups.FirstOrDefault(x => x.Value.Name == groupName).Key;
        Transaction.RGBA = rgba;
        Transaction.UseGroupColor = useGroupColor;
        Transaction.Receipt = null;
        if (receiptPath != null)
        {
            try
            {
                if (Path.Exists(receiptPath))
                {
                    if (Path.GetExtension(receiptPath).ToLower() == ".jpeg" || Path.GetExtension(receiptPath).ToLower() == ".jpg" || Path.GetExtension(receiptPath).ToLower() == ".png")
                    {
                        Transaction.Receipt = Image.Load(receiptPath);
                    }
                    else if (Path.GetExtension(receiptPath).ToLower() == ".pdf")
                    {
                        Transaction.Receipt = ConvertPDFToJPEG(receiptPath);
                    }
                }
            }
            catch
            {
                return TransactionCheckStatus.CannotAccessReceipt;
            }
        }
        if (Transaction.RepeatInterval == TransactionRepeatInterval.Never)
        {
            Transaction.RepeatFrom = -1;
        }
        else if (Transaction.RepeatInterval != OriginalRepeatInterval)
        {
            Transaction.RepeatFrom = 0;
        }
        Transaction.RepeatEndDate = Transaction.RepeatInterval == TransactionRepeatInterval.Never ? null : repeatEndDate;
        Transaction.Notes = notes;
        return TransactionCheckStatus.Valid;
    }

    /// <summary>
    /// Converts a PDF to a JPEG Image
    /// </summary>
    /// <param name="pathToPDF">The path to the pdf file</param>
    /// <returns>The JPEG Image</returns>
    private Image ConvertPDFToJPEG(string pathToPDF)
    {
        using var library = DocLib.Instance;
        using var docReader = library.GetDocReader(pathToPDF, new PageDimensions(1080, 1920));
        using var pageReader = docReader.GetPageReader(0);
        return Image.LoadPixelData<Bgra32>(pageReader.GetImage(new NaiveTransparencyRemover(255, 255, 255)), pageReader.GetPageWidth(), pageReader.GetPageHeight());
    }
}
