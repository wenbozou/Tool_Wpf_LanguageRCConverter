namespace LanguageRCConverter.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    ///   将代码转到UI线程
    /// </summary>
    public static class Execute
    {
        static bool? inDesignMode;
        static Action<System.Action> executor = action => action();

        /// <summary>
        ///   Indicates whether or not the framework is in design-time mode.
        /// </summary>
        public static bool InDesignMode
        {
            get
            {
                if (inDesignMode == null)
                {
                    var prop = DesignerProperties.IsInDesignModeProperty;
                    inDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(prop, typeof(FrameworkElement)).Metadata.DefaultValue;

                    if (!inDesignMode.GetValueOrDefault(false) && Process.GetCurrentProcess().ProcessName.StartsWith("devenv", StringComparison.Ordinal))
                        inDesignMode = true;
                }

                return inDesignMode.GetValueOrDefault(false);
            }
        }

        /// <summary>
        ///   初始化UI线程
        /// </summary>
        public static void InitializeWithDispatcher()
        {
            var dispatcher = Dispatcher.CurrentDispatcher;

            SetUIThreadMarshaller(action =>
            {
                try
                {
                    if (dispatcher.CheckAccess())
                        action();
                    else dispatcher.Invoke(action);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex;
                }
            });
        }

        /// <summary>
        ///   Resets the executor to use a non-dispatcher-based action executor.
        /// </summary>
        public static void ResetWithoutDispatcher()
        {
            SetUIThreadMarshaller(action => action());
        }

        /// <summary>
        /// Sets a custom UI thread marshaller.
        /// </summary>
        /// <param name="marshaller">The marshaller.</param>
        public static void SetUIThreadMarshaller(Action<System.Action> marshaller)
        {
            executor = marshaller;
        }

        /// <summary>
        ///  切换线程到UI线程
        /// </summary>
        /// <param name = "action">切换至UI线程中需要执行的Action</param>
        public static void OnUIThread(this System.Action action)
        {
            try
            {
                if (Application.Current == null || Application.Current.Dispatcher == null) return;
                Application.Current.Dispatcher.Invoke(action);
                // executor(action);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 封装System.Windows.Application.Current.Dispatcher.Invoke调用，尝试切换到主线程同步执行
        /// </summary>
        public static void UIThreadInvoke(DispatcherPriority priority, Delegate method)
        {
            Execute.OnUIThread(method as System.Action);
        }

        /// <summary>
        /// 封装System.Windows.Application.Current.Dispatcher.BeginInvoke的调用，尝试切换到主线程异步执行
        /// </summary>
        public static void UIThreadBeginInvoke(DispatcherPriority priority, Delegate method)
        {
            Application objApplication = System.Windows.Application.Current;
            if (null == objApplication)
            {
                return;
            }

            Dispatcher objDispatcher = objApplication.Dispatcher;
            if (null == objDispatcher)
            {
                return;
            }

            try
            {
                objDispatcher.BeginInvoke(priority, method);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 封装System.Windows.Application.Current.Dispatcher.BeginInvoke的调用，尝试切换到主线程异步执行
        /// 此方法需要慎用，有时可能会导致UI显示顺序错乱
        /// </summary>
        public static void UIThreadBeginInvoke(System.Action method)
        {
            UIThreadBeginInvoke(DispatcherPriority.Normal, method);
        }

        /// <summary>
        /// 同步运行，此方法需要在非UI线程中调用,否则会卡死UI 
        /// </summary>
        /// <param name="func">事件处理方法，需要返回结果，true or false.</param>
        /// <returns></returns>
        public static bool SyncRun(Func<bool> func)
        {
            if (Application.Current == null || Application.Current.Dispatcher == null) return false;
            AutoResetEvent resetEvent = new AutoResetEvent(false);
            bool res = false;
            Application.Current.Dispatcher.Invoke((System.Action)(() =>
            {
                res = true == func?.Invoke();
                resetEvent.Set();
            }));
            resetEvent.WaitOne();
            return res;
        }


        /// <summary>
        /// 同步运行，此方法需要在非UI线程中调用,否则会卡死UI 
        /// </summary>
        /// <param name="act">事件处理方法，需要返回结果，true or false.</param>
        /// <returns></returns>
        public static void SyncRunAction(Action<object> act, object arg)
        {
            if (Application.Current == null || Application.Current.Dispatcher == null) return;
            AutoResetEvent resetEvent = new AutoResetEvent(false);
            Application.Current.Dispatcher.Invoke((System.Action)(() =>
            {
                act?.Invoke(arg);
                resetEvent.Set();
            }));
            resetEvent.WaitOne();
        }

        /// <summary>
        /// 同步运行，此方法需要在非UI线程中调用,否则会卡死UI 
        /// </summary>
        /// <param name="act">事件处理方法，需要返回结果，true or false.</param>
        /// <returns></returns>
        public static void SyncRunAction(System.Action act)
        {
            if (Application.Current == null || Application.Current.Dispatcher == null) return;
            AutoResetEvent resetEvent = new AutoResetEvent(false);
            Application.Current.Dispatcher.Invoke((System.Action)(() =>
            {
                act?.Invoke();
                resetEvent.Set();
            }));
            resetEvent.WaitOne();
        }


        /// <summary>
        /// 在UI线程同步清空数据集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        public static void ClearItems<T>(IEnumerable<T> collection)
        {
            ClearItems(collection as ICollection<T>);
        }
        /// <summary>
        /// 在UI线程同步清空数据集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        public static void ClearItems<T>(ICollection<T> collection)
        {
            if (collection == null) return;
            SyncRunAction(() =>
            {
                collection.Clear();
            });
        }
    }

    /// <summary>
    ///   扩展 <see cref = "INotifyPropertyChanged" /> 使事件能被外部触发.
    /// </summary>
    public interface INotifyPropertyChangedEx : INotifyPropertyChanged {
        /// <summary>
        ///   Enables/Disables property change notification.
        /// </summary>
        bool IsNotifying { get; set; }

        /// <summary>
        ///   Notifies subscribers of the property change.
        /// </summary>
        /// <param name = "propertyName">Name of the property.</param>
        void NotifyOfPropertyChange(string propertyName);

        /// <summary>
        ///   Raises a change notification indicating that all bindings should be refreshed.
        /// </summary>
        void Refresh();
    }

    [Serializable]
    /// <summary>
    ///   A base class that implements the infrastructure for property change notification and automatically performs UI thread marshalling.
    /// </summary>
    public class PropertyChangedBase : INotifyPropertyChangedEx
    {
        /// <summary>
        ///   Creates an instance of <see cref = "PropertyChangedBase" />.
        /// </summary>
        public PropertyChangedBase()
        {
            IsNotifying = true;
        }

        [field: NonSerialized]
        /// <summary>
        ///   Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        [field: NonSerialized]
        bool isNotifying; //serializator try to serialize even autogenerated fields

        /// <summary>
        ///   Enables/Disables property change notification.
        /// </summary>
        public bool IsNotifying
        {
            get { return isNotifying; }
            set { isNotifying = value; }
        }

        /// <summary>
        ///   Raises a change notification indicating that all bindings should be refreshed.
        /// </summary>
        public void Refresh()
        {
            NotifyOfPropertyChange(string.Empty);
        }

        /// <summary>
        ///   Notifies subscribers of the property change.
        /// </summary>
        /// <param name = "propertyName">Name of the property.</param>
        public virtual void NotifyOfPropertyChange(string propertyName)
        {
            if (IsNotifying)
            {
                Execute.OnUIThread(() => RaisePropertyChangedEventCore(propertyName));
            }
        }

        /// <summary>
        ///   Notifies subscribers of the property change.
        /// </summary>
        /// <typeparam name = "TProperty">The type of the property.</typeparam>
        /// <param name = "property">The property expression.</param>
        public virtual void NotifyOfPropertyChange<TProperty>(Expression<Func<TProperty>> property)
        {
            //NotifyOfPropertyChange(property.GetMemberInfo().Name);
        }

        /// <summary>
        ///   Raises the property changed event immediately.
        /// </summary>
        /// <param name = "propertyName">Name of the property.</param>
        public virtual void RaisePropertyChangedEventImmediately(string propertyName)
        {
            if (IsNotifying)
            {
                RaisePropertyChangedEventCore(propertyName);
            }
        }

        void RaisePropertyChangedEventCore(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Called when the object is deserialized.
        /// </summary>
        /// <param name="c">The streaming context.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext c)
        {
            IsNotifying = true;
        }

        /// <summary>
        /// Used to indicate whether or not the IsNotifying property is serialized to Xml.
        /// </summary>
        /// <returns>Whether or not to serialize the IsNotifying property. The default is false.</returns>
        public virtual bool ShouldSerializeIsNotifying()
        {
            return false;
        }

        protected void OnPropertyChanged(Expression<Func<object, object>> propery)
        {
            var body = propery.Body.ToString();
            string propertyname = body.Substring(body.LastIndexOf(".") + 1);
            if (propertyname.EndsWith(")"))
            {
                propertyname = propertyname.Substring(0, propertyname.Length - 1);
            }
            this.RaisePropertyChangedEventImmediately(propertyname);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    ///   Represents a collection that is observable.
    /// </summary>
    /// <typeparam name = "T">The type of elements contained in the collection.</typeparam>
    public interface IObservableCollection<T> : IList<T>, INotifyPropertyChangedEx, INotifyCollectionChanged {
        /// <summary>
        ///   Adds the range.
        /// </summary>
        /// <param name = "items">The items.</param>
        void AddRange(IEnumerable<T> items);

        /// <summary>
        ///   Removes the range.
        /// </summary>
        /// <param name = "items">The items.</param>
        void RemoveRange(IEnumerable<T> items);
    }


    /// <summary>
    /// A base collection class that supports automatic UI thread marshalling.
    /// </summary>
    /// <typeparam name="T">The type of elements contained in the collection.</typeparam>
    [Serializable]
    public class BindableCollection<T> : ObservableCollection<T>, IObservableCollection<T> {
        
        /// <summary>
        ///   Initializes a new instance of the <see cref = "BindableCollection{T}" /> class.
        /// </summary>
        public BindableCollection() {
            IsNotifying = true;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "BindableCollection{T}" /> class.
        /// </summary>
        /// <param name = "collection">The collection from which the elements are copied.</param>
        /// <exception cref = "T:System.ArgumentNullException">
        ///   The <paramref name = "collection" /> parameter cannot be null.
        /// </exception>
        public BindableCollection(IEnumerable<T> collection) {
            IsNotifying = true;
            AddRange(collection);
        }

        [field: NonSerialized]
        bool isNotifying; //serializator try to serialize even autogenerated fields

        /// <summary>
        ///   Enables/Disables property change notification.
        /// </summary>
        public bool IsNotifying {
            get { return isNotifying; }
            set { isNotifying = value; }
        }

        /// <summary>
        ///   Notifies subscribers of the property change.
        /// </summary>
        /// <param name = "propertyName">Name of the property.</param>
        public void NotifyOfPropertyChange(string propertyName) {
            if(IsNotifying)
                Execute.OnUIThread(() => RaisePropertyChangedEventImmediately(new PropertyChangedEventArgs(propertyName)));
        }

        /// <summary>
        ///   Raises a change notification indicating that all bindings should be refreshed.
        /// </summary>
        public void Refresh() {
            Execute.OnUIThread(() => {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                OnPropertyChanged(new PropertyChangedEventArgs(string.Empty));
            });
        }

        /// <summary>
        ///   Inserts the item to the specified position.
        /// </summary>
        /// <param name = "index">The index to insert at.</param>
        /// <param name = "item">The item to be inserted.</param>
        protected override sealed void InsertItem(int index, T item) {
            Execute.OnUIThread(() => InsertItemBase(index, item));
        }

        /// <summary>
        ///   Exposes the base implementation of the <see cref = "InsertItem" /> function.
        /// </summary>
        /// <param name = "index">The index.</param>
        /// <param name = "item">The item.</param>
        /// <remarks>
        ///   Used to avoid compiler warning regarding unverifiable code.
        /// </remarks>
        protected virtual void InsertItemBase(int index, T item) {
            base.InsertItem(index, item);
        }

        /// <summary>
        /// Moves the item within the collection.
        /// </summary>
        /// <param name="oldIndex">The old position of the item.</param>
        /// <param name="newIndex">The new position of the item.</param>
        protected sealed override void MoveItem(int oldIndex, int newIndex)
        {
            Execute.OnUIThread(() => MoveItemBase(oldIndex, newIndex));
        }

        /// <summary>
        /// Exposes the base implementation fo the <see cref="MoveItem"/> function.
        /// </summary>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        /// <remarks>Used to avoid compiler warning regarding unverificable code.</remarks>
        protected virtual void MoveItemBase(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
        }

        /// <summary>
        ///   Sets the item at the specified position.
        /// </summary>
        /// <param name = "index">The index to set the item at.</param>
        /// <param name = "item">The item to set.</param>
        protected override sealed void SetItem(int index, T item) {
            Execute.OnUIThread(() => SetItemBase(index, item));
        }

        /// <summary>
        ///   Exposes the base implementation of the <see cref = "SetItem" /> function.
        /// </summary>
        /// <param name = "index">The index.</param>
        /// <param name = "item">The item.</param>
        /// <remarks>
        ///   Used to avoid compiler warning regarding unverifiable code.
        /// </remarks>
        protected virtual void SetItemBase(int index, T item) {
            base.SetItem(index, item);
        }

        /// <summary>
        ///   Removes the item at the specified position.
        /// </summary>
        /// <param name = "index">The position used to identify the item to remove.</param>
        protected override sealed void RemoveItem(int index) {
            Execute.OnUIThread(() => RemoveItemBase(index));
        }

        /// <summary>
        ///   Exposes the base implementation of the <see cref = "RemoveItem" /> function.
        /// </summary>
        /// <param name = "index">The index.</param>
        /// <remarks>
        ///   Used to avoid compiler warning regarding unverifiable code.
        /// </remarks>
        protected virtual void RemoveItemBase(int index) {
            base.RemoveItem(index);
        }

        /// <summary>
        ///   Clears the items contained by the collection.
        /// </summary>
        protected override sealed void ClearItems() {
            Execute.OnUIThread(ClearItemsBase);
        }

        /// <summary>
        ///   Exposes the base implementation of the <see cref = "ClearItems" /> function.
        /// </summary>
        /// <remarks>
        ///   Used to avoid compiler warning regarding unverifiable code.
        /// </remarks>
        protected virtual void ClearItemsBase() {
            base.ClearItems();
        }

        /// <summary>
        ///   Raises the <see cref = "E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged" /> event with the provided arguments.
        /// </summary>
        /// <param name = "e">Arguments of the event being raised.</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            if (IsNotifying) {
                base.OnCollectionChanged(e);
            }
        }

        /// <summary>
        ///   Raises the PropertyChanged event with the provided arguments.
        /// </summary>
        /// <param name = "e">The event data to report in the event.</param>
        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e) {
            if (IsNotifying) {
                base.OnPropertyChanged(e);

            }
        }


        void RaisePropertyChangedEventImmediately(PropertyChangedEventArgs e) {
            OnPropertyChanged(e);
        }

        /// <summary>
        ///   Adds the range.
        /// </summary>
        /// <param name = "items">The items.</param>
        public virtual void AddRange(IEnumerable<T> items) {
            Execute.OnUIThread(() => {
                var previousNotificationSetting = IsNotifying;
                IsNotifying = false;
                var index = Count;
                foreach(var item in items) {
                    InsertItemBase(index, item);
                    index++;
                }
                IsNotifying = previousNotificationSetting;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                OnPropertyChanged(new PropertyChangedEventArgs(string.Empty));
            });
        }

        /// <summary>
        ///   Removes the range.
        /// </summary>
        /// <param name = "items">The items.</param>
        public virtual void RemoveRange(IEnumerable<T> items) {
            Execute.OnUIThread(() => {
                var previousNotificationSetting = IsNotifying;
                IsNotifying = false;
                foreach(var item in items) {
                    var index = IndexOf(item);
                    RemoveItemBase(index);
                }
                IsNotifying = previousNotificationSetting;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                OnPropertyChanged(new PropertyChangedEventArgs(string.Empty));
            });
        }

        /// <summary>
        /// Called when the object is deserialized.
        /// </summary>
        /// <param name="c">The streaming context.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext c) {
            IsNotifying = true;
        }

        /// <summary>
        /// Used to indicate whether or not the IsNotifying property is serialized to Xml.
        /// </summary>
        /// <returns>Whether or not to serialize the IsNotifying property. The default is false.</returns>
        public virtual bool ShouldSerializeIsNotifying() {
            return false;
        }
    }
}