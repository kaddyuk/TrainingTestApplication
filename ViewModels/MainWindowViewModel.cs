using EnvisionClient.Data.Contexts;
using EnvisionClient.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TrainingTestApplication.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly IDbContextFactory<TechRecordsDbContext> _dbContextFactory;
    private string filterString = string.Empty;
    private ICollectionView? partNumberCollectionView;

    public MainWindowViewModel(IDbContextFactory<TechRecordsDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;

        NotifyPartNumberList = new NotifyTaskCompletion<List<PartNumber>>(GetPartNumbers());
        NotifyPartNumberList.PropertyChanged += NotifyPartNumberList_PropertyChanged;
    }

    private void NotifyPartNumberList_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!NotifyPartNumberList.IsSuccessfullyCompleted) return;

        if (PartNumberCollectionView != null) return;

        PartNumberCollectionView = CollectionViewSource.GetDefaultView(NotifyPartNumberList.Result);
        PartNumberCollectionView.Filter = Filter;

        if(PartNumberCollectionView.CanGroup)
        {
            PartNumberCollectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PartNumber.Model)));
            PartNumberCollectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PartNumber.PartClassification)));
        }
    }

    /// <summary>
    /// Sets the property.
    /// </summary>
    /// <returns><c>true</c>, if property was set, <c>false</c> otherwise.</returns>
    /// <param name="backingStore">Backing store.</param>
    /// <param name="value">Value.</param>
    /// <param name="validateValue">Validates value.</param>
    /// <param name="propertyName">Property name.</param>
    /// <param name="onChanged">On changed.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    protected virtual bool SetProperty<T>(
    ref T backingStore, T value,
    [CallerMemberName] string propertyName = "",
    Action? onChanged = null,
    Func<T, T, bool>? validateValue = null)
    {
        //if value didn't change
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        //if value changed but didn't validate
        if (validateValue != null && !validateValue(backingStore, value))
            return false;

        backingStore = value;

        //OnErrorsChanged(propertyName);
        onChanged?.Invoke();
        OnPropertyChanged(propertyName);


        return true;
    }

    /// <summary>
    /// Occurs when property changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the property changed event.
    /// </summary>
    /// <param name="propertyName">Property name.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Get a list of part numbers from the Database Asynchronously
    /// </summary>
    /// <returns></returns>
    public async Task<List<PartNumber>> GetPartNumbers()
    {
        await using (var context = _dbContextFactory.Create())
        {
            return await (from sp in context.sPart
                          join pc in context.sPartClassification on sp.sPartClassification_ID equals pc.ID
                          join uom in context.sPartUnit on sp.sPartUnit_ID equals uom.ID
                          join un in context.sPartUnitName on uom.sPartUnitName_ID equals un.ID
                          join tmp in context.tModelPart on sp.ID equals tmp.sPart_ID into _tmp
                          from tmp in _tmp.DefaultIfEmpty()
                          join tm in context.tModel on tmp.tModel_ID equals tm.ID into _tm
                          from tm in _tm.DefaultIfEmpty()
                          select new PartNumber
                          {
                              ID = sp.ID,
                              PartNo = sp.PartNo,
                              PartDescription = sp.Description,
                              Model = tm.Model,
                              IsRotable = pc.Asset,
                              PartClassification = pc.Description,
                              RecordTimeStampCreated = sp.RecordTimeStampCreated,
                              UnitOfMeasure = $"{un.Unit} {uom.UnitQty:0.00}",
                          }).ToListAsync();
        }
    }

    public NotifyTaskCompletion<List<PartNumber>> NotifyPartNumberList { get; set; }

    public string FilterString { get => filterString; 
        set => SetProperty(ref filterString, value, onChanged: FilterStringOnChanged); }

    private void FilterStringOnChanged()
    {
        PartNumberCollectionView?.Refresh();
    }

    public Predicate<object> Filter
    {

        get
        {
            return x =>
            {
                if (x is not PartNumber part) return false;

                switch (part)
                {
                    case var _ when part.PartNo.Contains(FilterString, StringComparison.CurrentCultureIgnoreCase):
                    case var _ when part.PartDescription.Contains(FilterString, StringComparison.CurrentCultureIgnoreCase):
                    case var _ when part.PartClassification.Contains(FilterString, StringComparison.CurrentCultureIgnoreCase):
                        return true;
                    default:
                        return false;
                }
            };
        }
    }

    public ICollectionView? PartNumberCollectionView { get => partNumberCollectionView; set => SetProperty(ref partNumberCollectionView, value); }

}



public class ExpanderStateConverter : IMultiValueConverter
{

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        CollectionViewGroup collectionViewGroup = values[0] as CollectionViewGroup;
        List<string> expandersStates = values[1] as List<string>;
        if (!expandersStates.Any())
        {//prevent forming group identifier to speed up process as there are no expanded expanders anyway
            return false;
        }

        string groupId = MainWindow.FormViewGroupIdentifier(collectionViewGroup, null);
        bool contains = expandersStates.Contains(groupId);
        return contains;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return new object[2];
    }
}