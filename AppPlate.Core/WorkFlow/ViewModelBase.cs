using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AppPlate.Core.Concurrency;

namespace AppPlate.Core.WorkFlow
{    
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private readonly ViewModelBase _parent;
        private readonly IDispatcher _dispatcher;
        private bool _isBusy;
        private bool _isActive;
        private string _busyMessage;
        private readonly Dictionary<string, string> _validationErrors;
        private readonly ValidationErrorMessageCollection _validationErrorMessageCollection;
        private readonly ValidationErrorIsValidCollection _validationErrorIsValidCollection;
        private readonly List<string> _validationInput;
        private string _notificationTitle;
        private string _notificationMessage;

        /// <summary>
        /// Initializes a sub viewmodel that defers command handling to it's parent
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="parent"></param>
        protected ViewModelBase(IDispatcher dispatcher, ViewModelBase parent) : this(dispatcher)
        {
            _parent = parent;
        }

        /// <summary>
        /// Initializes a new instance of the ViewModelBase class.
        /// 
        /// </summary>
        protected ViewModelBase(IDispatcher dispatcher)
        {
            if (dispatcher == null)
            {
                System.Diagnostics.Debug.Assert(this.TypeName() == "DesignTimeData",
                    "The empty constructor for ViewModelBase should ONLY be called by the DesignTimeData instances. Rename your class to " +
                    "DesignTimeData (and only use it as such) to suppress this Assert. A real viewmodel should be created by the Container " +
                    "with a real Dispatcher as argument");
            }

            _dispatcher = dispatcher;
            _validationErrors = new Dictionary<string, string>();
            _validationInput = new List<string>();
            _validationErrorMessageCollection = new ValidationErrorMessageCollection(_validationErrors);
            _validationErrorIsValidCollection = new ValidationErrorIsValidCollection(_validationErrors, _validationInput);
        }

        /// <summary>
        /// Gets or sets the current flow controlling this viewmodel.
        /// This property should not be set manually but should be
        /// controlled by the current <see cref="IUserInterfaceManager"/> and
        /// it's active <see cref="IUserInterfaceFlow"/>
        /// </summary>
        public IUserInterfaceFlow Flow { get; set; }

        /// <summary>
        /// Gets the Dispatcher set for this ViewModel
        /// </summary>
        public IDispatcher Dispatcher
        {
            get { return _dispatcher; }
        }

        /// <summary>
        /// Helper method that sets the IsActive property through the dispatcher
        /// thus allowing background threads to safely set this property without
        /// wrapping it in a dispatcher action
        /// </summary>
        /// <param name="isActive"></param>
        public void SetActive(bool isActive)
        {
            if (_dispatcher != null)
                _dispatcher.DispatchAction(() => IsActive = isActive);
            else
                IsActive = isActive;
        }

        /// <summary>
        /// Helper method that sets the IsBusy property through the dispatcher
        /// thus allowing background threads to safely set this property without
        /// wrapping it in a dispatcher action
        /// </summary>
        /// <param name="isBusy"></param>
        /// <param name="busyMessage"></param>
        public void SetBusy(bool isBusy, string busyMessage = null)
        {
            if (_dispatcher != null)
            {
                IsBusy = isBusy;
                if (busyMessage.IsNotNullOrEmpty()) BusyMessage = busyMessage;
            }
            else
            {
                IsBusy = isBusy;
                if (busyMessage.IsNotNullOrEmpty()) BusyMessage = busyMessage;
            }
        }

        /// <summary>
        /// Get if the viewmodel is busy performing some background action.
        /// This property is controlled by the current flow.
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            private set
            {
                Set(() => IsBusy, ref _isBusy, value);
            }
        }

        /// <summary>
        /// Get if the current viewmodel is currently active
        /// Property is updated by the Flow, it should not be set manually. 
        /// Returns true when this viewmodel is the currently active viewmodel
        /// according to the active Flow
        /// </summary>
        public bool IsActive
        {
            get { return _isActive; }
            private set
            {
                Set(() => IsActive, ref _isActive, value);
            }
        }

        /// <summary>
        /// Gets and sets the message for when <see cref="IsBusy"/> is true
        /// </summary>
        public string BusyMessage
        {
            get { return _busyMessage; }
            set
            {
                Set(() => BusyMessage, ref _busyMessage, value);                
            }
        }

        /// <summary>
        /// Gets or sets the title to show in a general notification
        /// </summary>
        public string NotificationTitle
        {
            get { return _notificationTitle; }
            set { Set(() => NotificationTitle, ref _notificationTitle, value); }
        }

        /// <summary>
        /// Gets or sets the message to show in a general notification
        /// </summary>
        public string NotificationMessage
        {
            get { return _notificationMessage; }
            set { Set(() => NotificationMessage, ref _notificationMessage, value); }
        }

        /// <summary>
        /// Override this method in ViewModel implementations to create custom validation logic
        /// for the ViewModel
        /// </summary>
        protected virtual void Validate()
        {
            _validationErrors.Clear();
        }

        /// <summary>
        /// Call this method to force revalidation of the viewmodel
        /// and trigger any validation errors to the view
        /// </summary>
        public void ReValidate()
        {
            _validationErrorIsValidCollection.OnlyShowErrorForPropertiesWithInput = false;
            Validate();
            RaisePropertyChanged("IsPropertyValid");
        }

        /// <summary>
        /// Clears any displayed validation errors from the viewmodel
        /// This method should be called when entering a viewmodel to
        /// prevent the views from directly displaying errors before
        /// the user has had a chance to input information
        /// </summary>
        public void ClearValidation()
        {
            _validationInput.Clear();
            _validationErrors.Clear();
            _validationErrorIsValidCollection.OnlyShowErrorForPropertiesWithInput = true;
            RaisePropertyChanged(() => IsValid);
            RaisePropertyChanged(() => IsPropertyValid);
            RaisePropertyChanged(() => PropertyError);
        }

        /// <summary>
        /// Gets a value indicating if the viewmodel has validation errors.
        /// ValidationError can be set by overriding the Validate method
        /// in the ViewModel implementation 
        /// </summary>
        public bool IsValid
        {
            get
            {
                return !_validationErrors.Any();
            }
        }

        /// <summary>
        /// Returns the collection of current validation errors
        /// </summary>
        public ValidationErrorMessageCollection PropertyError
        {
            get
            {
                return _validationErrorMessageCollection;
            }
        }

        public ValidationErrorIsValidCollection IsPropertyValid
        {
            get
            {
                return _validationErrorIsValidCollection;
            }   
        }

        protected void Validate<T>(Expression<Func<T>> propertyExpression, Func<T, bool> validator, string errorMessage)
        {
            var memberExpression = propertyExpression.Body as MemberExpression;
            var propertyInfo = memberExpression.Member as PropertyInfo;
            var value = (T)propertyInfo.GetValue(this);
            var isValid = validator(value);
            if (!isValid)
            {
                var propertyName = GetPropertyName(propertyExpression);
                _validationErrors[propertyName] = errorMessage;
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event if needed, and broadcasts a
        ///             PropertyChangedMessage using the Messenger instance (or the
        ///             static default instance if no Messenger instance is available).
        /// 
        /// </summary>
        /// <typeparam name="T">The type of the property that
        ///             changed.</typeparam><param name="propertyName">The name of the property that
        ///             changed.</param><param name="oldValue">The property's value before the change
        ///             occurred.</param><param name="newValue">The property's value after the change
        ///             occurred.</param><param name="broadcast">If true, a PropertyChangedMessage will
        ///             be broadcasted. If false, only the event will be raised.</param>
        /// <remarks>
        /// If the propertyName parameter
        ///             does not correspond to an existing property on the current class, an
        ///             exception is thrown in DEBUG configuration only.
        /// </remarks>
        protected virtual void RaisePropertyChanged<T>(string propertyName, T oldValue, T newValue, bool broadcast)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("This method cannot be called with an empty string", "propertyName");
            RaisePropertyChanged(propertyName);
            //TODO: something missing here?
            if (!broadcast)
                return;
        }

        /// <summary>
        /// Raises the PropertyChanged event if needed, and broadcasts a
        ///             PropertyChangedMessage using the Messenger instance (or the
        ///             static default instance if no Messenger instance is available).
        /// 
        /// </summary>
        /// <typeparam name="T">The type of the property that
        ///             changed.</typeparam><param name="propertyExpression">An expression identifying the property
        ///             that changed.</param><param name="oldValue">The property's value before the change
        ///             occurred.</param><param name="newValue">The property's value after the change
        ///             occurred.</param><param name="broadcast">If true, a PropertyChangedMessage will
        ///             be broadcasted. If false, only the event will be raised.</param>
        protected virtual void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression, T oldValue, T newValue,
            bool broadcast)
        {
            var propertyChangedHandler = PropertyChangedHandler;
            if (propertyChangedHandler == null && !broadcast)
                return;
            var propertyName = GetPropertyName(propertyExpression);
            if (propertyChangedHandler != null)
                propertyChangedHandler(this, new PropertyChangedEventArgs(propertyName));
            if (!broadcast)
                return;
        }

        /// <summary>
        /// Provides access to the PropertyChanged event handler to derived classes.
        /// 
        /// </summary>
        protected PropertyChangedEventHandler PropertyChangedHandler
        {
            get
            {
                return PropertyChanged;
            }
        }

        /// <summary>
        /// Gets the parent for this viewmodel if it was created as a sub viewmodel
        /// </summary>
        protected ViewModelBase Parent
        {
            get { return _parent; }
        }

        /// <summary>
        /// Occurs after a property value changes.
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event if needed.
        /// 
        /// </summary>
        /// 
        /// <remarks>
        /// If the propertyName parameter
        ///             does not correspond to an existing property on the current class, an
        ///             exception is thrown in DEBUG configuration only.
        /// </remarks>
        /// <param name="propertyName">The name of the property that
        ///             changed.</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler changedEventHandler = PropertyChanged;
            if (changedEventHandler == null)
                return;
            changedEventHandler((object)this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the PropertyChanged event if needed.
        /// 
        /// </summary>
        /// <typeparam name="T">The type of the property that
        ///             changed.</typeparam><param name="propertyExpression">An expression identifying the property
        ///             that changed.</param>
        protected virtual void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            PropertyChangedEventHandler changedEventHandler = PropertyChanged;
            if (changedEventHandler == null)
                return;
            string propertyName = this.GetPropertyName<T>(propertyExpression);
            changedEventHandler((object)this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Extracts the name of a property from an expression.
        /// 
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam><param name="propertyExpression">An expression returning the property's name.</param>
        /// <returns>
        /// The name of the property returned by the expression.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">If the expression is null.</exception><exception cref="T:System.ArgumentException">If the expression does not represent a property.</exception>
        protected string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException("propertyExpression");
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Invalid argument", "propertyExpression");
            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("Argument is not a property", "propertyExpression");
            else
                return propertyInfo.Name;
        }

        /// <summary>
        /// Assigns a new value to the property. Then, raises the
        ///             PropertyChanged event if needed.
        /// 
        /// </summary>
        /// <typeparam name="T">The type of the property that changed.</typeparam>
        /// <param name="propertyExpression">An expression identifying the property that changed.</param>
        /// <param name="field">The field storing the property's value.</param>
        /// <param name="newValue">The property's value after the change occurred.</param>
        /// <returns> True if the PropertyChanged event has been raised,
        ///           false otherwise. The event is not raised if the old
        ///           value is equal to the new value.
        /// </returns>
        protected bool Set<T>(Expression<Func<T>> propertyExpression, ref T field, T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;
            var propertyName = GetPropertyName(propertyExpression);
            Set(propertyName, ref field, newValue);
            
            return true;
        }

        /// <summary>
        /// Assigns a new value to the property. Then, raises the
        ///             PropertyChanged event if needed.
        /// 
        /// </summary>
        /// <typeparam name="T">The type of the property that changed.</typeparam>
        /// <param name="propertyName">The name of the property that changed.</param>
        /// <param name="field">The field storing the property's value.</param>
        /// <param name="newValue">The property's value after the change occurred.</param>
        /// <returns> True if the PropertyChanged event has been raised, false otherwise. 
        ///           The event is not raised if the old value is equal to the new value.
        /// </returns>
        protected bool Set<T>(string propertyName, ref T field, T newValue)
        {
#if DEBUG
            _dispatcher.AssertUIThread();
#endif
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            field = newValue;
            RaisePropertyChanged(propertyName);
            if( !_validationInput.Contains(propertyName)) _validationInput.Add(propertyName);
            Validate();
            RaisePropertyChanged("IsPropertyValid");
            RaisePropertyChanged("PropertyError");
            return true;
        }

        public class ValidationErrorMessageCollection
        {
            private readonly Dictionary<string, string> _validationErrors;

            public ValidationErrorMessageCollection(Dictionary<string, string> validationErrors)
            {
                _validationErrors = validationErrors;
            }

            public string this[string key]
            {
                get
                {
                    if (_validationErrors.ContainsKey(key))
                        return _validationErrors[key];

                    return null;
                }
            }
        }

        public class ValidationErrorIsValidCollection
        {
            private readonly Dictionary<string, string> _validationErrors;
            private readonly List<string> _validationInput;
            public bool OnlyShowErrorForPropertiesWithInput { get; set; }

            public ValidationErrorIsValidCollection(Dictionary<string, string> validationErrors, List<string> validationInput)
            {
                _validationErrors = validationErrors;
                _validationInput = validationInput;
                OnlyShowErrorForPropertiesWithInput = true;
            }

            public bool this[string key]
            {
                get
                {
                    // Only return false if we have any input on this property
                    if (!OnlyShowErrorForPropertiesWithInput || _validationInput.Contains(key) )
                    {
                        return !(_validationErrors.ContainsKey(key));    
                    }
                    return true;
                }
            }
        }
    }
}