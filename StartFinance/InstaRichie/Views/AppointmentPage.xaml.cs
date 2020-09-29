using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SQLite.Net;
using StartFinance.Models;
using Windows.UI.Popups;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace StartFinance.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppointmentPage : Page
    {
        DateTime currentTime;
        SQLiteConnection conn; // adding an SQLite connection
        string path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Findata.sqlite");

        public AppointmentPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            /// Initializing a database
            conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);

            // Creating table
            Results();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Resets the UI if the user left during an edit
            // AddButton.Visibility = Visibility.Visible;
            TransactionList.Visibility = Visibility.Visible;
            currentTime = DateTime.Now;
            // _DatePicker.MaxYear = currentTime;
            _DatePicker.Date = currentTime;

            ClearFields();
        }
        public void Results()
        {
            // Creating table
            conn.CreateTable<AppointmentInfo>();
            var query = conn.Table<AppointmentInfo>();
            TransactionList.ItemsSource = query.ToList();
        }
        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // checks if account name is null
                if (_EventName.Text.ToString() == "" || _Location.Text.ToString() == "" || _DatePicker.ToString() == "" || _StartTime.ToString() == "" || _EndTime.ToString() == "")
                {
                    MessageDialog dialog = new MessageDialog("All Fields must be entered", "Oops..!");
                    await dialog.ShowAsync();
                }
                else if (_DatePicker.Date < currentTime)
                {
                    MessageDialog dialog = new MessageDialog("Check the date", "Oops..!");
                    await dialog.ShowAsync();
                }

                else
                {   // Inserts the data
                    conn.Insert(new AppointmentInfo()
                    {
                        EventName = _EventName.Text,
                        Location = _Location.Text,
                        EventDate = _DatePicker.Date.ToString("yyyy/MM/dd"),
                        StartTime = _StartTime.Time.ToString(),
                        EndTime = _EndTime.Time.ToString()
                    });
                    ClearFields();
                    Results();
                }
            }
            catch (Exception ex)
            {   // Exception to display when name is numbers
                if (ex is FormatException)
                {
                    MessageDialog dialog = new MessageDialog("You forgot to enter Event Name or entered an invalid data", "Oops..!");
                    await dialog.ShowAsync();
                }
                else
                {
                    /// Do Nothing
                }
            }
        }
        private async void EditItemButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int AppointmentInfo = ((AppointmentInfo)TransactionList.SelectedItem).AppointmentID;
                AppointmentInfo infoObject = new AppointmentInfo();
                infoObject = conn.Find<AppointmentInfo>(AppointmentInfo);
                _EventName.Text = infoObject.EventName;
                _Location.Text = infoObject.Location;
                _DatePicker.Date = Convert.ToDateTime(infoObject.EventDate);
                _StartTime.Time = Convert.ToDateTime(infoObject.StartTime).TimeOfDay;
                _EndTime.Time = Convert.ToDateTime(infoObject.EndTime).TimeOfDay;


                //switched out the add new entry button for the save edited button
                AddButton.Visibility = Visibility.Collapsed;
                SaveButton.Visibility = Visibility.Visible;
                // Hides the sql List so that the selection can't be changed while editing
                TransactionList.Visibility = Visibility.Collapsed;
            }
            catch (NullReferenceException)
            {
                MessageDialog ClearDialog = new MessageDialog("Please select the item to Edit", "Oops..!");
                await ClearDialog.ShowAsync();
            }
        }
        //saves the edited info over the existing info
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_EventName.Text.ToString() == "" || _Location.Text.ToString() == "" || _DatePicker.ToString() == "" || _StartTime.ToString() == "" || _EndTime.ToString() == "")
                {
                    MessageDialog dialog = new MessageDialog("All Fields must be entered", "Oops..!");
                    await dialog.ShowAsync();
                }
                else if (_DatePicker.Date < currentTime.Date)
                {
                    MessageDialog dialog = new MessageDialog("Check the date", "Oops..!");
                    await dialog.ShowAsync();
                }
                else
                {
                    AppointmentInfo temp = ((AppointmentInfo)TransactionList.SelectedItem);

                    temp.EventName = _EventName.Text;
                    temp.Location = _Location.Text;
                    temp.EventDate = _DatePicker.Date.ToString("yyyy/MM/dd");
                    temp.StartTime = _StartTime.Time.ToString();
                    temp.EndTime = _EndTime.Time.ToString();

                    conn.Update(temp);
                    // Resets the UI when edit is complete
                    AddButton.Visibility = Visibility.Visible;
                    SaveButton.Visibility = Visibility.Collapsed;
                    TransactionList.Visibility = Visibility.Visible;

                    Results();
                    ClearFields();
                }
            }
            catch (Exception ex)
            {   // Exception to display when amount is invalid or not numbers
                if (ex is FormatException)
                {
                    MessageDialog dialog = new MessageDialog("You forgot to enter the data or entered an invalid data", "Oops..!");
                    await dialog.ShowAsync();
                }
                else
                {
                }
            }
        }

        // Displays the data when navigation between pages
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Results();
        }
        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog ShowConf = new MessageDialog("Deleting this info will delete all data of this Name", "Important");
            ShowConf.Commands.Add(new UICommand("Yes, Delete")
            {
                Id = 0
            });
            ShowConf.Commands.Add(new UICommand("Cancel")
            {
                Id = 1
            });
            ShowConf.DefaultCommandIndex = 0;
            ShowConf.CancelCommandIndex = 1;

            var result = await ShowConf.ShowAsync();
            if ((int)result.Id == 0)
            {
                // checks if data is null else inserts
                try
                {
                    AppointmentInfo temp = (AppointmentInfo)TransactionList.SelectedItem;
                    conn.Delete(temp);
                    Results();

                }
                catch (NullReferenceException)
                {
                    MessageDialog dialog = new MessageDialog("Please select the item to Delete", "Oops..!");
                    await dialog.ShowAsync();
                }
            }
            else
            {
                // Do Nothing
            }
        }
        public void ClearFields()
        {
            _EventName.Text = "";
            _Location.Text = "";
            _DatePicker.Date = currentTime;
            _StartTime.Time = currentTime.TimeOfDay;
            _EndTime.Time = currentTime.TimeOfDay;
        }
    }
}
