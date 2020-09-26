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
    public sealed partial class PersonalInfoPage : Page
    {
        DateTime currentTime;
        SQLiteConnection conn; // adding an SQLite connection
        string path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Findata.sqlite");

        public PersonalInfoPage()
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
            conn.CreateTable<PersonalInfo>();
            var query = conn.Table<PersonalInfo>();
            TransactionList.ItemsSource = query.ToList();
        }
        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // checks if account name is null
                if (_FirstName.Text.ToString() == "" || _LastName.Text.ToString() == "" || _Email.Text.ToString() == ""|| _Phone.Text.ToString() == "")
                {
                    MessageDialog dialog = new MessageDialog("All Fields must be entered", "Oops..!");
                    await dialog.ShowAsync();
                }
                else if (_DatePicker.Date >= currentTime)
                {
                    MessageDialog dialog = new MessageDialog("Check the date", "Oops..!");
                    await dialog.ShowAsync();
                }
                else if (_GenderComboBox.SelectedIndex == -1)
                {
                    MessageDialog dialog = new MessageDialog("Select a Gender", "Oops..!");
                    await dialog.ShowAsync();
                }
                else
                {   // Inserts the data
                    conn.Insert(new PersonalInfo()
                    {
                        FirstName = _FirstName.Text,
                        LastName = _LastName.Text,
                        DOB = _DatePicker.Date.Date,
                        Gender = _GenderComboBox.SelectedValue.ToString(),
                        Email = _Email.Text,
                        MobilePhone = _Phone.Text
                });
                    ClearFields();
                    Results();
                }
            }
            catch (Exception ex)
            {   // Exception to display when name is numbers
                if (ex is FormatException)
                {
                    MessageDialog dialog = new MessageDialog("You forgot to enter the Name or entered an invalid data", "Oops..!");
                    await dialog.ShowAsync();
                }   // Exception handling when SQLite constraints are violated
                else if (ex is SQLiteException)
                {
                    MessageDialog dialog = new MessageDialog("Email or Mobile Phone already exist, it must be unique", "Oops..!");
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
                int personal_Id = ((PersonalInfo)TransactionList.SelectedItem).PersonalID;
                PersonalInfo infoObject = new PersonalInfo();
                infoObject = conn.Find<PersonalInfo>(personal_Id);
                _FirstName.Text = infoObject.FirstName;
                _LastName.Text = infoObject.LastName;
                _DatePicker.Date = infoObject.DOB;
                _GenderComboBox.SelectedItem = infoObject.Gender;
                _Email.Text = infoObject.Email;
                _Phone.Text = infoObject.MobilePhone;


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
                if (_FirstName.Text.ToString() == "" || _LastName.Text.ToString() == "" || _Email.Text == ""|| _Phone.Text == "")
                {
                    MessageDialog dialog = new MessageDialog("All Fields must be entered", "Oops..!");
                    await dialog.ShowAsync();
                }
                else if (_DatePicker.Date >= currentTime)
                {
                    MessageDialog dialog = new MessageDialog("Check the date", "Oops..!");
                    await dialog.ShowAsync();
                }
                else
                {
                    PersonalInfo temp = ((PersonalInfo)TransactionList.SelectedItem);

                    temp.FirstName = _FirstName.Text;
                    temp.LastName = _LastName.Text;
                    temp.DOB = _DatePicker.Date.Date;
                    temp.Gender = _GenderComboBox.SelectedValue.ToString();
                    temp.Email = _Email.Text;
                    temp.MobilePhone = _Phone.Text;

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
                }   // Exception handling when SQLite constraints are violated
                else if (ex is SQLiteException)
                {
                    MessageDialog dialog = new MessageDialog("Email or Mobile Phone already exist, it must be unique", "Oops..!");
                    await dialog.ShowAsync();
                }
                else
                {
                }
            }
        }
        /*// Clears the fields
        private async void ClearFileds_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog ClearDialog = new MessageDialog("Cleared", "information");
            await ClearDialog.ShowAsync();
        }*/

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
                    PersonalInfo temp = (PersonalInfo)TransactionList.SelectedItem;
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
            _FirstName.Text = "";
            _LastName.Text = "";
            _DatePicker.Date = currentTime;
            _GenderComboBox.SelectedItem = null;
            _Email.Text = "";
            _Phone.Text = "";
        }
    }
}
