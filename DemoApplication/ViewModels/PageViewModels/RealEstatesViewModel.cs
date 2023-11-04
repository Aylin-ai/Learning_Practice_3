using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using DemoApplication.Infrastructure.Commands;
using DemoApplication.Infrastructure.DB;
using DemoApplication.Infrastructure.Stores;
using DemoApplication.Models;
using MySqlConnector;
using ReactiveUI;

namespace DemoApplication.ViewModels.PageViewModels;

public class RealEstatesViewModel : ViewModelBase
{
    public RealEstatesViewModel()
    {
        GetAllRealEstates();

        #region Команды

        SaveRealEstateCommand = new LambdaCommand(OnSaveRealEstateCommandExecuted);
        DeleteRealEstateCommand = new LambdaCommand(OnDeleteRealEstateCommandExecuted);
        CancelRealEstateEditionCommand = new LambdaCommand(OnCancelRealEstateEditionCommandExecuted);

        #endregion
    }

    #region Коллекции
    
    private ObservableCollection<RealEstate> _realEstates = new ObservableCollection<RealEstate>();
    public ObservableCollection<RealEstate> RealEstates
    {
        get => _realEstates;
        set => this.RaiseAndSetIfChanged(ref _realEstates, value);
    }
    
    private ObservableCollection<string> _comboBoxCollection = new ObservableCollection<string>()
    {
        "Нет", "Предложения", "Информация"
    };
    public ObservableCollection<string> ComboBoxCollection
    {
        get => _comboBoxCollection;
        set => this.RaiseAndSetIfChanged(ref _comboBoxCollection, value);
    }

    private ObservableCollection<string> _comboBoxCollectionOfRealEstateTypes = new ObservableCollection<string>()
    {
        "Квартира", "Дом", "Земля"
    };
    public ObservableCollection<string> ComboBoxCollectionOfRealEstateTypes
    {
        get => _comboBoxCollectionOfRealEstateTypes;
        set => this.RaiseAndSetIfChanged(ref _comboBoxCollectionOfRealEstateTypes, value);
    }

    #endregion

    #region Выбираемые переменные
    
    private string _selectedComboBoxItem = "Нет";
    public string SelectedComboBoxItem
    {
        get => _selectedComboBoxItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedComboBoxItem, value);
            if (value == "Нет")
            {
                IsRealEstateSupplyInformationSelected = false;
                IsRealEstateInformationSelected = false;
            }
            else if (value == "Информация")
            {
                IsRealEstateInformationSelected = true;
                IsRealEstateSupplyInformationSelected = false;
            }
            else if (value == "Предложения")
            {
                IsRealEstateInformationSelected = false;
                IsRealEstateSupplyInformationSelected = true;
            }
        }
    }

    private RealEstate _oldSelectedRealEstate = new RealEstate();
    public RealEstate OldSelectedRealEstate
    {
        get => _oldSelectedRealEstate;
        set => this.RaiseAndSetIfChanged(ref _oldSelectedRealEstate, value);
    }

    private int countOfSelections = 1;
    
    private RealEstate _selectedEstate;
    public RealEstate SelectedEstate
    {
        get => _selectedEstate;
        set
        {
            if (value == null)
            {
                this.RaiseAndSetIfChanged(ref _selectedEstate, value);
                return;
            }
            if (countOfSelections == 1)
            {
                this.RaiseAndSetIfChanged(ref _selectedEstate, value);
                countOfSelections++;
                OldSelectedRealEstate = GetSpecificRealEstate(SelectedEstate.Id);
                IsRealEstateSelected = true;
                SelectedComboBoxRealEstateType = value.Type;
                SelectedRealEstateSupply = GetSpecificSupply(value.Id);
                return;
            }
            if (IsCancellingHappening)
            {
                IsCancellingHappening = false;
                this.RaiseAndSetIfChanged(ref _selectedEstate, OldSelectedRealEstate);
                return;
            }
            OldSelectedRealEstate = GetSpecificRealEstate(SelectedEstate.Id);
            if (!IsTwoRealEstatesEven(OldSelectedRealEstate, SelectedEstate))
                IsEditionSaved = false;
            if (IsEditionSaved == false)
            { 
                Console.WriteLine("Вы не сохранили изменения. Либо сохраните, либо отмените их");
                return;
            }
            else
            {
                this.RaiseAndSetIfChanged(ref _selectedEstate, value);
                SelectedComboBoxRealEstateType = value.Type;
                SelectedRealEstateSupply = GetSpecificSupply(value.Id);
            }
        }
    }
    
    private string _selectedComboBoxRealEstateType;
    public string SelectedComboBoxRealEstateType
    {
        get => _selectedComboBoxRealEstateType;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedComboBoxRealEstateType, value);
            SelectedEstate.Type = value;
            if (value == "Земля")
                IsLandChosen = true;
            else
                IsLandChosen = false;
        }
    }

    private ObservableCollection<Supply> _selectedRealEstateSupply = new ObservableCollection<Supply>();
    public ObservableCollection<Supply> SelectedRealEstateSupply
    {
        get => _selectedRealEstateSupply;
        set => this.RaiseAndSetIfChanged(ref _selectedRealEstateSupply, value);
    }

    #endregion

    #region Контроль видимости

    private bool _isEditionSaved = true;
    public bool IsEditionSaved
    {
        get => _isEditionSaved;
        set => this.RaiseAndSetIfChanged(ref _isEditionSaved, value);
    }

    private bool _isCancellingHappening;
    public bool IsCancellingHappening
    {
        get => _isCancellingHappening;
        set => this.RaiseAndSetIfChanged(ref _isCancellingHappening, value);
    }
    
    private bool _isLandChosen;
    public bool IsLandChosen
    {
        get => _isLandChosen;
        set => this.RaiseAndSetIfChanged(ref _isLandChosen, value);
    }

    private bool _isRealEstateSelected = false;
    public bool IsRealEstateSelected
    {
        get => _isRealEstateSelected;
        set => this.RaiseAndSetIfChanged(ref _isRealEstateSelected, value);
    }

    private bool _isRealEstateSupplyInformationSelected = false;
    public bool IsRealEstateSupplyInformationSelected
    {
        get => _isRealEstateSupplyInformationSelected;
        set => this.RaiseAndSetIfChanged(ref _isRealEstateSupplyInformationSelected, value);
    }
    
    private bool _isRealEstateInformationSelected = false;
    public bool IsRealEstateInformationSelected
    {
        get => _isRealEstateInformationSelected;
        set => this.RaiseAndSetIfChanged(ref _isRealEstateInformationSelected, value);
    }

    #endregion
    
    #region Команды

    #region Команды изменения бд

    public ICommand SaveRealEstateCommand { get; }
    public ICommand DeleteRealEstateCommand { get; }
    public ICommand CancelRealEstateEditionCommand { get; }

    private void OnSaveRealEstateCommandExecuted(object parameter)
    {
        if (SelectedEstate.Coordinates.Latitude > 90 ||
            SelectedEstate.Coordinates.Latitude < -90 ||
            SelectedEstate.Coordinates.Longitude > 180 ||
            SelectedEstate.Coordinates.Longitude < -180)
        {
            Console.WriteLine("Широта или долгота приняли некорректное значение");
            return;
        }
        
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            
            if (OldSelectedRealEstate.Type != SelectedEstate.Type)
            {
                MySqlCommand typeCommand = new MySqlCommand();
                typeCommand.Connection = connection;
                
                string typeQuery = "", typeQuery1 = "";
                if (OldSelectedRealEstate.Type == "Квартира")
                {
                    typeQuery = "delete from apartment " +
                                "where RealEstateId = @id";
                }
                else if (OldSelectedRealEstate.Type == "Дом")
                {
                    typeQuery = "delete from house " +
                                "where RealEstateId = @id";
                }
                else if (OldSelectedRealEstate.Type == "Земля")
                {
                    typeQuery = "delete from land " +
                                "where RealEstateId = @id";
                }

                if (SelectedEstate.Type == "Квартира")
                {
                    typeQuery1 = "insert into apartment values" +
                                 "(@id, @floor, @rooms, @totalArea);";
                }
                else if (SelectedEstate.Type == "Дом")
                {
                    typeQuery1 = "insert into house values" +
                                 "(@id, @floor, @rooms, @totalArea);";
                }
                else if (SelectedEstate.Type == "Земля")
                {
                    typeQuery1 = "insert into land values" +
                                 "(@id, @totalArea);";
                }

                typeCommand.Parameters.AddWithValue("@id", SelectedEstate.Id);
                typeCommand.Parameters.AddWithValue("@floor", SelectedEstate.MoreInformation.Floor);
                typeCommand.Parameters.AddWithValue("@rooms", SelectedEstate.MoreInformation.Rooms);
                typeCommand.Parameters.AddWithValue("@totalArea", SelectedEstate.MoreInformation.TotalArea);

                typeCommand.CommandText = typeQuery;
                typeCommand.ExecuteNonQuery();

                typeCommand.CommandText = typeQuery1;
                typeCommand.ExecuteNonQuery();

            }
            
            string query1 = "update address set " +
                            "City = @newCity, " +
                            "Street = @newStreet, " +
                            "House = @newHouse, " +
                            "Apartment = @newApartment " +
                            "where RealEstateId = @id;";
            string query2 = "update coordinates set " +
                            "Latitude = @newLatitude, " +
                            "Longitude = @newLongitude " +
                            "where RealEstateId = @id;";
            string query3 = "";
            if (SelectedEstate.Type == "Квартира")
                query3 = "update apartment set " +
                         "Floor = @newFloor, " +
                         "Rooms = @newRooms, " +
                         "TotalArea = @newTotalArea " +
                         "where RealEstateId = @id";
            else if (SelectedEstate.Type == "Дом")
            {
                query3 = "update house set " +
                         "Floor = @newFloor, " +
                         "Rooms = @newRooms, " +
                         "TotalArea = @newTotalArea " +
                         "where RealEstateId = @id";
            }
            else if (SelectedEstate.Type == "Земля")
            {
                query3 = "update land set " +
                         "TotalArea = @newTotalArea " +
                         "where RealEstateId = @id";
            }

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query1;

            cmd.Parameters.AddWithValue("@id", SelectedEstate.Id);
            cmd.Parameters.AddWithValue("@newCity", SelectedEstate.Address.City);
            cmd.Parameters.AddWithValue("@newStreet", SelectedEstate.Address.Street);
            cmd.Parameters.AddWithValue("@newHouse", SelectedEstate.Address.House);
            cmd.Parameters.AddWithValue("@newApartment", SelectedEstate.Address.Apartment);
            cmd.Parameters.AddWithValue("@newLatitude", SelectedEstate.Coordinates.Latitude);
            cmd.Parameters.AddWithValue("@newLongitude", SelectedEstate.Coordinates.Longitude);
            cmd.Parameters.AddWithValue("@newFloor", SelectedEstate.MoreInformation.Floor);
            cmd.Parameters.AddWithValue("@newRooms", SelectedEstate.MoreInformation.Rooms);
            cmd.Parameters.AddWithValue("@newTotalArea", SelectedEstate.MoreInformation.TotalArea);

            cmd.ExecuteNonQuery();

            cmd.CommandText = query2;
            cmd.ExecuteNonQuery();

            cmd.CommandText = query3;
            cmd.ExecuteNonQuery();
            
            OldSelectedRealEstate = GetSpecificRealEstate(SelectedEstate.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            connection.Dispose();
            connection.Close();
        }
    }
    private void OnDeleteRealEstateCommandExecuted(object parameter)
    {
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query1 = "select * from supply " +
                            "where RealEstateId = @id;";
            string query2 = "delete from realEstate " +
                            "where id = @id;";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query1;
            cmd.Parameters.AddWithValue("@id", SelectedEstate.Id);

            var reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Console.WriteLine($"Невозможно удалить объект недвижимости, " +
                                      $"участвующий в предложении №{reader.GetInt32(0)}");
                    return;
                }
            }
            reader.Close();

            cmd.CommandText = query2;
            cmd.ExecuteNonQuery();
            
            SelectedEstate = null;
            IsRealEstateSelected = false;
            SelectedComboBoxItem = "Нет";
            GetAllRealEstates();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            connection.Dispose();
            connection.Close();
        }
    }
    private void OnCancelRealEstateEditionCommandExecuted(object parameter = null)
    {
        IsCancellingHappening = true;
        SelectedEstate = OldSelectedRealEstate;
        IsEditionSaved = true;
        IsRealEstateSelected = false;
        SelectedComboBoxItem = "Нет";
        GetAllRealEstates();
        countOfSelections = 1;
    }

    #endregion

    #endregion

    private void GetAllRealEstates()
    {
        RealEstates.Clear();
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT realEstate.Id, address.City, address.Street, address.House, " +
                           "address.Apartment, coordinates.Latitude, coordinates.Longitude " +
                           "FROM `realEstate` " +
                           "inner join address on realEstate.id = address.RealEstateId " +
                           "INNER JOIN coordinates on realEstate.id = coordinates.RealEstateId; ";

            string checkQuery1 = "Select * from apartment where RealEstateId = @id";
            string checkQuery2 = "Select * from house where RealEstateId = @id";
            string checkQuery3 = "Select * from land where RealEstateId = @id";
            

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;

            var reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    RealEstate realEstate = new RealEstate()
                    {
                        Id = reader.GetInt32(0),
                        Address = new Address()
                        {
                            City = reader.IsDBNull(1) ? "Нет" : reader.GetString(1),
                            Street = reader.IsDBNull(2) ? "Нет" : reader.GetString(2),
                            House = reader.IsDBNull(3) ? "Нет" : reader.GetString(3),
                            Apartment = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                        },
                        Coordinates = new Coordinates()
                        {
                            Latitude = reader.IsDBNull(5) ? 0 : reader.GetFloat(5),
                            Longitude = reader.IsDBNull(6) ? 0 : reader.GetFloat(6)
                        },
                    };
                    RealEstates.Add(realEstate);
                }
            }
            reader.Close();

            foreach (var realEstate in RealEstates)
            {
                MySqlCommand checkComand = new MySqlCommand();
                checkComand.Connection = connection;
                checkComand.CommandText = checkQuery1;

                int id = realEstate.Id;
                RealEstateMoreInformation moreInformation = new RealEstateMoreInformation(); 
                string type = "";
                
                checkComand.Parameters.AddWithValue("@id", id); 
                var readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read())
                    {
                        moreInformation = new RealEstateMoreInformation()
                        {
                            Floor = reader.IsDBNull(1) ? 0 : readerInside.GetInt32(1),
                            Rooms = readerInside.IsDBNull(2) ? 0 : readerInside.GetInt32(2),
                            TotalArea = reader.IsDBNull(3) ? 0 : readerInside.GetFloat(3)

                        };
                        type = "Квартира";
                    }
                } 
                readerInside.Close();
                
                checkComand.CommandText = checkQuery2; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read()) 
                    { 
                        moreInformation = new RealEstateMoreInformation() 
                        { 
                            Floor = reader.IsDBNull(1) ? 0 : readerInside.GetInt32(1), 
                            Rooms = readerInside.IsDBNull(2) ? 0 : readerInside.GetInt32(2), 
                            TotalArea = reader.IsDBNull(3) ? 0 : readerInside.GetFloat(3)
                        }; 
                        type = "Дом";
                    }
                }
                readerInside.Close();
                
                checkComand.CommandText = checkQuery3; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read()) 
                    { 
                        moreInformation = new RealEstateMoreInformation() 
                        { 
                            TotalArea = reader.IsDBNull(1) ? 0 : readerInside.GetFloat(1)
                        };
                        type = "Земля";
                    }
                } 
                readerInside.Close();

                realEstate.MoreInformation = moreInformation;
                realEstate.Type = type;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            connection.Dispose();
            connection.Close();
            OnPropertyChanged(nameof(RealEstates));
        }
    }
    
    private RealEstate GetSpecificRealEstate(int realEstateId)
    {
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT realEstate.id, address.City, address.Street, address.House, " +
                           "address.Apartment, coordinates.Latitude, coordinates.Longitude " +
                           "FROM `realEstate` " +
                           "inner join address on realEstate.id = address.RealEstateId " +
                           "INNER JOIN coordinates on realEstate.id = coordinates.RealEstateId " +
                           "where realEstate.id = @realEstateId;";

            string checkQuery1 = "Select * from apartment where RealEstateId = @id";
            string checkQuery2 = "Select * from house where RealEstateId = @id";
            string checkQuery3 = "Select * from land where RealEstateId = @id";
            

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@realEstateId", realEstateId);

            var reader = cmd.ExecuteReader();

            RealEstate realEstate = new RealEstate();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    realEstate = new RealEstate()
                    {
                        Id = reader.GetInt32(0),
                        Address = new Address()
                        {
                            City = reader.IsDBNull(1) ? "Нет" : reader.GetString(1),
                            Street = reader.IsDBNull(2) ? "Нет" : reader.GetString(2),
                            House = reader.IsDBNull(3) ? "Нет" : reader.GetString(3),
                            Apartment = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                        },
                        Coordinates = new Coordinates()
                        {
                            Latitude = reader.IsDBNull(5) ? 0 : reader.GetFloat(5),
                            Longitude = reader.IsDBNull(6) ? 0 : reader.GetFloat(6)
                        },
                    };
                }
            }
            reader.Close();

            MySqlCommand checkComand = new MySqlCommand();
                checkComand.Connection = connection;
                checkComand.CommandText = checkQuery1;
                
                RealEstateMoreInformation moreInformation = new RealEstateMoreInformation(); 
                string type = "";
                
                checkComand.Parameters.AddWithValue("@id", realEstateId); 
                var readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read())
                    {
                        moreInformation = new RealEstateMoreInformation()
                        {
                            Floor = reader.IsDBNull(1) ? 0 : readerInside.GetInt32(1),
                            Rooms = readerInside.IsDBNull(2) ? 0 : readerInside.GetInt32(2),
                            TotalArea = reader.IsDBNull(3) ? 0 : readerInside.GetFloat(3)

                        };
                        type = "Квартира";
                    }
                } 
                readerInside.Close();
                
                checkComand.CommandText = checkQuery2; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read()) 
                    { 
                        moreInformation = new RealEstateMoreInformation() 
                        { 
                            Floor = reader.IsDBNull(1) ? 0 : readerInside.GetInt32(1), 
                            Rooms = readerInside.IsDBNull(2) ? 0 : readerInside.GetInt32(2), 
                            TotalArea = reader.IsDBNull(3) ? 0 : readerInside.GetFloat(3)
                        }; 
                        type = "Дом";
                    }
                }
                readerInside.Close();
                
                checkComand.CommandText = checkQuery3; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read()) 
                    { 
                        moreInformation = new RealEstateMoreInformation() 
                        { 
                            TotalArea = reader.IsDBNull(1) ? 0 : readerInside.GetFloat(1)
                        };
                        type = "Земля";
                    }
                } 
                readerInside.Close();

                realEstate.MoreInformation = moreInformation;
                realEstate.Type = type;

                return realEstate;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
        finally
        {
            connection.Dispose();
            connection.Close();
            OnPropertyChanged(nameof(RealEstates));
        }
    }

    private ObservableCollection<Supply> GetSpecificSupply(int id)
    {
        MySqlConnection connection = DBUtils.GetDBConnection();

        ObservableCollection<Supply> supplies = new ObservableCollection<Supply>();

        try
        {
            connection.Open();
            string query = "SELECT * from supply where supply.RealEstateId = @realEstateId;";

            string query1 = "Select * from client where id = @clientId;";
            string query2 = "Select * from realtor where id = @realtorId;";
            string query3 = "Select * from address where RealEstateId = @realEstateId;";
            
            string checkQuery1 = "Select * from apartment where RealEstateId = @id";
            string checkQuery2 = "Select * from house where RealEstateId = @id";
            string checkQuery3 = "Select * from land where RealEstateId = @id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@realEstateId", id);

            var reader = cmd.ExecuteReader();
            
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var supply = new Supply()
                    {
                        Id = reader.GetInt32(0),
                        Cost = reader.GetInt32(1),
                        Client = new Client()
                        {
                            Id = reader.GetInt32(2)
                        },
                        Realtor = new Realtor()
                        {
                            Id = reader.GetInt32(3)
                        },
                        RealEstate = new RealEstate()
                        {
                            Id = reader.GetInt32(4),
                            Type = ""
                        }
                    };
                    supplies.Add(supply);
                }
            }
            reader.Close();

            foreach (var supply in supplies)
            {
                MySqlCommand checkComand = new MySqlCommand();
                checkComand.Connection = connection;
                checkComand.CommandText = query1;
                
                checkComand.Parameters.AddWithValue("@clientId", supply.Client.Id); 
                checkComand.Parameters.AddWithValue("@realtorId", supply.Realtor.Id); 
                checkComand.Parameters.AddWithValue("@realEstateId", supply.RealEstate.Id); 
                
                var readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read())
                    {
                        supply.Client.FirstName = readerInside.IsDBNull(1) ? "Нет" : readerInside.GetString(1);
                        supply.Client.SecondName = readerInside.IsDBNull(2) ? "Нет" : readerInside.GetString(2);
                        supply.Client.LastName = readerInside.IsDBNull(3) ? "Нет" :readerInside.GetString(3);
                        supply.Client.Phone = readerInside.IsDBNull(4) ? "Нет" :readerInside.GetString(4);
                        supply.Client.Email = readerInside.IsDBNull(5) ? "Нет" : reader.GetString(5);
                    }
                } 
                readerInside.Close();
                
                checkComand.CommandText = query2; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read()) 
                    { 
                        supply.Realtor.FirstName = readerInside.IsDBNull(1) ? "Нет" : readerInside.GetString(1);
                        supply.Realtor.SecondName = readerInside.IsDBNull(2) ? "Нет" : readerInside.GetString(2);
                        supply.Realtor.LastName = readerInside.IsDBNull(3) ? "Нет" :readerInside.GetString(3);
                        supply.Realtor.Share = readerInside.IsDBNull(4) ? 0 :readerInside.GetInt32(4);
                    }
                }
                readerInside.Close();
                
                checkComand.CommandText = query3; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                {
                    while (readerInside.Read())
                    {
                        supply.RealEstate.Address = new Address();
                        supply.RealEstate.Address.City = readerInside.IsDBNull(1) ? "Нет" : readerInside.GetString(1);
                        supply.RealEstate.Address.Street = readerInside.IsDBNull(2) ? "Нет" : readerInside.GetString(2);
                        supply.RealEstate.Address.House = readerInside.IsDBNull(3) ? "Нет" : readerInside.GetString(3);
                        supply.RealEstate.Address.Apartment = readerInside.IsDBNull(4) ? 0 : readerInside.GetInt32(4);
                    }
                } 
                readerInside.Close();
                
                
                checkComand.Parameters.AddWithValue("@id", supply.RealEstate.Id); 
                
                checkComand.CommandText = checkQuery1;
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows)
                {
                    supply.RealEstate.Type = "Квартира";
                } 
                readerInside.Close();
                
                checkComand.CommandText = checkQuery2; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                {
                    supply.RealEstate.Type = "Дом";
                }
                readerInside.Close();
                
                checkComand.CommandText = checkQuery3; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    supply.RealEstate.Type = "Земля";
                } 
                readerInside.Close();
            }

            return supplies;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
        finally
        {
            connection.Dispose();
            connection.Close();
        }
    }
    
    private bool IsTwoRealEstatesEven(RealEstate realEstate1, RealEstate realEstate2)
    {
        if (realEstate1.Address.City != realEstate2.Address.City ||
            realEstate1.Address.Street != realEstate2.Address.Street ||
            realEstate1.Address.House != realEstate2.Address.House ||
            realEstate1.Address.Apartment != realEstate2.Address.Apartment ||
            realEstate1.Id != realEstate2.Id || realEstate1.Type != realEstate2.Type ||
            realEstate1.Coordinates.Longitude != realEstate2.Coordinates.Longitude ||
            realEstate1.Coordinates.Latitude != realEstate2.Coordinates.Latitude ||
            realEstate1.MoreInformation.Floor != realEstate2.MoreInformation.Floor ||
            realEstate1.MoreInformation.Rooms != realEstate2.MoreInformation.Rooms ||
            realEstate1.MoreInformation.TotalArea != realEstate2.MoreInformation.TotalArea)
            return false;
        return true;
    }
}