using System;
using System.Activities.Presentation.Converters;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.PropertyEditing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

// Sample XAML Templates have been incorporated here for simplification of source code
// Usually they are placed into a Resource file

namespace WpfPropertyGrid_Demo
{
    /// <summary>
    /// Custom editor to select continent/country
    /// </summary>
    class CountryEditor : ExtendedPropertyValueEditor
    {
        public CountryEditor()
        {
            // Template for normal view
            string template1 = @"
                <DataTemplate
                    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                    xmlns:pe='clr-namespace:System.Activities.Presentation.PropertyEditing;assembly=System.Activities.Presentation'>
                    <DockPanel LastChildFill='True'>
                        <pe:EditModeSwitchButton TargetEditMode='ExtendedPopup' Name='EditButton' 
                        DockPanel.Dock='Right' />
                        <TextBlock Text='{Binding Value}' Margin='2,0,0,0' VerticalAlignment='Center'/>
                    </DockPanel>
                </DataTemplate>";

            // Template for extended view. Shown when dropdown button is pressed.
            string template2 = @"
                <DataTemplate
                    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                    xmlns:sys='clr-namespace:System;assembly=mscorlib'
                    xmlns:pe='clr-namespace:System.Activities.Presentation.PropertyEditing;assembly=System.Activities.Presentation'
                    xmlns:wpg='clr-namespace:WpfPropertyGrid_Demo;assembly=WpfPropertyGrid_Demo'>
                    <DataTemplate.Resources>
                        <ObjectDataProvider MethodName='GetValues' ObjectType='{x:Type sys:Enum}' x:Key='ContinentEnumValues'>
                            <ObjectDataProvider.MethodParameters>
                                <x:Type TypeName='wpg:Continent' />
                            </ObjectDataProvider.MethodParameters>
                        </ObjectDataProvider>
                        <wpg:FilteredCountriesConverter x:Key='CountriesConverter' />
                    </DataTemplate.Resources>
                    <StackPanel Orientation='Vertical' Background='White'>
                        <ComboBox Width='120' x:Name='ComboContinents' 
                            ItemsSource='{Binding Source={StaticResource ContinentEnumValues}}' 
                            SelectedItem='{Binding Path=Value.Contin,Mode=OneTime}' />
                        <ComboBox Width='120' SelectedItem='{Binding Path=Value, Mode=TwoWay, UpdateSourceTrigger=LostFocus}' 
                            IsSynchronizedWithCurrentItem='True'>
                            <ComboBox.ItemsSource>
                                <MultiBinding Converter='{StaticResource CountriesConverter}'>
                                    <Binding ElementName='ComboContinents' Path='SelectedItem' />
                                </MultiBinding>                   
                            </ComboBox.ItemsSource>
                       </ComboBox>
                    </StackPanel>
                </DataTemplate>";

            // Load templates
            using (var sr = new MemoryStream(Encoding.UTF8.GetBytes(template1)))
            {
                this.InlineEditorTemplate = XamlReader.Load(sr) as DataTemplate;
            }
            using (var sr = new MemoryStream(Encoding.UTF8.GetBytes(template2)))
            {
                this.ExtendedEditorTemplate = XamlReader.Load(sr) as DataTemplate;
            }
        }
    }

    /// <summary>
    /// Custom editor to select a picture
    /// </summary>
    class PictureEditor : DialogPropertyValueEditor
    {
        // Window to show the current image and optionally pick a different one
        public class ImagePickerWindow : Window
        {
            public BitmapImage TheImage = null;
            public Image ImageControl = null;

            public ImagePickerWindow(BitmapImage bitmap)
            {
                this.TheImage = bitmap;

                this.Title = "Select Picture:";
                this.Width = 195;
                this.Height = 235;
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                this.WindowStyle = System.Windows.WindowStyle.ToolWindow;
                this.ResizeMode = System.Windows.ResizeMode.NoResize;
                this.ShowInTaskbar = false;

                var image = new Image();
                image.Width = 180;
                image.Height = 180;
                image.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                image.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;

                var border = new Border();
                border.Width = 180;
                border.Height = 180;
                border.BorderThickness = new Thickness(1);
                border.BorderBrush = Brushes.Black;
                border.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                border.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                border.Margin = new Thickness(5, 5, 0, 0);
                border.Child = image;

                var button1 = new Button();
                button1.Content = "Pick...";
                button1.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                button1.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                button1.Width = 70;
                button1.Height = 20;
                button1.Margin = new Thickness(5, 190, 0, 0);
                button1.Click += new RoutedEventHandler(PickButton_Click);

                var button2 = new Button();
                button2.Content = "OK";
                button2.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                button2.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                button2.Width = 70;
                button2.Height = 20;
                button2.Margin = new Thickness(85, 190, 0, 0);
                button2.Click += new RoutedEventHandler(OKButton_Click);

                var grid = new Grid();
                grid.Children.Add(border);
                grid.Children.Add(button1);
                grid.Children.Add(button2);
                this.AddChild(grid);

                this.ImageControl = image;
                if (bitmap != null)
                    ImageControl.Source = bitmap;
            }

            void OKButton_Click(object sender, RoutedEventArgs e)
            {
                this.DialogResult = true;
                this.Close();
            }
            void PickButton_Click(object sender, RoutedEventArgs e)
            {
                OpenFileDialog dialog = new OpenFileDialog();

                dialog.CheckFileExists = true;
                dialog.DefaultExt = ".jpg";
                dialog.Filter = "Picture Files (*.jpg, *.bmp, *.png)|*.jpg;*.bmp;*.png";
                dialog.Multiselect = false;
                dialog.Title = "Select Picture";

                if (dialog.ShowDialog().Equals(true))
                {
                    try
                    {
                        BitmapImage image = new BitmapImage();
                        image.BeginInit();
                        image.UriSource = new Uri(dialog.FileName, UriKind.RelativeOrAbsolute);
                        image.EndInit();

                        this.TheImage = image;
                        this.ImageControl.Source = image;
                    }
                    catch { }
                }
            }
        }

        public PictureEditor()
        {
            string template = @"
                <DataTemplate
                    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                    xmlns:pe='clr-namespace:System.Activities.Presentation.PropertyEditing;assembly=System.Activities.Presentation'>
                    <DockPanel LastChildFill='True'>
                        <pe:EditModeSwitchButton TargetEditMode='Dialog' Name='EditButton' 
                        DockPanel.Dock='Right'>...</pe:EditModeSwitchButton>
                        <TextBlock Text='Picture' Margin='2,0,0,0' VerticalAlignment='Center'/>
                    </DockPanel>
                </DataTemplate>";

            using (var sr = new MemoryStream(Encoding.UTF8.GetBytes(template)))
            {
                this.InlineEditorTemplate = XamlReader.Load(sr) as DataTemplate;
            }
        }

        // Open the dialog to pick image, when the dropdown button is pressed 
        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            ImagePickerWindow window = new ImagePickerWindow(propertyValue.Value as BitmapImage);
            if (window.ShowDialog().Equals(true))
            {
                var ownerActivityConverter = new ModelPropertyEntryToOwnerActivityConverter();
                ModelItem activityItem = ownerActivityConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), false, null) as ModelItem;
                using (ModelEditingScope editingScope = activityItem.BeginEdit())
                {
                    propertyValue.Value = window.TheImage; 
                    editingScope.Complete(); // commit the changes

                    var control = commandSource as Control;
                    var oldData = control.DataContext;
                    control.DataContext = null;
                    control.DataContext = oldData;
                }
            }
        }
    }
}
