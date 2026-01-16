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

        private Employee originalEmployee;


        private string customFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ImageImports");

        public EmployeeAddData()   // ADD NEW EMPLOYEE
        {
            InitializeComponent();
            LoadCities();

            Employee = new Employee();   // empty employee



            // 👉 Load image (if exists)
            if (!string.IsNullOrWhiteSpace(Employee.SavedLocation) && File.Exists(Employee.SavedLocation))
            {
                ImageDisplay.Source = new BitmapImage(new Uri(Employee.SavedLocation));
            }
            else
            {
                string defaultImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/default_profile.png");

                if (File.Exists(defaultImagePath))
                    ImageDisplay.Source = new BitmapImage(new Uri(defaultImagePath));
            }
        }


        public EmployeeAddData(Employee existingEmployee)
        {
            InitializeComponent();
            LoadCities();

            // 👉 Use the existing employee data
            Employee = existingEmployee;

            // Load basic text fields
            EmployeeNameTextBox.Text = Employee.Name;
            EmployeeIDTextBox.Text = Employee.EmployeeId;
            EmployeeEmailTextBox.Text = Employee.EmployeeEmail;
            jrole.Text = Employee.jobrole;
            Address1.Text = Employee.Address1;
            Address2.Text = Employee.Address2;
            city.Text = Employee.City;
            province.Text = Employee.Province;
            contact1.Text = Employee.Contactn1;
            contact2.Text = Employee.Contactn2;
            NatId.Text = Employee.NID;
            birthday.SelectedDate = Employee.Birthday;
            Department.Text = Employee.Department;

            ValidFrom.SelectedDate = Employee.ValidFrom;
            ValidTo.SelectedDate = Employee.ValidTo;

            Mon.IsChecked = Employee.Monday;
            Tue.IsChecked = Employee.Tuesday;
            Wen.IsChecked = Employee.Wednesday;
            The.IsChecked = Employee.Thursday;
            Fri.IsChecked = Employee.Friday;
            Sat.IsChecked = Employee.Saturday;
            Sun.IsChecked = Employee.Sunday;

            EmployeeSalaryTextBox.Text = Employee.Salary.ToString("F2");
            EmployeeBasicSalaryTextBox.Text = Employee.BSalary.ToString("F2");
            EmployeeOTTextBox.Text = Employee.OvertimeAmount.ToString("F2");
            EmployeeDOTTextBox.Text = Employee.DoubleOvertimeAmount.ToString("F2");
            EmployeeAbsentTextBox.Text = Employee.AbesentAmount.ToString("F2");
            NoPayTextBox.Text = Employee.Nopay.ToString("F2");

            // Times
            TxtCheckIn_Hour.Text = Employee.CheckIn.Hours.ToString("00");
            TxtCheckIn_Minute.Text = Employee.CheckIn.Minutes.ToString("00");

            TxtCheckOut_Hour.Text = Employee.CheckOut.Hours.ToString("00");
            TxtCheckOut_Minute.Text = Employee.CheckOut.Minutes.ToString("00");

            TxtSatCheckIn_Hour.Text = Employee.SaturdayCheckIn.Hours.ToString("00");
            TxtSatCheckIn_Minute.Text = Employee.SaturdayCheckIn.Minutes.ToString("00");

            TxtSatCheckOut_Hour.Text = Employee.SaturdayCheckOut.Hours.ToString("00");
            TxtSatCheckOut_Minute.Text = Employee.SaturdayCheckOut.Minutes.ToString("00");



            // store original copy (deep copy)
            originalEmployee = new Employee
            {
                EmployeeId = existingEmployee.EmployeeId,
                Name = existingEmployee.Name,
                EmployeeEmail = existingEmployee.EmployeeEmail,
                jobrole = existingEmployee.jobrole,
                Address1 = existingEmployee.Address1,
                Address2 = existingEmployee.Address2,
                City = existingEmployee.City,
                Province = existingEmployee.Province,
                Contactn1 = existingEmployee.Contactn1,
                Contactn2 = existingEmployee.Contactn2,
                NID = existingEmployee.NID,
                Birthday = existingEmployee.Birthday,
                Department = existingEmployee.Department,
                ValidFrom = existingEmployee.ValidFrom,
                ValidTo = existingEmployee.ValidTo,
                Salary = existingEmployee.Salary,
                BSalary = existingEmployee.BSalary,
                OvertimeAmount = existingEmployee.OvertimeAmount,
                DoubleOvertimeAmount = existingEmployee.DoubleOvertimeAmount,
                AbesentAmount = existingEmployee.AbesentAmount,
                Nopay = existingEmployee.Nopay,
                CheckIn = existingEmployee.CheckIn,
                CheckOut = existingEmployee.CheckOut,
                SaturdayCheckIn = existingEmployee.SaturdayCheckIn,
                SaturdayCheckOut = existingEmployee.SaturdayCheckOut,
                Monday = existingEmployee.Monday,
                Tuesday = existingEmployee.Tuesday,
                Wednesday = existingEmployee.Wednesday,
                Thursday = existingEmployee.Thursday,
                Friday = existingEmployee.Friday,
                Saturday = existingEmployee.Saturday,
                Sunday = existingEmployee.Sunday,
                OriginalPath = existingEmployee.OriginalPath,
                SavedLocation = existingEmployee.SavedLocation
            };





            // 👉 Load image (if exists)
            if (!string.IsNullOrWhiteSpace(Employee.SavedLocation) && File.Exists(Employee.SavedLocation))
            {
                ImageDisplay.Source = new BitmapImage(new Uri(Employee.SavedLocation));
            }
            else
            {
                string defaultImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/default_profile.png");

                if (File.Exists(defaultImagePath))
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

                CustomMessageBox.Show("Please enter a valid Employee Name.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate ID
            if (string.IsNullOrWhiteSpace(EmployeeIDTextBox.Text))
            {
                CustomMessageBox.Show("Please enter a valid Employee ID.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate Salary
            if (!decimal.TryParse(EmployeeSalaryTextBox.Text.Trim(), out decimal employeeS) || employeeS < 0)
            {
                CustomMessageBox.Show("Please enter a valid positive number for Salary.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate Salary
            if (!decimal.TryParse(EmployeeBasicSalaryTextBox.Text.Trim(), out decimal employeeBS) || employeeBS < 0)
            {
                CustomMessageBox.Show("Please enter a valid positive number for Basic Salary.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            // Validate OT
            if (!decimal.TryParse(EmployeeOTTextBox.Text, out decimal employeeOT) || employeeOT < 0)
            {
                CustomMessageBox.Show("Please enter a valid positive number for Overtime Hours.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate DOT
            if (!decimal.TryParse(EmployeeDOTTextBox.Text, out decimal employeeDOT) || employeeDOT < 0)
            {
                CustomMessageBox.Show("Please enter a valid positive number for Double Overtime Hours.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate Absent Days
            if (!decimal.TryParse(EmployeeAbsentTextBox.Text, out decimal employeeAbsent) || employeeAbsent < 0)
            {
                CustomMessageBox.Show("Please enter a valid positive number for Absent.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Validate Absent Days
            if (!decimal.TryParse(NoPayTextBox.Text, out decimal NoPay) || NoPay < 0)
            {
                CustomMessageBox.Show("Please enter a valid positive number for NoPay.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TimeSpan WeekDaysCheckin;
            if (!TimeSpan.TryParse($"{TxtCheckIn_Hour.Text}:{TxtCheckIn_Minute.Text}", out WeekDaysCheckin))
            {
                CustomMessageBox.Show("Please enter a valid check-in time (HH:mm).", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            TimeSpan WeekDaysCheckout;
            if (!TimeSpan.TryParse($"{TxtCheckOut_Hour.Text}:{TxtCheckOut_Minute.Text}", out WeekDaysCheckout))
            {
                CustomMessageBox.Show("Please enter a valid check-in time (HH:mm).", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TimeSpan SatCheckin;
            if (!TimeSpan.TryParse($"{TxtSatCheckIn_Hour.Text}:{TxtSatCheckIn_Minute.Text}", out SatCheckin))
            {
                CustomMessageBox.Show("Please enter a valid check-in time (HH:mm).", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            TimeSpan SatCheckOut;
            if (!TimeSpan.TryParse($"{TxtSatCheckOut_Hour.Text}:{TxtSatCheckOut_Minute.Text}", out SatCheckOut))
            {
                CustomMessageBox.Show("Please enter a valid check-in time (HH:mm).", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // ✅ Validate Email (allow empty, but validate format if provided)
            string email = EmployeeEmailTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                CustomMessageBox.Show("Please enter a valid email address.", "Invalid Email", MessageBoxButton.OK, MessageBoxImage.Warning);
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


            // ----------------------------
            // SHOW CHANGED FIELDS MESSAGE
            // ----------------------------
            if (originalEmployee != null) // only for edit mode
            {
                string changes = "Updated Successfully!\n\nChanged Fields:\n";

                void CheckChange(string fieldName, object oldValue, object newValue)
                {
                    if (!Equals(oldValue, newValue))
                    {
                        changes += $"{fieldName}: {oldValue} → {newValue}\n";
                    }
                }

                CheckChange("Employee ID", originalEmployee.EmployeeId, Employee.EmployeeId);
                CheckChange("Name", originalEmployee.Name, Employee.Name);
                CheckChange("Email", originalEmployee.EmployeeEmail, Employee.EmployeeEmail);
                CheckChange("Job Role", originalEmployee.jobrole, Employee.jobrole);
                CheckChange("Address 1", originalEmployee.Address1, Employee.Address1);
                CheckChange("Address 2", originalEmployee.Address2, Employee.Address2);
                CheckChange("City", originalEmployee.City, Employee.City);
                CheckChange("Province", originalEmployee.Province, Employee.Province);
                CheckChange("Contact 1", originalEmployee.Contactn1, Employee.Contactn1);
                CheckChange("Contact 2", originalEmployee.Contactn2, Employee.Contactn2);
                CheckChange("NIC", originalEmployee.NID, Employee.NID);
                CheckChange("Birthday", originalEmployee.Birthday?.Date, Employee.Birthday?.Date);

                CheckChange("Department", originalEmployee.Department, Employee.Department);

                CheckChange("Valid From", originalEmployee.ValidFrom.Date, Employee.ValidFrom.Date);
                CheckChange("Valid To", originalEmployee.ValidTo.Date, Employee.ValidTo.Date);


                CheckChange("Salary", originalEmployee.Salary, Employee.Salary);
                CheckChange("Basic Salary", originalEmployee.BSalary, Employee.BSalary);
                CheckChange("OT Amount", originalEmployee.OvertimeAmount, Employee.OvertimeAmount);
                CheckChange("Double OT", originalEmployee.DoubleOvertimeAmount, Employee.DoubleOvertimeAmount);
                CheckChange("Absent Amount", originalEmployee.AbesentAmount, Employee.AbesentAmount);
                CheckChange("Nopay", originalEmployee.Nopay, Employee.Nopay);

                CheckChange("Check-in", originalEmployee.CheckIn, Employee.CheckIn);
                CheckChange("Check-out", originalEmployee.CheckOut, Employee.CheckOut);
                CheckChange("Saturday Check-in", originalEmployee.SaturdayCheckIn, Employee.SaturdayCheckIn);
                CheckChange("Saturday Check-out", originalEmployee.SaturdayCheckOut, Employee.SaturdayCheckOut);

                // Working days
                CheckChange("Works Monday", originalEmployee.Monday, Employee.Monday);
                CheckChange("Works Tuesday", originalEmployee.Tuesday, Employee.Tuesday);
                CheckChange("Works Wednesday", originalEmployee.Wednesday, Employee.Wednesday);
                CheckChange("Works Thursday", originalEmployee.Thursday, Employee.Thursday);
                CheckChange("Works Friday", originalEmployee.Friday, Employee.Friday);
                CheckChange("Works Saturday", originalEmployee.Saturday, Employee.Saturday);
                CheckChange("Works Sunday", originalEmployee.Sunday, Employee.Sunday);

                // Detect image changes
                CheckChange("Profile Image", originalEmployee.SavedLocation, Employee.SavedLocation);

                CustomMessageBox.Show(changes, "Employee Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                NotificationHelper.ShowNotification("PDC System!", changes);
            }

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
