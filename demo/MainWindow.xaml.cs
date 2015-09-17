using System;
using System.Windows;
using System.Collections;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;

namespace WpfPropertyGrid_Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Person Person = new Person();
        private Vehicle Vehicle1 = new Vehicle();
        private Vehicle Vehicle2 = new Vehicle();
        private Place Place = new Place();

        // names must match the data members
        private object[] ItemArray = { "Person", "Vehicle1", "Vehicle2", "Place" };

        public MainWindow()
        {
            InitializeComponent();

            this.Vehicle1.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Vehicle_PropertyChanged);
            this.Vehicle2.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Vehicle_PropertyChanged);

            this.Radio3.IsChecked = true;
            this.NoSelection_Click(this, null);
        }

        // Special handling for vehicle type change
        void Vehicle_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.PropertyGrid1.RefreshPropertyList();
        }

        private void SingleSelect_Click(object sender, RoutedEventArgs e)
        {
            this.ItemList.ItemTemplate = this.Resources["RadioButtons"] as DataTemplate;
            this.ItemList.ItemsSource = this.ItemArray;
            this.PropertyGrid1.SelectedObject = null;
        }
        private void MultiSelect_Click(object sender, RoutedEventArgs e)
        {
            this.ItemList.ItemTemplate = this.Resources["CheckBoxes"] as DataTemplate;
            this.ItemList.ItemsSource = this.ItemArray;
            this.PropertyGrid1.SelectedObject = null;
        }
        private void NoSelection_Click(object sender, RoutedEventArgs e)
        {
            this.ItemList.ItemTemplate = null;
            this.ItemList.ItemsSource = new string[] { "(none)" };
            this.PropertyGrid1.SelectedObject = null;
        }

        private void Item_Checked(object sender, RoutedEventArgs e)
        {
            if (e.Source is RadioButton)
            {
                object selected = this.GetType().GetField((e.Source as RadioButton).Content.ToString(), 
                    System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(this);
                this.PropertyGrid1.SelectedObject = selected;
            }
            else if (e.Source is CheckBox && this.Radio2.IsChecked.GetValueOrDefault())
            {
                ArrayList selected = new ArrayList();

                for (int i = 0; i < ItemList.Items.Count; i++)
                {
                    ContentPresenter container = ItemList.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                    DataTemplate dataTemplate = container.ContentTemplate;
                    CheckBox chk = (CheckBox)dataTemplate.FindName("chk", container);
                    if (chk.IsChecked.GetValueOrDefault())
                    {
                        object item = this.GetType().GetField(chk.Content.ToString(), 
                            System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(this);
                        selected.Add(item);
                    }
                }
                this.PropertyGrid1.SelectedObjects = selected.ToArray();
            }
        }

        private void ShowDescrip_Click(object sender, RoutedEventArgs e)
        {
            this.PropertyGrid1.HelpVisible = !this.PropertyGrid1.HelpVisible;
        }

        private void ShowToolbar_Click(object sender, RoutedEventArgs e)
        {
            this.PropertyGrid1.ToolbarVisible = !this.PropertyGrid1.ToolbarVisible;
        }
    }
}
