using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows.Input;
using DemoApplication.Infrastructure.Commands;
using DemoApplication.Infrastructure.DB;
using DemoApplication.Infrastructure.Stores;
using DemoApplication.Models;
using MySqlConnector;
using ReactiveUI;

namespace DemoApplication.ViewModels.PageViewModels;

public class DemandsViewModel : ViewModelBase
{
    public DemandsViewModel()
    {
        GetAllDemands();
        GetAllClients();
        GetAllRealtors();

        #region Команды

        SaveDemandCommand = new LambdaCommand(OnSaveDemandCommandExecuted);
        DeleteDemandCommand = new LambdaCommand(OnDeleteDemandCommandExecuted);
        CancelDemandEditionCommand = new LambdaCommand(OnCancelDemandEditionCommandExecuted);

        #endregion
    }

    #region Коллекции

    private ObservableCollection<Demand> _demands = new ObservableCollection<Demand>();
    public ObservableCollection<Demand> Demands
    {
        get => _demands;
        set => this.RaiseAndSetIfChanged(ref _demands, value);
    }

    private ObservableCollection<Client> _clients = new ObservableCollection<Client>();
    public ObservableCollection<Client> Clients
    {
        get => _clients;
        set => this.RaiseAndSetIfChanged(ref _clients, value);
    }

    private ObservableCollection<string> _comboBoxCollectionOfClients = new ObservableCollection<string>();
    public ObservableCollection<string> ComboBoxCollectionOfClients
    {
        get => _comboBoxCollectionOfClients;
        set => this.RaiseAndSetIfChanged(ref _comboBoxCollectionOfClients, value);
    }

    private ObservableCollection<Realtor> _realtors = new ObservableCollection<Realtor>();
    public ObservableCollection<Realtor> Realtors
    {
        get => _realtors;
        set => this.RaiseAndSetIfChanged(ref _realtors, value);
    }
    
    private ObservableCollection<string> _comboBoxCollectionOfRealtors = new ObservableCollection<string>();
    public ObservableCollection<string> ComboBoxCollectionOfRealtors
    {
        get => _comboBoxCollectionOfRealtors;
        set => this.RaiseAndSetIfChanged(ref _comboBoxCollectionOfRealtors, value);
    }

    private ObservableCollection<string> _comboBoxCollection = new ObservableCollection<string>()
    {
        "Нет", "Сделки", "Информация"
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

    private ObservableCollection<Deal> _relatedDeals = new ObservableCollection<Deal>();
    public ObservableCollection<Deal> RelatedDeals
    {
        get => _relatedDeals;
        set => this.RaiseAndSetIfChanged(ref _relatedDeals, value);
    }

    #endregion

    #region Выбираемые переменные

    private Client _selectedClient;
    public Client SelectedClient
    {
        get => _selectedClient;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedClient, value);
            SelectedDemand.Client = value;
        }
    }

    private string _selectedComboBoxClient;
    public string SelectedComboBoxClient
    {
        get => _selectedComboBoxClient;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedComboBoxClient, value);
            if (value != null && value != "")
                SelectedClient = Clients.First(x => x.Id.ToString() == value.Split(' ')[0] &&
                                                    x.FirstName == value.Split(' ')[1] &&
                                                    x.SecondName == value.Split(' ')[2] &&
                                                    x.LastName == value.Split(' ')[3]);
        }
    }

    private Realtor _selectedRealtor;
    public Realtor SelectedRealtor
    {
        get => _selectedRealtor;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedRealtor, value);
            SelectedDemand.Realtor = value;
        }
    }

    private string _selectedComboBoxRealtor;
    public string SelectedComboBoxRealtor
    {
        get => _selectedComboBoxRealtor;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedComboBoxRealtor, value);
            if (value != null && value != "")
                SelectedRealtor = Realtors.First(x => x.Id.ToString() == value.Split(' ')[0] &&
                                                    x.FirstName == value.Split(' ')[1] &&
                                                    x.SecondName == value.Split(' ')[2] &&
                                                    x.LastName == value.Split(' ')[3]);
        }
    }
    
    private Demand _oldSelectedDemand = new Demand();
    public Demand OldSelectedDemand
    {
        get => _oldSelectedDemand;
        set => this.RaiseAndSetIfChanged(ref _oldSelectedDemand, value);
    }
    
    private int _countOfSelections = 1;

    private Demand _selectedDemand;
    public Demand SelectedDemand
    {
        get => _selectedDemand;
        set
        {
            if (value == null)
            {
                this.RaiseAndSetIfChanged(ref _selectedDemand, value);
                return;
            }
            if (_countOfSelections == 1)
            {
                _countOfSelections++;
                this.RaiseAndSetIfChanged(ref _selectedDemand, value);
                OldSelectedDemand = GetSpecificDemand(_selectedDemand.Id);
                IsDemandSelected = true;
                SelectedComboBoxClient = $"{value.Client.Id} {value.Client.FirstName} " +
                                         $"{value.Client.SecondName} {value.Client.LastName}";
                SelectedComboBoxRealtor = $"{value.Realtor.Id} {value.Realtor.FirstName} " +
                                          $"{value.Realtor.SecondName} {value.Realtor.LastName}";

                RelatedDeals = GetRelatedDeals(value.Id);

                SelectedComboBoxRealEstateType = value.RealEstateType;
                
                return;
            }
            if (IsCancellingHappening)
            {
                IsDemandSelected = false;
                SelectedComboBoxItem = "Нет";
                SelectedComboBoxClient = "";
                SelectedComboBoxRealtor = "";
                this.RaiseAndSetIfChanged(ref _selectedDemand, OldSelectedDemand);
                IsCancellingHappening = false;
                return;
            }
            OldSelectedDemand = GetSpecificDemand(SelectedDemand.Id);
            if (!IsTwoDemandsEven(OldSelectedDemand, SelectedDemand))
                IsEditionSaved = false;
            if (IsEditionSaved == false)
            { 
                Console.WriteLine("Вы не сохранили изменения. Либо сохраните, либо отмените их");
            }
            else
            {
                this.RaiseAndSetIfChanged(ref _selectedDemand, value);
                OldSelectedDemand = GetSpecificDemand(SelectedDemand.Id);
                IsDemandSelected = true;
                SelectedComboBoxClient = $"{value.Client.Id} {value.Client.FirstName} " +
                                         $"{value.Client.SecondName} {value.Client.LastName}";
                SelectedComboBoxRealtor = $"{value.Realtor.Id} {value.Realtor.FirstName} " +
                                          $"{value.Realtor.SecondName} {value.Realtor.LastName}";

                RelatedDeals = GetRelatedDeals(value.Id);
                
                SelectedComboBoxRealEstateType = value.RealEstateType;
            }
        }
    }

    private string _selectedComboBoxItem = "Нет";
    public string SelectedComboBoxItem
    {
        get => _selectedComboBoxItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedComboBoxItem, value);
            if (value == "Нет")
            {
                IsUserEditingVisible = false;
                IsRelatedDealsVisible = false;
            }
            else if (value == "Информация")
            {
                IsUserEditingVisible = true;
                IsRelatedDealsVisible = false;
            }
            else if (value == "Сделки")
            {
                IsUserEditingVisible = false;
                IsRelatedDealsVisible = true;
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
            SelectedDemand.RealEstateType = value;
            if (value == "Земля")
                IsLandChosen = true;
            else
                IsLandChosen = false;
        }
    }

    #endregion

    #region Контроль видимости и доступности

    private bool _isDemandSelected = false;
    public bool IsDemandSelected
    {
        get => _isDemandSelected;
        set => this.RaiseAndSetIfChanged(ref _isDemandSelected, value);
    }

    private bool _isUserEditingVisible = false;
    public bool IsUserEditingVisible
    {
        get => _isUserEditingVisible;
        set => this.RaiseAndSetIfChanged(ref _isUserEditingVisible, value);
    }
    
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

    private bool _isRelatedDealsVisible = false;
    public bool IsRelatedDealsVisible
    {
        get => _isRelatedDealsVisible;
        set => this.RaiseAndSetIfChanged(ref _isRelatedDealsVisible, value);
    }

    private bool _isLandChosen;
    public bool IsLandChosen
    {
        get => _isLandChosen;
        set => this.RaiseAndSetIfChanged(ref _isLandChosen, value);
    }

    #endregion
    
    #region Команды

    #region Команды изменения бд

    public ICommand SaveDemandCommand { get; }
    public ICommand DeleteDemandCommand { get; }
    public ICommand CancelDemandEditionCommand { get; }

    private void OnSaveDemandCommandExecuted(object parameter)
    {
        if ((SelectedDemand.MinCost != 0 && SelectedDemand.MaxCost != 0) && 
            SelectedDemand.MinCost > SelectedDemand.MaxCost)
        {
            Console.WriteLine("Указан некорректный разброс цен");
            return;
        }
        if ((SelectedDemand.MoreInformation.MinFloor != 0 &&
             SelectedDemand.MoreInformation.MaxFloor != 0) &&
            SelectedDemand.MoreInformation.MinFloor > SelectedDemand.MoreInformation.MaxFloor)
        {
            Console.WriteLine("Указан некорректный разброс этажей");
            return;
        }
        if ((SelectedDemand.MoreInformation.MinRooms != 0 &&
             SelectedDemand.MoreInformation.MaxRooms != 0) &&
            SelectedDemand.MoreInformation.MinRooms > SelectedDemand.MoreInformation.MaxRooms)
        {
            Console.WriteLine("Указан некорректный разброс количества комнат");
            return;
        }
        if ((SelectedDemand.MoreInformation.MinArea != 0 &&
             SelectedDemand.MoreInformation.MaxArea != 0) &&
            SelectedDemand.MoreInformation.MinArea > SelectedDemand.MoreInformation.MaxArea)
        {
            Console.WriteLine("Указан некорректный разброс площади");
            return;
        }
        
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query1 = "update demand set " +
                            "RealEstateType = @realEstateType, " +
                            "MinCost = @minCost, " +
                            "MaxCost = @maxCost, " +
                            "Address = @address, " +
                            "ClientId = @clientId, " +
                            "RealtorId = @realtorId " +
                            "where id = @id";
            string query2 = "";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query1;

            if (OldSelectedDemand.RealEstateType != SelectedDemand.RealEstateType)
            {
                MySqlCommand typeCommand = new MySqlCommand();
                typeCommand.Connection = connection;
                
                string typeQuery = "", typeQuery1 = "";
                if (OldSelectedDemand.RealEstateType == "Квартира")
                {
                    typeQuery = "delete from apartmentDemand " +
                                "where DemandId = @id";
                }
                else if (OldSelectedDemand.RealEstateType == "Дом")
                {
                    typeQuery = "delete from houseDemand " +
                                "where DemandId = @id";
                }
                else if (OldSelectedDemand.RealEstateType == "Земля")
                {
                    typeQuery = "delete from landDemand " +
                                "where DemandId = @id";
                }

                if (SelectedDemand.RealEstateType == "Квартира")
                {
                    typeQuery1 = "insert into apartmentDemand values" +
                                 "(@id, @minArea, @maxArea, @minRooms, " +
                                 "@maxRooms, @minFloor, @maxFloor);";
                }
                else if (SelectedDemand.RealEstateType == "Дом")
                {
                    typeQuery1 = "insert into houseDemand values" +
                                 "(@id, @minArea, @maxArea, @minRooms, " +
                                 "@maxRooms, @minFloor, @maxFloor);";
                }
                else if (SelectedDemand.RealEstateType == "Земля")
                {
                    typeQuery1 = "insert into landDemand values" +
                                 "(@id, @minArea, @maxArea);";
                }

                typeCommand.Parameters.AddWithValue("@id", SelectedDemand.Id);
                typeCommand.Parameters.AddWithValue("@minArea", SelectedDemand.MoreInformation.MinArea);
                typeCommand.Parameters.AddWithValue("@maxArea", SelectedDemand.MoreInformation.MaxArea);
                typeCommand.Parameters.AddWithValue("@minRooms", SelectedDemand.MoreInformation.MinRooms);
                typeCommand.Parameters.AddWithValue("@maxRooms", SelectedDemand.MoreInformation.MaxRooms);
                typeCommand.Parameters.AddWithValue("@minFloor", SelectedDemand.MoreInformation.MinFloor);
                typeCommand.Parameters.AddWithValue("@maxFloor", SelectedDemand.MoreInformation.MaxFloor);

                typeCommand.CommandText = typeQuery;
                typeCommand.ExecuteNonQuery();

                typeCommand.CommandText = typeQuery1;
                typeCommand.ExecuteNonQuery();

            }
            
            if (SelectedDemand.RealEstateType == "Квартира")
            {
                cmd.Parameters.AddWithValue("@realEstateType", 1);
                query2 = "update apartmentDemand set " +
                         "MinArea = @minArea, " +
                         "MaxArea = @maxArea, " +
                         "MinRooms = @minRooms, " +
                         "MaxRooms = @maxRooms, " +
                         "MinFloor = @minFloor, " +
                         "MaxFloor = @maxFloor " +
                         "where DemandId = @id;";

                cmd.Parameters.AddWithValue("@minArea", SelectedDemand.MoreInformation.MinArea);
                cmd.Parameters.AddWithValue("@maxArea", SelectedDemand.MoreInformation.MaxArea);
                cmd.Parameters.AddWithValue("@minRooms", SelectedDemand.MoreInformation.MinRooms);
                cmd.Parameters.AddWithValue("@maxRooms", SelectedDemand.MoreInformation.MaxRooms);
                cmd.Parameters.AddWithValue("@minFloor", SelectedDemand.MoreInformation.MinFloor);
                cmd.Parameters.AddWithValue("@maxFloor", SelectedDemand.MoreInformation.MaxFloor);
            }
            else if (SelectedDemand.RealEstateType == "Дом")
            {
                cmd.Parameters.AddWithValue("@realEstateType", 2);
                query2 = "update houseDemand set " +
                         "MinArea = @minArea, " +
                         "MaxArea = @maxArea, " +
                         "MinRooms = @minRooms, " +
                         "MaxRooms = @maxRooms, " +
                         "MinFloor = @minFloor, " +
                         "MaxFloor = @maxFloor " +
                         "where DemandId = @id;";
                cmd.Parameters.AddWithValue("@minArea", SelectedDemand.MoreInformation.MinArea);
                cmd.Parameters.AddWithValue("@maxArea", SelectedDemand.MoreInformation.MaxArea);
                cmd.Parameters.AddWithValue("@minRooms", SelectedDemand.MoreInformation.MinRooms);
                cmd.Parameters.AddWithValue("@maxRooms", SelectedDemand.MoreInformation.MaxRooms);
                cmd.Parameters.AddWithValue("@minFloor", SelectedDemand.MoreInformation.MinFloor);
                cmd.Parameters.AddWithValue("@maxFloor", SelectedDemand.MoreInformation.MaxFloor);
            }
            else if (SelectedDemand.RealEstateType == "Земля")
            {
                cmd.Parameters.AddWithValue("@realEstateType", 3);
                query2 = "update landDemand set " +
                         "MinArea = @minArea, " +
                         "MaxArea = @maxArea " +
                         "where DemandId = @id;";
                cmd.Parameters.AddWithValue("@minArea", SelectedDemand.MoreInformation.MinArea);
                cmd.Parameters.AddWithValue("@maxArea", SelectedDemand.MoreInformation.MaxArea);
            }

            cmd.Parameters.AddWithValue("@id", SelectedDemand.Id);
            cmd.Parameters.AddWithValue("@minCost", SelectedDemand.MinCost);
            cmd.Parameters.AddWithValue("@maxCost", SelectedDemand.MaxCost);
            cmd.Parameters.AddWithValue("@address", $"{SelectedDemand.Address.City}," +
                                                    $"{SelectedDemand.Address.Street}," +
                                                    $"{SelectedDemand.Address.House}," +
                                                    $"{SelectedDemand.Address.Apartment}");
            cmd.Parameters.AddWithValue("@clientId", SelectedDemand.Client.Id);
            cmd.Parameters.AddWithValue("@realtorId", SelectedDemand.Realtor.Id);

            cmd.ExecuteNonQuery();

            cmd.CommandText = query2;
            cmd.ExecuteNonQuery();
            
            IsEditionSaved = true;
            OldSelectedDemand = GetSpecificDemand(SelectedDemand.Id);
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
    private void OnDeleteDemandCommandExecuted(object parameter)
    {
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query1 = "select * from deal " +
                            "where DemandId = @id;";
            string query2 = "delete from demand " +
                            "where id = @id;";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query1;
            cmd.Parameters.AddWithValue("@id", SelectedDemand.Id);

            var reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Console.WriteLine($"Невозможно удалить потребность, " +
                                      $"участвующую в сделке №{reader.GetInt32(0)}");
                    return;
                }
            }
            reader.Close();

            cmd.CommandText = query2;
            cmd.ExecuteNonQuery();
            SelectedDemand = null;
            IsDemandSelected = false;
            SelectedComboBoxItem = "Нет";
            SelectedComboBoxClient = "";
            SelectedComboBoxRealtor = "";
            GetAllDemands();
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
    private void OnCancelDemandEditionCommandExecuted(object parameter = null)
    {
        IsCancellingHappening = true;
        SelectedDemand = OldSelectedDemand;
        IsEditionSaved = true;
        GetAllDemands();
        _countOfSelections = 1;
    }

    #endregion

    #endregion
    
    private void GetAllDemands()
    {
        Demands.Clear();
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT * from demand;";

            string query1 = "Select * from client where id = @clientId;";
            string query2 = "Select * from realtor where id = @realtorId;";
            
            string checkQuery1 = "Select * from apartmentDemand where DemandId = @id";
            string checkQuery2 = "Select * from houseDemand where DemandId = @id";
            string checkQuery3 = "Select * from landDemand where DemandId = @id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;

            var reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    string type = "";
                    if (reader.GetInt32(1) == 1) type = "Квартира";
                    else if (reader.GetInt32(1) == 2) type = "Дом";
                    else type = "Земля";
                    Demand demand = new Demand()
                    {
                        Id = reader.GetInt32(0),
                        RealEstateType = type,
                        MinCost = reader.GetInt32(2),
                        MaxCost = reader.GetInt32(3),
                        Client = new Client()
                        {
                            Id = reader.GetInt32(5)
                        },
                        Realtor = new Realtor()
                        {
                            Id = reader.GetInt32(6)
                        }
                    };
                    string[] address = reader.GetString(4).Split(',');
                    demand.Address = new Address()
                    {
                        City = address[0] == "" ? "Нет" : address[0],
                        Street = address[1] == "" ? "Нет" : address[1],
                        House = address[2] == "" ? "Нет" : address[2],
                        Apartment = address[3] == "" ? 0 : int.Parse(address[3])
                    };
                    Demands.Add(demand);
                }
            }
            reader.Close();

            foreach (var demand in Demands)
            {
                MySqlCommand checkComand = new MySqlCommand();
                checkComand.Connection = connection;
                checkComand.CommandText = query1;
                
                checkComand.Parameters.AddWithValue("@clientId", demand.Client.Id); 
                checkComand.Parameters.AddWithValue("@realtorId", demand.Realtor.Id); 
                
                var readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read())
                    {
                        demand.Client.FirstName = readerInside.IsDBNull(1) ? "Нет" : readerInside.GetString(1);
                        demand.Client.SecondName = readerInside.IsDBNull(2) ? "Нет" : readerInside.GetString(2);
                        demand.Client.LastName = readerInside.IsDBNull(3) ? "Нет" :readerInside.GetString(3);
                        demand.Client.Phone = readerInside.IsDBNull(4) ? "Нет" :readerInside.GetString(4);
                        demand.Client.Email = readerInside.IsDBNull(5) ? "Нет" : reader.GetString(5);
                    }
                } 
                readerInside.Close();
                
                checkComand.CommandText = query2; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read()) 
                    { 
                        demand.Realtor.FirstName = readerInside.IsDBNull(1) ? "Нет" : readerInside.GetString(1);
                        demand.Realtor.SecondName = readerInside.IsDBNull(2) ? "Нет" : readerInside.GetString(2);
                        demand.Realtor.LastName = readerInside.IsDBNull(3) ? "Нет" :readerInside.GetString(3);
                        demand.Realtor.Share = readerInside.IsDBNull(4) ? 0 :readerInside.GetInt32(4);
                    }
                }
                readerInside.Close();
                
                checkComand.Parameters.AddWithValue("@id", demand.Id);

                DemandMoreInformation moreInformation = new DemandMoreInformation();
                
                checkComand.CommandText = checkQuery1;
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows)
                {
                    while (readerInside.Read())
                    {
                        moreInformation = new DemandMoreInformation()
                        {
                            MinArea = readerInside.GetFloat(1),
                            MaxArea = readerInside.GetFloat(2),
                            MinRooms = readerInside.GetInt32(3),
                            MaxRooms = readerInside.GetInt32(4),
                            MinFloor = readerInside.GetInt32(5),
                            MaxFloor = readerInside.GetInt32(6)
                        };
                    }
                } 
                readerInside.Close();
                
                checkComand.CommandText = checkQuery2; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                {
                    while (readerInside.Read())
                    {
                        moreInformation = new DemandMoreInformation()
                        {
                            MinArea = readerInside.GetFloat(1),
                            MaxArea = readerInside.GetFloat(2),
                            MinRooms = readerInside.GetInt32(3),
                            MaxRooms = readerInside.GetInt32(4),
                            MinFloor = readerInside.GetInt32(5),
                            MaxFloor = readerInside.GetInt32(6)
                        };
                    }                
                }
                readerInside.Close();
                
                checkComand.CommandText = checkQuery3; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read())
                    {
                        moreInformation = new DemandMoreInformation()
                        {
                            MinArea = readerInside.GetFloat(1),
                            MaxArea = readerInside.GetFloat(2),
                        };
                    }
                } 
                readerInside.Close();

                demand.MoreInformation = moreInformation;
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
            OnPropertyChanged(nameof(Demands));
        }
    }

    private void GetAllClients()
    {
        ComboBoxCollectionOfClients.Clear();
        Clients.Clear();
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "select * from client;";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;

            var reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Client client = new Client()
                    {
                        Id = reader.GetInt32(0),
                        FirstName = reader.IsDBNull(1) ? "Нет" : reader.GetString(1),
                        SecondName = reader.IsDBNull(2) ? "Нет" : reader.GetString(2),
                        LastName = reader.IsDBNull(3) ? "Нет" : reader.GetString(3),
                        Phone = reader.IsDBNull(4) ? "Нет" : reader.GetString(4),
                        Email = reader.IsDBNull(5) ? "Нет" : reader.GetString(5)
                    };
                    Clients.Add(client);

                    string stringForComboBoxCollection = $"{client.Id} {client.FirstName} {client.SecondName} " +
                                                         $"{client.LastName}";
                    ComboBoxCollectionOfClients.Add(stringForComboBoxCollection);
                }
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
            OnPropertyChanged(nameof(Clients));
        }
    }
    
    private void GetAllRealtors()
    {
        ComboBoxCollectionOfRealtors.Clear();
        Realtors.Clear();
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "select * from realtor;";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;

            var reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Realtor realtor = new Realtor()
                    {
                        Id = reader.GetInt32(0),
                        FirstName = reader.IsDBNull(1) ? "Нет" : reader.GetString(1),
                        SecondName = reader.IsDBNull(2) ? "Нет" : reader.GetString(2),
                        LastName = reader.IsDBNull(3) ? "Нет" : reader.GetString(3),
                        Share = reader.GetInt32(4)
                    };
                    Realtors.Add(realtor);
                    
                    string stringForComboBoxCollection = $"{realtor.Id} {realtor.FirstName} {realtor.SecondName} " +
                                                         $"{realtor.LastName}";
                    ComboBoxCollectionOfRealtors.Add(stringForComboBoxCollection);
                }
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
            OnPropertyChanged(nameof(Realtors));
        }
    }
    
    private Demand GetSpecificDemand(int id)
    {
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT * from demand where id = @id;";

            string query1 = "Select * from client where id = @clientId;";
            string query2 = "Select * from realtor where id = @realtorId;";
            
            string checkQuery1 = "Select * from apartmentDemand where DemandId = @id";
            string checkQuery2 = "Select * from houseDemand where DemandId = @id";
            string checkQuery3 = "Select * from landDemand where DemandId = @id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@id", id);

            var reader = cmd.ExecuteReader();

            Demand demand = new Demand();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    string type = "";
                    if (reader.GetInt32(1) == 1) type = "Квартира";
                    else if (reader.GetInt32(1) == 2) type = "Дом";
                    else type = "Земля";
                    demand = new Demand()
                    {
                        Id = reader.GetInt32(0),
                        RealEstateType = type,
                        MinCost = reader.GetInt32(2),
                        MaxCost = reader.GetInt32(3),
                        Client = new Client()
                        {
                            Id = reader.GetInt32(5)
                        },
                        Realtor = new Realtor()
                        {
                            Id = reader.GetInt32(6)
                        }
                    };
                    string[] address = reader.GetString(4).Split(',');
                    demand.Address = new Address()
                    {
                        City = address[0] == "" ? "Нет" : address[0],
                        Street = address[1] == "" ? "Нет" : address[1],
                        House = address[2] == "" ? "Нет" : address[2],
                        Apartment = address[3] == "" ? 0 : int.Parse(address[3])
                    };
                }
            }
            reader.Close();

            MySqlCommand checkComand = new MySqlCommand();
                checkComand.Connection = connection;
                checkComand.CommandText = query1;
                
                checkComand.Parameters.AddWithValue("@clientId", demand.Client.Id); 
                checkComand.Parameters.AddWithValue("@realtorId", demand.Realtor.Id); 
                
                var readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read())
                    {
                        demand.Client.FirstName = readerInside.IsDBNull(1) ? "Нет" : readerInside.GetString(1);
                        demand.Client.SecondName = readerInside.IsDBNull(2) ? "Нет" : readerInside.GetString(2);
                        demand.Client.LastName = readerInside.IsDBNull(3) ? "Нет" :readerInside.GetString(3);
                        demand.Client.Phone = readerInside.IsDBNull(4) ? "Нет" :readerInside.GetString(4);
                        demand.Client.Email = readerInside.IsDBNull(5) ? "Нет" : reader.GetString(5);
                    }
                } 
                readerInside.Close();
                
                checkComand.CommandText = query2; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read()) 
                    { 
                        demand.Realtor.FirstName = readerInside.IsDBNull(1) ? "Нет" : readerInside.GetString(1);
                        demand.Realtor.SecondName = readerInside.IsDBNull(2) ? "Нет" : readerInside.GetString(2);
                        demand.Realtor.LastName = readerInside.IsDBNull(3) ? "Нет" :readerInside.GetString(3);
                        demand.Realtor.Share = readerInside.IsDBNull(4) ? 0 :readerInside.GetInt32(4);
                    }
                }
                readerInside.Close();
                
                checkComand.Parameters.AddWithValue("@id", id);

                DemandMoreInformation moreInformation = new DemandMoreInformation();
                
                checkComand.CommandText = checkQuery1;
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows)
                {
                    while (readerInside.Read())
                    {
                        moreInformation = new DemandMoreInformation()
                        {
                            MinArea = readerInside.GetFloat(1),
                            MaxArea = readerInside.GetFloat(2),
                            MinRooms = readerInside.GetInt32(3),
                            MaxRooms = readerInside.GetInt32(4),
                            MinFloor = readerInside.GetInt32(5),
                            MaxFloor = readerInside.GetInt32(6)
                        };
                    }
                } 
                readerInside.Close();
                
                checkComand.CommandText = checkQuery2; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                {
                    while (readerInside.Read())
                    {
                        moreInformation = new DemandMoreInformation()
                        {
                            MinArea = readerInside.GetFloat(1),
                            MaxArea = readerInside.GetFloat(2),
                            MinRooms = readerInside.GetInt32(3),
                            MaxRooms = readerInside.GetInt32(4),
                            MinFloor = readerInside.GetInt32(5),
                            MaxFloor = readerInside.GetInt32(6)
                        };
                    }                
                }
                readerInside.Close();
                
                checkComand.CommandText = checkQuery3; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read())
                    {
                        moreInformation = new DemandMoreInformation()
                        {
                            MinArea = readerInside.GetFloat(1),
                            MaxArea = readerInside.GetFloat(2),
                        };
                    }
                } 
                readerInside.Close();

                demand.MoreInformation = moreInformation;

                return demand;
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

    private ObservableCollection<Deal> GetRelatedDeals(int id)
    {
        ObservableCollection<Deal> deals = new ObservableCollection<Deal>();
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT * from deal where deal.DemandId = @demandId;";
            string query1 = "Select * from supply where id = @supplyId;";
            string query2 = "Select * from client where id = @clientId;";
            string query3 = "Select * from realtor where id = @realtorId;";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@demandId", id);

            var reader = cmd.ExecuteReader();
            
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Deal deal = new Deal()
                    {
                        Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        Demand = SelectedDemand,
                        Supply = new Supply()
                        {
                            Id = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
                        }
                    };
                    deals.Add(deal);
                }
            }
            reader.Close();

            foreach (var deal in deals)
            {
                MySqlCommand checkComand = new MySqlCommand();
                checkComand.Connection = connection;
                checkComand.CommandText = query1;
                checkComand.Parameters.AddWithValue("@supplyId", deal.Supply.Id); 
                var readerInside = checkComand.ExecuteReader(); 
                
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read())
                    {
                        deal.Supply.Id = readerInside.IsDBNull(0) ? 0 : readerInside.GetInt32(0);
                        deal.Supply.Cost = readerInside.IsDBNull(1) ? 0 : readerInside.GetInt32(1);
                        deal.Supply.Client = new Client()
                        {
                            Id = readerInside.IsDBNull(2) ? 0 : readerInside.GetInt32(2)
                        };
                        deal.Supply.Realtor = new Realtor()
                        {
                            Id = readerInside.IsDBNull(3) ? 0 : readerInside.GetInt32(3)
                        };
                        deal.Supply.RealEstate = new RealEstate()
                        {
                            Id = readerInside.IsDBNull(4) ? 0 : readerInside.GetInt32(4)
                        };
                    }
                } 
                readerInside.Close();
                
                checkComand.Parameters.AddWithValue("@clientId", deal.Supply.Client.Id); 
                checkComand.Parameters.AddWithValue("@realtorId", deal.Supply.Realtor.Id);

                checkComand.CommandText = query2;
                readerInside = checkComand.ExecuteReader();
                if (readerInside.HasRows)
                {
                    while (readerInside.Read())
                    {
                        deal.Supply.Client.FirstName = readerInside.IsDBNull(1) ? "Нет" : readerInside.GetString(1);
                        deal.Supply.Client.SecondName = readerInside.IsDBNull(2) ? "Нет" : readerInside.GetString(2);
                        deal.Supply.Client.LastName = readerInside.IsDBNull(3) ? "Нет" : readerInside.GetString(3);
                        deal.Supply.Client.Phone = readerInside.IsDBNull(4) ? "Нет" : readerInside.GetString(4);
                        deal.Supply.Client.Email = readerInside.IsDBNull(5) ? "Нет" : readerInside.GetString(5);
                    }
                }
                readerInside.Close();
                
                checkComand.CommandText = query3;
                readerInside = checkComand.ExecuteReader();
                if (readerInside.HasRows)
                {
                    while (readerInside.Read())
                    {
                        deal.Supply.Realtor.FirstName = readerInside.IsDBNull(1) ? "Нет" : readerInside.GetString(1);
                        deal.Supply.Realtor.SecondName = readerInside.IsDBNull(2) ? "Нет" : readerInside.GetString(2);
                        deal.Supply.Realtor.LastName = readerInside.IsDBNull(3) ? "Нет" : readerInside.GetString(3);
                        deal.Supply.Realtor.Share = readerInside.IsDBNull(4) ? 0 : readerInside.GetInt32(4);
                    }
                }
                readerInside.Close();
            }

            return deals;
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
            OnPropertyChanged(nameof(RelatedDeals));
        }
    }

    private bool IsTwoDemandsEven(Demand demand1, Demand demand2)
    {
        if (demand1.Id != demand2.Id || demand1.RealEstateType != demand2.RealEstateType ||
            demand1.MinCost != demand2.MinCost || demand1.MaxCost != demand2.MaxCost ||
            demand1.Address.City != demand1.Address.City ||
            demand1.Address.Street != demand2.Address.Street ||
            demand1.Address.House != demand2.Address.House ||
            demand1.Address.Apartment != demand2.Address.Apartment ||
            demand1.Client.Id != demand2.Client.Id ||
            demand1.Realtor.Id != demand2.Realtor.Id)
            return false;
        return true;
    }

    
}