using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace NetworkFileExplorer.WpfApplication.DataModels;

public class DispatchedObservableCollection<T> : ObservableCollection<T>
{
    public override event NotifyCollectionChangedEventHandler? CollectionChanged;
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        NotifyCollectionChangedEventHandler? collectionChanged = CollectionChanged;
        if (collectionChanged == null)
            return;
        
        Dispatcher? dispatcher = (from NotifyCollectionChangedEventHandler handler
        in collectionChanged.GetInvocationList()
                                  let dispatcherObject = handler.Target as DispatcherObject
                                  where dispatcherObject != null
                                  select dispatcherObject.Dispatcher).FirstOrDefault();
        if (dispatcher != null && !dispatcher.CheckAccess())
            dispatcher.Invoke(DispatcherPriority.DataBind, () => OnCollectionChanged(e));
        else
            foreach (NotifyCollectionChangedEventHandler handler in collectionChanged.GetInvocationList())
                handler.Invoke(this, e);
    }

}