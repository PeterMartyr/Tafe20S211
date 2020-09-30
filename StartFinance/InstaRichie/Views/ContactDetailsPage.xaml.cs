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
    public sealed partial class ContactDetailsPage : Page
    {
        SQLiteConnection conn; // adding an SQLite connection
        string path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Findata.sqlite");

        public ContactDetailsPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            /// Initializing a database
            conn = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);
            // Creating table
            Results();
        }

        public void Results()
        {
            // Creating table
            conn.CreateTable<ContactDetails>();
            var query = conn.Table<ContactDetails>();
            TransactionList.ItemsSource = query.ToList();
        }

        // Displays the data when navigation between pages
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Results();
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // checks if account name is null
                if (_FirstName.Text.ToString() == "" || _LastName.Text.ToString() == "" ||_CompanyName.Text.ToString() == "" || _MobilePhone.Text.ToString() == "")
                {
                    MessageDialog dialog = new MessageDialog("All Fields must be entered", "Oops..!");
                    await dialog.ShowAsync();
                }
                else
                {   // Inserts the data
                    conn.Insert(new ContactDetails()
                    {
                        FirstNameContact = _FirstName.Text,
                        LastNameContact = _LastName.Text,
                        CompanyNameContact = _CompanyName.Text,
                        MobilePhoneContact = _MobilePhone.Text,
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

        public void ClearFields()
        {

            _FirstName.Text = "";
            _LastName.Text = "";
            _CompanyName.Text = "";
            _MobilePhone.Text = "";
            
         
        }

        //saves the edited info over the existing info
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_FirstName.Text.ToString() == "" || _LastName.Text.ToString() == ""  || _CompanyName.Text.ToString() == "" || _MobilePhone.Text.ToString() == "")
                {
                    MessageDialog dialog = new MessageDialog("All Fields must be entered", "Oops..!");
                    await dialog.ShowAsync();
                
                }
                else
                {
                    ContactDetails temp = ((ContactDetails)TransactionList.SelectedItem);

                    temp.FirstNameContact = _FirstName.Text;
                    temp.LastNameContact = _LastName.Text;
                    temp.CompanyNameContact = _CompanyName.Text;
                    temp.MobilePhoneContact = _MobilePhone.Text;

                   
                  

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
                    MessageDialog dialog = new MessageDialog("Data already exist, it must be unique", "Oops..!");
                    await dialog.ShowAsync();
                }
                else
                {
                }
            }
        }

        private async void EditItemButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int contact_ID = ((ContactDetails)TransactionList.SelectedItem).ID;
                ContactDetails infoObject = new ContactDetails();
                infoObject = conn.Find<ContactDetails>(contact_ID);
                _FirstName.Text = infoObject.FirstNameContact;
                _LastName.Text = infoObject.LastNameContact; 
                _CompanyName.Text = infoObject.CompanyNameContact;
                _MobilePhone.Text = infoObject.MobilePhoneContact; 

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
                    ContactDetails temp = (ContactDetails)TransactionList.SelectedItem;
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
    }// class close
}// nameSpace close


