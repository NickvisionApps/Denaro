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

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// Statuses for when a transaction is validated
/// </summary>
public enum TransactionCheckStatus
{
    Valid = 0,
    EmptyDescription,
    InvalidAmount,
    InvalidRepeatEndDate
}

/// <summary>
/// A controller for a TransactionDialog
/// </summary>
public class TransactionDialogController : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }
    /// <summary>
    /// The transaction represented by the controller
    /// </summary>
    public Transaction Transaction { get; init; }
    /// <summary>
    /// The groups in the account
    /// </summary>
    public Dictionary<uint, string> Groups { get; init; }
    /// <summary>
    /// Whether or not the dialog was accepted (response)
    /// </summary>
    public bool Accepted { get; set; }
    /// <summary>
    /// The original repeat interval of a transaction
    /// </summary>
    public TransactionRepeatInterval OriginalRepeatInterval { get; private set; }
    /// <summary>
    /// The CultureInfo to use when displaying a number string
    /// </summary>
    public CultureInfo CultureForNumberString { get; init; }

    /// <summary>
    /// The repeat interval index used by GUI
    /// </summary>
    public uint RepeatIntervalIndex => (uint)Transaction.RepeatInterval switch
    {
        0 => (uint) Transaction.RepeatInterval,
        1 => (uint)Transaction.RepeatInterval,
        2 => (uint)Transaction.RepeatInterval,
        7 => 3,
        _ => (uint)Transaction.RepeatInterval + 1
    };

    /// <summary>
    /// Constructs a TransactionDialogController
    /// </summary>
    /// <param name="transaction">The Transaction object represented by the controller</param>
    /// <param name="groups">The list of groups in the account</param>
    /// <param name="transactionDefaultType">A default type for the transaction</param>
    /// <param name="transactionDefaultColor">A default color for the transaction</param>
    /// <param name="culture">The CultureInfo to use for the amount string</param>
    /// <param name="localizer">The Localizer of the app</param>
    internal TransactionDialogController(Transaction transaction, Dictionary<uint, string> groups, TransactionType transactionDefaultType, string transactionDefaultColor, CultureInfo culture, Localizer localizer)
    {
        _disposed = false;
        Localizer = localizer;
        Transaction = (Transaction)transaction.Clone();
        Groups = groups;
        Accepted = false;
        OriginalRepeatInterval = transaction.RepeatInterval;
        CultureForNumberString = culture;
        if (Transaction.Amount == 0m) //new transaction
        {
            Transaction.Type = transactionDefaultType;
            Transaction.RGBA = transactionDefaultColor;
        }
    }

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
            var jpgPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}{Path.DirectorySeparatorChar}Denaro_ViewReceipt_TEMP.jpg";
            if (File.Exists(jpgPath))
            {
                File.Delete(jpgPath);
            }
        }
        _disposed = true;
    }

    /// <summary>
    /// Opens the receipt image of the transaction in the default viewer application
    /// </summary>
    /// <param name="receiptPath">A possible path of a new receipt image</param>
    public async Task OpenReceiptImageAsync(string? receiptPath = null)
    {
        Image? image;
        if (receiptPath != null)
        {
            if (File.Exists(receiptPath))
            {
                if (Path.GetExtension(receiptPath) == ".jpeg" || Path.GetExtension(receiptPath) == ".jpg" || Path.GetExtension(receiptPath) == ".png")
                {
                    image = await Image.LoadAsync(receiptPath);
                }
                else if (Path.GetExtension(receiptPath) == ".pdf")
                {
                    image = ConvertPDFToJPEG(receiptPath);
                }
                else
                {
                    image = null;
                }
            }
            else
            {
                image = null;
            }
        }
        else
        {
            image = Transaction.Receipt;
        }
        if(image != null)
        {
            var jpgPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}{Path.DirectorySeparatorChar}Denaro_ViewReceipt_TEMP.jpg";
            await image.SaveAsJpegAsync(jpgPath);
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("explorer", $"\"{jpgPath}\"") { CreateNoWindow = true });
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start(new ProcessStartInfo("xdg-open", jpgPath));
            }
        }
        if(receiptPath != null)
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
    /// <param name="amountString">The new amount string</param>
    /// <param name="receiptPath">The new receipt image path</param>
    /// <param name="repeatEndDate">The new repeat end date DateOnly object</param>
    /// <returns>TransactionCheckStatus</returns>
    public TransactionCheckStatus UpdateTransaction(DateOnly date, string description, TransactionType type, int selectedRepeat, string groupName, string rgba, string amountString, string? receiptPath, DateOnly? repeatEndDate)
    {
        var amount = 0m;
        if(string.IsNullOrEmpty(description))
        {
            return TransactionCheckStatus.EmptyDescription;
        }
        try
        {
            amount = decimal.Parse(amountString, NumberStyles.Currency, CultureForNumberString);
        }
        catch
        {
            return TransactionCheckStatus.InvalidAmount;
        }
        if (amount <= 0)
        {
            return TransactionCheckStatus.InvalidAmount;
        }
        if(repeatEndDate.HasValue && repeatEndDate.Value <= date)
        {
            return TransactionCheckStatus.InvalidRepeatEndDate;
        }
        Transaction.Date = date;
        Transaction.Description = description;
        Transaction.Type = type;
        if(selectedRepeat == 3)
        {
            selectedRepeat = 7;
        }
        else if(selectedRepeat > 3)
        {
            selectedRepeat -= 1;
        }
        Transaction.RepeatInterval = (TransactionRepeatInterval)selectedRepeat;
        Transaction.Amount = amount;
        Transaction.GroupId = groupName == "Ungrouped" ? -1 : (int)Groups.FirstOrDefault(x => x.Value == groupName).Key;
        Transaction.RGBA = rgba;
        if(receiptPath != null)
        {
            if (Path.Exists(receiptPath))
            {
                if (Path.GetExtension(receiptPath) == ".jpeg" || Path.GetExtension(receiptPath) == ".jpg" || Path.GetExtension(receiptPath) == ".png")
                {
                    Transaction.Receipt = Image.Load(receiptPath);
                }
                else if (Path.GetExtension(receiptPath) == ".pdf")
                {
                    Transaction.Receipt = ConvertPDFToJPEG(receiptPath);
                }
                else
                {
                    Transaction.Receipt = null;
                }
            }
            else
            {
                Transaction.Receipt = null;
            }
        }
        if(Transaction.RepeatInterval == TransactionRepeatInterval.Never)
        {
            Transaction.RepeatFrom = -1;
        }
        else if(Transaction.RepeatInterval != OriginalRepeatInterval)
        {
            Transaction.RepeatFrom = 0;
        }
        Transaction.RepeatEndDate = Transaction.RepeatInterval == TransactionRepeatInterval.Never ? null : repeatEndDate;
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
