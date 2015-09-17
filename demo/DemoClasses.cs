using System;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media.Imaging;

// Sample classes to be shown in WpfPropertyGrid
// They have regular and customized properties, and custom editors as well

namespace WpfPropertyGrid_Demo
{
    // Simple sample class with properties of several types
    public class Person
    {
        public enum Gender { Male, Female }

        #region private fields
        private string[] _Names = new string[3];
        #endregion

        // The following properties are wrapping an array of strings
        #region Public Properties
        [Category("Name")]
        [DisplayName("First Name")]
        public string FirstName
        {
            set { _Names[0] = value; }
            get { return _Names[0]; }
        }

        [Category("Name")]
        [DisplayName("Mid Name")]
        public string MidName
        {
            set { _Names[1] = value; }
            get { return _Names[1]; }
        }

        [Category("Name")]
        [DisplayName("Last Name")]
        public string LastName
        {
            set { _Names[2] = value; }
            get { return _Names[2]; }
        }

        // The following are autoimplemented properties (C# 3.0 and up)
        [Category("Characteristics")]
        [DisplayName("Gender")]
        public Gender PersonGender { get; set; }

        [Category("Characteristics")]
        [DisplayName("Birth Date")]
        public DateTime BirthDate { get; set; }

        [Category("Characteristics")]
        public int Income { get; set; }

        // Other cases of hidden read-only property and formatted property
        [DisplayName("GUID"), ReadOnly(true), Browsable(true)]   // many attributes defined in the same row
        public string GuidStr
        {
            get { return Guid.ToString(); }
        }

        [Browsable(false)]  // this property will not be displayed
        public System.Guid Guid
        {
            get;
            private set;
        }
        #endregion

        public Person()
        {
            // default values
            for (int i = 0; i < 3; i++)
                _Names[i] = "";
            this.PersonGender = Gender.Male;
            this.Guid = System.Guid.NewGuid();
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", FirstName, MidName, LastName).Trim().Replace("  ", " ");
        }
    }

    // Sample class with custom properties. Implements ICustomTypeDescriptor interface.
    public class Vehicle : ICustomTypeDescriptor, INotifyPropertyChanged
    {
        public enum CarType { Sedan, StationWagon, Coupe, Roadster, Van, Pickup, Truck } 
        public enum CarBrand { Acura, Audi, BMW, Citroen, Ford, GMC, Honda, Lexus, Mercedes, Mitsubishi, Nissan, Porshe, Suzuki, Toyota, VW, Volvo }

        #region Private fields
        private CarType _TypeOfCar;
        #endregion

        #region Public Properties
        [Category("Classification")]
        public CarBrand Brand { get; set; }

        [Category("Classification")]
        [DisplayName("Type")]
        [Description("Extra fields can be shown depending on type of car.")]
        public CarType TypeOfCar
        {
            get
            {
                return this._TypeOfCar;
            }
            set
            {
                this._TypeOfCar = value;
                NotifyPropertyChanged("TypeOfCar");
            }
        }

        [Category("Classification")]
        public string Model { get; set; }

        [Category("Identification")]
        [DisplayName("Manuf.Year")]
        public int Year { get; set; }

        [Category("Identification")]
        [DisplayName("License Plate")]
        public string Plate { get; set; }

        // Will shown only for Pickup and Truck
        [Category("Capacity")]
        [DisplayName("Volume (ft³)")]
        public int Volume { get; set; }

        [Category("Capacity")]
        [DisplayName("Payload (kg)")]
        public int Payload { get; set; }

        [Category("Capacity")]
        [DisplayName("Crew cab?")]
        public bool CrewCab { get; set; }
        #endregion

        #region ICustomTypeDescriptor Members
        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }
        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }
        public string GetComponentName()
        {
            return null;
        }
        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }
        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }
        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }
        public object GetEditor(Type editorBaseType)
        {
            return null;
        }
        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }
        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }
        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return this.GetProperties();
        }

        // Method implemented to expose Volume and PayLoad properties conditionally, depending on TypeOfCar
        public PropertyDescriptorCollection GetProperties()
        {
            var props = new PropertyDescriptorCollection(null);

            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(this, true))
            {
                if (prop.Category=="Capacity" && (this.TypeOfCar != CarType.Pickup && this.TypeOfCar != CarType.Truck))
                    continue;
                props.Add(prop);
            }

            return props;
        }
        #endregion

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion
    }

    public enum Continent { Africa = 1, America = 2, Asia = 3, Europe = 4, Oceania = 5 }

    public class FilteredCountriesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<Place.CountryInfo> items = new ObservableCollection<Place.CountryInfo>(Place.CountryInfo.Countries);
            CollectionView cv = CollectionViewSource.GetDefaultView(items) as CollectionView;

            if (values != null && values[0] != null)
            {
                Continent continent = (Continent)values[0];
                if (continent > 0)
                    cv.Filter = new Predicate<object>(c => ((Place.CountryInfo)c).Contin == continent);
            }
            return items;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Sample class with properties with custom editors
    public class Place
    {
        public struct CountryInfo
        {
            public static readonly CountryInfo[] Countries = {
            // African countries
            new CountryInfo(Continent.Africa , "AO", "ANGOLA" ),
            new CountryInfo(Continent.Africa, "CM", "CAMEROON" ),
            // American countries
            new CountryInfo(Continent.America, "MX", "MEXICO" ),
            new CountryInfo(Continent.America, "PE", "PERU" ),
            // Asian countries
            new CountryInfo(Continent.Asia, "JP", "JAPAN" ),
            new CountryInfo(Continent.Asia, "MN", "MONGOLIA" ),
            // European countries
            new CountryInfo(Continent.Europe, "DE", "GERMANY" ),
            new CountryInfo(Continent.Europe, "NL", "NETHERLANDS" ),
            // Oceanian countries
            new CountryInfo(Continent.Oceania, "AU", "AUSTRALIA" ),
            new CountryInfo(Continent.Oceania, "NZ", "NEW ZEALAND" )
        };

            public Continent Contin { get; set; }
            public string Abrev { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return string.Format("{0} ({1})", Name, Abrev);
            }
            public CountryInfo(Continent _continent, string _abrev, string _name)
                : this()
            {
                this.Contin = _continent;
                this.Abrev = _abrev;
                this.Name = _name;
            }
        }

        #region Private fields
        private string[] _Address = new string[4];
        #endregion

        #region Public properties
        [Category("Address")]
        public string Street
        {
            get { return _Address[0]; }
            set { _Address[0] = value; }
        }
        
        [Category("Address")]
        public string City
        {
            get { return _Address[1]; }
            set { _Address[1] = value; }
        }
        
        [Category("Address")]
        [Description("Province, state or department, depending on selected country")]
        public string Province
        {
            get { return _Address[2]; }
            set { _Address[2] = value; }
        }
        
        [Category("Address")]
        [Description("ZIP or other postal code according to the selected country")]
        public string Postal
        {
            get { return _Address[3]; }
            set { _Address[3] = value; }
        }

        [Category("Address")]
        [Editor(typeof(CountryEditor), typeof(PropertyValueEditor))]
        public CountryInfo Country { get; set; }
        
        [Category("Characteristics")]
        [Description("Photo or drawing of the place")]
        [Editor(typeof(PictureEditor), typeof(PropertyValueEditor))]
        public BitmapImage Picture { get; set; }
        
        [Category("Characteristics")]
        public int Floors { get; set; }
        
        [Category("Characteristics")]
        public int CurrentValue { get; set; }

        [Category("Proprietary")]
        public string FirstName { get; set; }

        [Category("Proprietary")]
        public string LastName { get; set; }
        #endregion

        public Place()
        {
            for (int i = 0; i < _Address.Length; i++)
                _Address[i] = string.Empty;
            this.Country = CountryInfo.Countries[0];
        }
    }
}
