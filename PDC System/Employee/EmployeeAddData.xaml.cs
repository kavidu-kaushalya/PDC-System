using Microsoft.Win32;
using Org.BouncyCastle.Asn1.Cms;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PDC_System
{
    public partial class EmployeeAddData : Window
    {
        public Employee Employee { get; private set; }
        public ImageInfo imageInfo { get; private set; }

        private string customFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImageImports");

        public EmployeeAddData()
        {
            InitializeComponent();
            LoadCities();
            string defaultImagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/default_profile.png");

            if (File.Exists(defaultImagePath))
            {
                ImageDisplay.Source = new BitmapImage(new Uri(defaultImagePath));
            }
        }

        private void adress_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // You might want to add event logic here or remove this method if not needed.
        }

        private void ImportImageButton_Click(object sender, RoutedEventArgs e)
        {
            // Open file dialog to select an image
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (openFileDialog.ShowDialog() == true)
            {
                string originalFilePath = openFileDialog.FileName;
                string fileExtension = Path.GetExtension(originalFilePath).ToLower();

                // Ensure the custom folder exists
                Directory.CreateDirectory(customFolderPath);

                // Find the next available file name
                int fileIndex = 1;
                string newFilePath = Path.Combine(customFolderPath, $"{fileIndex}{fileExtension}");

                // Keep incrementing the index until we find an unused file name
                while (File.Exists(newFilePath))
                {
                    fileIndex++;
                    newFilePath = Path.Combine(customFolderPath, $"{fileIndex}{fileExtension}");
                }

                // Save the image to the custom location with the new name
                File.Copy(originalFilePath, newFilePath);

                // Create a new ImageInfo object
                imageInfo = new ImageInfo
                {
                    OriginalPath = originalFilePath,
                    SavedLocation = newFilePath
                };

                // Display the imported image
                ImageDisplay.Source = new BitmapImage(new Uri(newFilePath));
            }
        }



        public class ImageInfo
        {
            public string OriginalPath { get; set; }
            public string SavedLocation { get; set; }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate Name
            if (string.IsNullOrWhiteSpace(EmployeeNameTextBox.Text))
            {
                MessageBox.Show("Please enter a valid Employee Name.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate ID
            if (string.IsNullOrWhiteSpace(EmployeeIDTextBox.Text))
            {
                MessageBox.Show("Please enter a valid Employee ID.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate Salary
            if (!decimal.TryParse(EmployeeSalaryTextBox.Text.Trim(), out decimal employeeS) || employeeS < 0)
            {
                MessageBox.Show("Please enter a valid positive number for Salary.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate Salary
            if (!decimal.TryParse(EmployeeBasicSalaryTextBox.Text.Trim(), out decimal employeeBS) || employeeBS < 0)
            {
                MessageBox.Show("Please enter a valid positive number for Basic Salary.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            // Validate OT
            if (!decimal.TryParse(EmployeeOTTextBox.Text, out decimal employeeOT) || employeeOT < 0)
            {
                MessageBox.Show("Please enter a valid positive number for Overtime Hours.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate DOT
            if (!decimal.TryParse(EmployeeDOTTextBox.Text, out decimal employeeDOT) || employeeDOT < 0)
            {
                MessageBox.Show("Please enter a valid positive number for Double Overtime Hours.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate Absent Days
            if (!decimal.TryParse(EmployeeAbsentTextBox.Text, out decimal employeeAbsent) || employeeAbsent < 0)
            {
                MessageBox.Show("Please enter a valid positive number for Absent.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Validate Absent Days
            if (!decimal.TryParse(NoPayTextBox.Text, out decimal NoPay) || NoPay < 0)
            {
                MessageBox.Show("Please enter a valid positive number for NoPay.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TimeSpan WeekDaysCheckin;
            if (!TimeSpan.TryParse($"{TxtCheckIn_Hour.Text}:{TxtCheckIn_Minute.Text}", out WeekDaysCheckin))
            {
                MessageBox.Show("Please enter a valid check-in time (HH:mm).", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            TimeSpan WeekDaysCheckout;
            if (!TimeSpan.TryParse($"{TxtCheckOut_Hour.Text}:{TxtCheckOut_Minute.Text}", out WeekDaysCheckout))
            {
                MessageBox.Show("Please enter a valid check-in time (HH:mm).", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TimeSpan SatCheckin;
            if (!TimeSpan.TryParse($"{TxtSatCheckIn_Hour.Text}:{TxtSatCheckIn_Minute.Text}", out SatCheckin))
            {
                MessageBox.Show("Please enter a valid check-in time (HH:mm).", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            TimeSpan SatCheckOut;
            if (!TimeSpan.TryParse($"{TxtSatCheckOut_Hour.Text}:{TxtSatCheckOut_Minute.Text}", out SatCheckOut))
            {
                MessageBox.Show("Please enter a valid check-in time (HH:mm).", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // ✅ Validate Email
            string email = EmployeeEmailTextBox.Text.Trim();
            if (string.IsNullOrEmpty(email) || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Please enter a valid email address.", "Invalid Email", MessageBoxButton.OK, MessageBoxImage.Warning);
                EmployeeEmailTextBox.Focus();
                return; // Stop saving
            }


            DateTime ValidFromDate = ValidFrom.SelectedDate ?? DateTime.Now;
            DateTime ValidToDate = ValidTo.SelectedDate ?? DateTime.Now;

            // Create Employee Object
            Employee = new Employee
            {
               
                EmployeeId = EmployeeIDTextBox.Text.Trim(),
                Name = EmployeeNameTextBox.Text.Trim(),
                EmployeeEmail = EmployeeEmailTextBox.Text.Trim(),
                jobrole = jrole.Text.Trim(),
                Address1 = Address1.Text.Trim(),
                Address2 = Address2.Text.Trim(),
                City = city.Text.Trim(),
                Province = province.Text.Trim(),
                Contactn1 = contact1.Text.Trim(),
                Contactn2 = contact2.Text.Trim(),
                NID = NatId.Text.Trim(),
                Birthday = birthday.SelectedDate,
                Department = Department.Text.Trim(),

                ValidFrom = ValidFromDate,
                ValidTo = ValidToDate,

                Monday = (Mon.IsChecked == true),
                Tuesday = (Tue.IsChecked == true),
                Wednesday = (Wen.IsChecked == true),
                Thursday = (The.IsChecked == true),
                Friday = (Fri.IsChecked == true),
                Saturday = (Sat.IsChecked == true),
                Sunday = (Sun.IsChecked == true),

                Nopay = NoPay,
                CheckIn = WeekDaysCheckin,
                CheckOut = WeekDaysCheckout,
                SaturdayCheckIn = SatCheckin,
                SaturdayCheckOut = SatCheckOut,

                BSalary = employeeBS,
                Salary = employeeS,
                OvertimeAmount = employeeOT,
                DoubleOvertimeAmount = employeeDOT,
                AbesentAmount = employeeAbsent,
            };

            // Check if imageInfo is null before using it
            if (imageInfo != null)
            {
                Employee.OriginalPath = imageInfo.OriginalPath;
                Employee.SavedLocation = imageInfo.SavedLocation;
            }
            else
            {
                // If no image is provided, leave these properties blank or set them to default values
                Employee.OriginalPath = string.Empty; // Or you can set it to null
                Employee.SavedLocation = string.Empty; // Or you can set it to null
            }

            // Update UI
            EmployeeSalaryTextBox.Text = employeeS.ToString("F2");

            DialogResult = true;
        }

        private void NumberOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Check if the input is a number using a regular expression
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numeric input (0-9) and limit to 10 characters
            if (!char.IsDigit(e.Text, 0) || ((sender as TextBox).Text.Length >= 10))
            {
                e.Handled = true;  // Prevent the input
            }
        }

        private bool vEntered = false;
        private void NatId_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow "V" only once
            if (e.Text == "V" && !vEntered)
            {
                vEntered = true;
            }
            // Block non-numeric characters and ensure the length doesn't exceed 10 characters
            else if ((!char.IsDigit(e.Text, 0) && e.Text != "V") || (NatId.Text.Length >= 12))
            {
                e.Handled = true;
            }
        }

        private void PreviewfloteTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string currentText = textBox?.Text ?? string.Empty;

            // Allow only numeric input (0-9) or a single decimal point
            if (!char.IsDigit(e.Text, 0) && e.Text != ".")
            {
                e.Handled = true;  // Prevent the input
            }
            else
            {
                // Allow only one decimal point
                if (e.Text == "." && currentText.Contains("."))
                {
                    e.Handled = true;  // Prevent input if there's already a decimal point
                }

                // Limit the length to 10 characters including the decimal point
                if (currentText.Length >= 10)
                {
                    e.Handled = true;  // Prevent input if length exceeds 10
                }
            }
        }



        private void TimeBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numbers (0-9)
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        // Validate hour range (0–23)
        private void TxtHour_LostFocus(object sender, RoutedEventArgs e)
        {
            var box = sender as TextBox;
            if (box == null) return;

            if (int.TryParse(box.Text, out int value))
            {
                if (value < 0) value = 0;
                else if (value > 23) value = 23;

                box.Text = value.ToString("00");
            }
            else
            {
                box.Text = "00";
            }
        }

        // Validate minute range (0–59)
        private void TxtMinute_LostFocus(object sender, RoutedEventArgs e)
        {
            var box = sender as TextBox;
            if (box == null) return;

            if (int.TryParse(box.Text, out int value))
            {
                if (value < 0) value = 0;
                else if (value > 59) value = 59;

                box.Text = value.ToString("00");
            }
            else
            {
                box.Text = "00";
            }
        }

        private void TxtHour_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox targetBox = null;

                if (sender == TxtCheckIn_Hour)
                    targetBox = TxtCheckIn_Minute;
                else if (sender == TxtCheckOut_Hour)
                    targetBox = TxtCheckOut_Minute;

                if (targetBox != null)
                {
                    targetBox.Focus();
                    targetBox.SelectAll(); // ✅ highlight the minute text
                }

                e.Handled = true; // prevent the default "ding" sound
            }
        }


        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                string text = textBox.Text;

                // If the text is not empty and is a valid number
                if (!string.IsNullOrWhiteSpace(text) && decimal.TryParse(text, out decimal result))
                {
                    // Format the number to 2 decimal places (e.g., 500 to 500.00)
                    textBox.Text = result.ToString("F2");
                }
            }
        }

        private void LoadCities()
        {
            string[] cities = new string[]
            {
        "Agalawatta", "Agrapathana", "Akuressa", "Akurana", "Akkaraipattu", "Alawatuwala",
        "Alawwa", "Aluthgama", "Ambalangoda", "Ambalantota", "Ampara", "Ampara City",
        "Anamaduwa", "Angoda", "Anuradhapura", "Anuradhapura City", "Aranayaka",
        "Attanagalla", "Athurugiriya", "Avissawella", "Baddegama", "Badulla", "Badulla City",
        "Balangoda", "Bandaragama", "Bandarawela", "Battaramulla", "Batticaloa", "Batticaloa City",
        "Beliatta", "Belihuloya", "Bentota", "Beruwala", "Bibile", "Bingiriya", "Biyagama",
        "Bokkawala", "Borella", "Boralesgamuwa", "Bulathkohupitiya", "Buttala", "Chavakacheri",
        "Chilaw", "Colombo", "Colombo 1", "Colombo 2", "Colombo 3", "Colombo 4", "Colombo 5",
        "Colombo 6", "Colombo 7", "Colombo 8", "Colombo 9", "Colombo 10", "Colombo 11",
        "Colombo 12", "Colombo 13", "Colombo 14", "Colombo 15", "Damana", "Dambulla",
        "Dehiattakandiya", "Dehiwala", "Delgoda", "Deniyaya", "Deraniyagala", "Dikoya", "Dikwella",
        "Dimbulagala", "Divulapitiya", "Dodanduwa", "Dompe", "Eheliyagoda", "Ella", "Embilipitiya",
        "Eravur", "Eratne", "Galagedara", "Galaha", "Galenbindunuwewa", "Galle", "Galle City",
        "Galnewa", "Gampaha", "Gampola", "Gampolawela", "Ganemulla", "Gelioya", "Giribawa",
        "Giritale", "Godakawela", "Gonapola Junction", "Habaraduwa", "Habarana", "Haggala",
        "Haldummulla", "Hali Ela", "Haliela", "Hambantota", "Hambantota City", "Hanwella",
        "Haputale", "Harispattuwa", "Hatton", "Hettipola", "Hikkaduwa", "Hingurakgoda", "Hiriyala",
        "Horana", "Horowpathana", "Horton Plains", "Hulftsdorp", "Ibbagamuwa", "Imbulgoda",
        "Ingiriya", "Ja-Ela", "Jaffna", "Jaffna City", "Kadawatha", "Kaduruwela", "Kaduwela",
        "Kahatagasdigiliya", "Kahawatta", "Kalawana", "Kalmunai", "Kalutara", "Kalutara City",
        "Kamburupitiya", "Kandana", "Kandavalai", "Kandy", "Kandy City", "Kantalai",
        "Karagoda Uyangoda", "Karapitiya", "Kataragama", "Katugastota", "Katunayake",
        "Kattankudy", "Kayts", "Kegalle", "Kegalle City", "Kekirawa", "Kelaniya", "Kesbewa",
        "Kilinochchi", "Kilinochchi City", "Kinniya", "Kiribathkumbura", "Kirindiwela",
        "Kirulapana", "Kochchikade", "Kolonnawa", "Kosgama", "Koslanda", "Kotadeniyawa",
        "Kotagala", "Kotmale", "Kottawa", "Kuchchaveli", "Kuliyapitiya", "Kundasale", "Kuragala",
        "Kurunegala", "Kurunegala City", "Laggala-Pallegama", "Lahugala", "Laksapana",
        "Lellopitiya", "Lindula", "Liyanagemulla", "Lunugala", "Lunugamvehera", "Madakalapuwa",
        "Madampe", "Madhu", "Madurankuliya", "Mahagalgamuwa", "Maharagama", "Mahawa",
        "Mahiyanganaya", "Mailapitiya", "Maho", "Makandura", "Makumbura", "Malabe", "Maligawila",
        "Mallakam", "Manampitiya", "Mandaitivu", "Mannar", "Manthai East", "Maradana", "Marawila",
        "Maskeliya", "Matale", "Matara", "Matara City", "Mathugama", "Mawanella", "Mawathagama",
        "Medagama", "Medawachchiya", "Medirigiriya", "Meegoda", "Meegollewa", "Meepilimana",
        "Melsiripura", "Mihintale", "Minneriya", "Mirigama", "Mirissa", "Mitiswala",
        "Miyana Eliya", "Monaragala", "Monaragala City", "Moratuwa", "Moronthuduwa",
        "Mount Lavinia", "Mullaitivu", "Mullaitivu City", "Mundel", "Murunkan", "Mutur",
        "Nagalagam Street", "Nallur", "Narahenpita", "Naula", "Navatkuli", "Nawalapitiya", "Nawinna",
        "Negombo", "Negombo City", "Nikaweratiya", "Nilaveli", "Nittambuwa", "Norocholai",
        "Nugegoda", "Oddamavadi", "Ohiya", "Padukka", "Padiyathalawa", "Pahala Madampella",
        "Pallekele", "Panadura", "Pannala", "Pannipitiya", "Parakaduwa", "Passara", "Pattiyapola",
        "Pellessa", "Pelmadulla", "Peradeniya", "Piliyandala", "Pitabeddara", "Pitakotte",
        "Point Pedro", "Polgahawela", "Polonnaruwa", "Polonnaruwa City", "Poonakary",
        "Poruwadanda", "Pothuhera", "Pottuvil", "Puliyankulam", "Punanai", "Puttalam",
        "Puthukkudiyiruppu", "Ragama", "Rajagiriya", "Rajanganaya", "Rambaikulam", "Rambukkana",
        "Ranna", "Ranala", "Ratmalana", "Ratnapura", "Ratnapura City", "Rideegama", "Ruwanwella",
        "Sabaragamuwa", "Sagama", "Saliyapura", "Sammanthurai", "Seeduwa", "Seenigama",
        "Seethawakapura", "Serunuwara", "Seruvila", "Sigiriya", "Siyambalanduwa", "Sooriyawewa",
        "Talawakele", "Tambuttegama", "Tangalle", "Tawalama", "Teldeniya", "Tissamaharama",
        "Trincomalee", "Trincomalee City", "Tulhiriya", "Udadumbara", "Udappuwa", "Ukuwela",
        "Ulpotha", "Unawatuna", "Uragasmanhandiya", "Vaddukoddai", "Vakarai", "Valachchenai",
        "Vandaramulla", "Vavuniya", "Vavuniya City", "Veyangoda", "Wadduwa", "Waga", "Waikkala",
        "Warakapola", "Wariyapola", "Waskaduwa", "Watapuluwa", "Wattegama", "Wattegoda",
        "Wavulagala", "Weligama", "Welimada", "Welimessa", "Wellampitiya", "Wellawatte",
        "Wellawaya", "Wennappuwa", "Weragala", "Wewalwatta", "Wilgamuwa", "Wilpattu", "Wiralur",
        "Wiyaluwa", "Yakkala", "Yala", "Yatiyantota", "Yodakandiya", "Yogarathnapura"
            };

            foreach (string cityName in cities)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = cityName;
                city.Items.Add(item);
            }
        }



        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
