using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DemoApplication.Infrastructure.Commands;
using DemoApplication.Infrastructure.DB;
using DemoApplication.Models;
using MySqlConnector;
using ReactiveUI;

namespace DemoApplication.ViewModels.PageViewModels;

public class SuppliesViewModel : ViewModelBase
{
    public SuppliesViewModel()
    {
        GetAllSupplies();
        GetAllClients();
        GetAllRealtors();
        GetAllFreeRealEstates();

        #region Команды

        SaveSupplyCommand = new LambdaCommand(OnSaveSupplyCommandExecuted);
        DeleteSupplyCommand = new LambdaCommand(OnDeleteSupplyCommandExecuted);
        CancelSupplyEditionCommand = new LambdaCommand(OnCancelSupplyEditionCommandExecuted);

        #endregion
    }

    #region Коллекции

    private ObservableCollection<Supply> _supplies = new ObservableCollection<Supply>();
    public ObservableCollection<Supply> Supplies
    {
        get => _supplies;
        set => this.RaiseAndSetIfChanged(ref _supplies, value);
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

    private ObservableCollection<RealEstate> _freeRealEstates = new ObservableCollection<RealEstate>();
    public ObservableCollection<RealEstate> FreeRealEstates
    {
        get => _freeRealEstates;
        set => this.RaiseAndSetIfChanged(ref _freeRealEstates, value);
    }
    
    private ObservableCollection<string> _comboBoxCollectionOfFreeRealEstates = new ObservableCollection<string>();
    public ObservableCollection<string> ComboBoxCollectionOfFreeRealEstates
    {
        get => _comboBoxCollectionOfFreeRealEstates;
        set => this.RaiseAndSetIfChanged(ref _comboBoxCollectionOfFreeRealEstates, value);
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
            SelectedSupply.Client = value;
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
            SelectedSupply.Realtor = value;
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

    private RealEstate _selectedRealEstate;
    public RealEstate SelectedRealEstate
    {
        get => _selectedRealEstate;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedRealEstate, value);
            SelectedSupply.RealEstate = value;
        }
    }
    
    private string _selectedComboBoxRealEstate;
    public string SelectedComboBoxRealEstate
    {
        get => _selectedComboBoxRealEstate;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedComboBoxRealEstate, value);
            if (value != null && value != "")
                SelectedRealEstate = FreeRealEstates.First(x => x.Id.ToString() == value.Split(' ')[0] &&
                                                      x.Type == value.Split(' ')[1]);
        }
    }
    
    private Supply _oldSelectedSupply = new Supply();
    public Supply OldSelectedSupply
    {
        get => _oldSelectedSupply;
        set => this.RaiseAndSetIfChanged(ref _oldSelectedSupply, value);
    }
    
    private int _countOfSelections = 1;

    private Supply _selectedSupply;
    public Supply SelectedSupply
    {
        get => _selectedSupply;
        set
        {
            if (value == null)
            {
                this.RaiseAndSetIfChanged(ref _selectedSupply, value);
                return;
            }
            if (_countOfSelections == 1)
            {
                _countOfSelections++;
                this.RaiseAndSetIfChanged(ref _selectedSupply, value);
                OldSelectedSupply = GetSpecificSupply(SelectedSupply.Id);
                GetAllFreeRealEstates();
                IsSupplySelected = true;
                SelectedComboBoxClient = $"{value.Client.Id} {value.Client.FirstName} " +
                                         $"{value.Client.SecondName} {value.Client.LastName}";
                SelectedComboBoxRealtor = $"{value.Realtor.Id} {value.Realtor.FirstName} " +
                                          $"{value.Realtor.SecondName} {value.Realtor.LastName}";
                FreeRealEstates.Add(value.RealEstate);
                ComboBoxCollectionOfFreeRealEstates.Add($"{value.RealEstate.Id} {value.RealEstate.Type}");
                SelectedComboBoxRealEstate = $"{value.RealEstate.Id} {value.RealEstate.Type}";

                RelatedDeals = GetRelatedDeals(value.Id);
                
                return;
            }
            if (IsCancellingHappening)
            {
                IsSupplySelected = false;
                SelectedComboBoxItem = "Нет";
                SelectedComboBoxClient = "";
                SelectedComboBoxRealtor = "";
                SelectedComboBoxRealEstate = "";
                GetAllFreeRealEstates();
                this.RaiseAndSetIfChanged(ref _selectedSupply, OldSelectedSupply);
                IsCancellingHappening = false;
                return;
            }
            OldSelectedSupply = GetSpecificSupply(SelectedSupply.Id);
            if (!IsTwoSuppliesEven(OldSelectedSupply, SelectedSupply))
                IsEditionSaved = false;
            if (IsEditionSaved == false)
            { 
                Console.WriteLine("Вы не сохранили изменения. Либо сохраните, либо отмените их");
            }
            else
            {
                this.RaiseAndSetIfChanged(ref _selectedSupply, value);
                OldSelectedSupply = GetSpecificSupply(SelectedSupply.Id);
                GetAllFreeRealEstates();
                IsSupplySelected = true;
                SelectedComboBoxClient = $"{value.Client.Id} {value.Client.FirstName} " +
                                         $"{value.Client.SecondName} {value.Client.LastName}";
                SelectedComboBoxRealtor = $"{value.Realtor.Id} {value.Realtor.FirstName} " +
                                          $"{value.Realtor.SecondName} {value.Realtor.LastName}";
                FreeRealEstates.Add(value.RealEstate);
                ComboBoxCollectionOfFreeRealEstates.Add($"{value.RealEstate.Id} {value.RealEstate.Type}");
                SelectedComboBoxRealEstate = $"{value.RealEstate.Id} {value.RealEstate.Type}";
                RelatedDeals = GetRelatedDeals(value.Id);
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

    #endregion

    #region Контроль видимости и доступности

    private bool _isSupplySelected = false;
    public bool IsSupplySelected
    {
        get => _isSupplySelected;
        set => this.RaiseAndSetIfChanged(ref _isSupplySelected, value);
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

    #endregion
    
    #region Команды

    #region Команды изменения бд

    public ICommand SaveSupplyCommand { get; }
    public ICommand DeleteSupplyCommand { get; }
    public ICommand CancelSupplyEditionCommand { get; }

    private void OnSaveSupplyCommandExecuted(object parameter)
    {
        if (SelectedSupply.Cost == null || SelectedSupply.Cost == 0)
        {
            Console.WriteLine("Вы не выбрали цену");
            OnCancelSupplyEditionCommandExecuted();
            return;
        }
        
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query1 = "update supply set " +
                            "Cost = @cost, " +
                            "ClientId = @clientId, " +
                            "RealtorId = @realtorId, " +
                            "RealEstateId = @realEstateId " +
                            "where id = @id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query1;

            cmd.Parameters.AddWithValue("@id", SelectedSupply.Id);
            cmd.Parameters.AddWithValue("@cost", SelectedSupply.Cost);
            cmd.Parameters.AddWithValue("@clientId", SelectedClient.Id);
            cmd.Parameters.AddWithValue("@realtorId", SelectedRealtor.Id);
            cmd.Parameters.AddWithValue("@realEstateId", SelectedRealEstate.Id);

            cmd.ExecuteNonQuery();
            IsEditionSaved = true;
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
    private void OnDeleteSupplyCommandExecuted(object parameter)
    {
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query1 = "select * from deal " +
                            "where SupplyId = @id;";
            string query2 = "delete from supply " +
                            "where id = @id;";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query1;
            cmd.Parameters.AddWithValue("@id", SelectedSupply.Id);

            var reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Console.WriteLine($"Невозможно удалить предложение, " +
                                      $"участвующее в сделке №{reader.GetInt32(0)}");
                    return;
                }
            }
            reader.Close();

            cmd.CommandText = query2;
            cmd.ExecuteNonQuery();
            SelectedSupply = null;
            IsSupplySelected = false;
            SelectedComboBoxItem = "Нет";
            SelectedComboBoxClient = "";
            SelectedComboBoxRealtor = "";
            SelectedComboBoxRealEstate = "";
            GetAllSupplies();
            GetAllFreeRealEstates();
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
    private void OnCancelSupplyEditionCommandExecuted(object parameter = null)
    {
        IsCancellingHappening = true;
        SelectedSupply = OldSelectedSupply;
        IsEditionSaved = true;
        GetAllSupplies();
        GetAllFreeRealEstates();
        _countOfSelections = 1;
    }

    #endregion

    #endregion

    private void GetAllSupplies()
    {
        Supplies.Clear();
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT * from supply;";

            string query1 = "Select * from client where id = @clientId;";
            string query2 = "Select * from realtor where id = @realtorId;";
            string query3 = "Select * from address where RealEstateId = @realEstateId;";
            string query4 = "Select * from coordinates where RealEstateId = @realEstateId;";
            
            string checkQuery1 = "Select * from apartment where RealEstateId = @realEstateId";
            string checkQuery2 = "Select * from house where RealEstateId = @realEstateId";
            string checkQuery3 = "Select * from land where RealEstateId = @realEstateId";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;

            var reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Supply supply = new Supply()
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
                    Supplies.Add(supply);
                }
            }
            reader.Close();

            foreach (var supply in Supplies)
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
                
                checkComand.CommandText = query4; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                {
                    while (readerInside.Read())
                    {
                        supply.RealEstate.Coordinates = new Coordinates();
                        supply.RealEstate.Coordinates.Latitude = readerInside.IsDBNull(1) ? 0 : readerInside.GetFloat(1);
                        supply.RealEstate.Coordinates.Longitude = readerInside.IsDBNull(2) ? 0 : readerInside.GetFloat(2);
                    }
                } 
                readerInside.Close();
                
                checkComand.CommandText = checkQuery1;
                readerInside = checkComand.ExecuteReader(); 
                
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read())
                    {
                        supply.RealEstate.MoreInformation = new RealEstateMoreInformation()
                        {
                            Floor = reader.IsDBNull(1) ? 0 : readerInside.GetInt32(1),
                            Rooms = readerInside.IsDBNull(2) ? 0 : readerInside.GetInt32(2),
                            TotalArea = reader.IsDBNull(3) ? 0 : readerInside.GetFloat(3)

                        };
                        supply.RealEstate.Type = "Квартира";
                    }
                } 
                readerInside.Close();
                
                checkComand.CommandText = checkQuery2; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read()) 
                    { 
                        supply.RealEstate.MoreInformation = new RealEstateMoreInformation() 
                        { 
                            Floor = reader.IsDBNull(1) ? 0 : readerInside.GetInt32(1), 
                            Rooms = readerInside.IsDBNull(2) ? 0 : readerInside.GetInt32(2), 
                            TotalArea = reader.IsDBNull(3) ? 0 : readerInside.GetFloat(3)
                        }; 
                        supply.RealEstate.Type = "Дом";
                    }
                }
                readerInside.Close();
                
                checkComand.CommandText = checkQuery3; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read()) 
                    { 
                        supply.RealEstate.MoreInformation = new RealEstateMoreInformation() 
                        { 
                            TotalArea = reader.IsDBNull(1) ? 0 : readerInside.GetFloat(1)
                        };
                        supply.RealEstate.Type = "Земля";
                    }
                } 
                readerInside.Close();
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
            OnPropertyChanged(nameof(Supplies));
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
    
    private void GetAllFreeRealEstates()
    {
        ComboBoxCollectionOfFreeRealEstates.Clear();
        FreeRealEstates.Clear();
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT realEstate.id, address.City, address.Street, address.House, " +
                           "address.Apartment, coordinates.Latitude, coordinates.Longitude " +
                           "FROM `realEstate` " +
                           "inner join address on realEstate.id = address.RealEstateId " +
                           "INNER JOIN coordinates on realEstate.id = coordinates.RealEstateId " +
                           "where realEstate.id not in (Select supply.RealEstateId from supply);";

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
                    FreeRealEstates.Add(realEstate);
                }
            }
            reader.Close();
            
            foreach (var realEstate in FreeRealEstates)
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
                
                string stringForComboBoxCollection = $"{realEstate.Id} {realEstate.Type}";
                ComboBoxCollectionOfFreeRealEstates.Add(stringForComboBoxCollection);
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
            OnPropertyChanged(nameof(FreeRealEstates));
        }
    }

    private Supply GetSpecificSupply(int id)
    {
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT * from supply where supply.id = @supplyId;";

            string query1 = "Select * from client where id = @clientId;";
            string query2 = "Select * from realtor where id = @realtorId;";
            string query3 = "Select * from address where RealEstateId = @realEstateId;";
            
            string checkQuery1 = "Select * from apartment where RealEstateId = @id";
            string checkQuery2 = "Select * from house where RealEstateId = @id";
            string checkQuery3 = "Select * from land where RealEstateId = @id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@supplyId", id);

            var reader = cmd.ExecuteReader();

            Supply supply = new Supply();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    supply = new Supply()
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
                    Supplies.Add(supply);
                }
            }
            reader.Close();

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
                return supply;
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
            OnPropertyChanged(nameof(Supplies));
        }
    }

    private ObservableCollection<Deal> GetRelatedDeals(int id)
    {
        ObservableCollection<Deal> deals = new ObservableCollection<Deal>();
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT * from deal where deal.SupplyId = @supplyId;";
            string query1 = "Select * from demand where id = @demandId;";
            string query2 = "Select * from client where id = @clientId;";
            string query3 = "Select * from realtor where id = @realtorId;";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@supplyId", id);

            var reader = cmd.ExecuteReader();
            
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Deal deal = new Deal()
                    {
                        Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        Demand = new Demand()
                        {
                            Id = reader.IsDBNull(1) ? 0 : reader.GetInt32(1)
                        },
                        Supply = SelectedSupply
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
                checkComand.Parameters.AddWithValue("@demandId", deal.Demand.Id); 
                var readerInside = checkComand.ExecuteReader(); 
                
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read())
                    {
                        if (readerInside.GetInt32(1) == 1)
                            deal.Demand.RealEstateType = "Квартира";
                        else if (readerInside.GetInt32(1) == 2)
                            deal.Demand.RealEstateType = "Квартира";
                        else if (readerInside.GetInt32(1) == 2)
                            deal.Demand.RealEstateType = "Квартира";
                        deal.Demand.MinCost = readerInside.IsDBNull(2) ? 0 : readerInside.GetInt32(2);
                        deal.Demand.MaxCost = readerInside.IsDBNull(3) ? 0 : readerInside.GetInt32(3);
                        
                        string address = readerInside.IsDBNull(3) ? ",,," : readerInside.GetString(4);
                        deal.Demand.Address = new Address()
                        {
                            City = address.Split(',')[0],
                            Street = address.Split(',')[1],
                            House = address.Split(',')[2],
                            Apartment = address.Split(',')[3] == "" ? 0 : int.Parse(address.Split(',')[3])
                        };

                        deal.Demand.Client = new Client()
                        {
                            Id = readerInside.IsDBNull(5) ? 0 : readerInside.GetInt32(5)
                        };
                        deal.Demand.Realtor = new Realtor()
                        {
                            Id = readerInside.IsDBNull(6) ? 0 : readerInside.GetInt32(6)
                        };
                    }
                } 
                readerInside.Close();
                
                checkComand.Parameters.AddWithValue("@clientId", deal.Demand.Client.Id); 
                checkComand.Parameters.AddWithValue("@realtorId", deal.Demand.Realtor.Id);

                checkComand.CommandText = query2;
                readerInside = checkComand.ExecuteReader();
                if (readerInside.HasRows)
                {
                    while (readerInside.Read())
                    {
                        deal.Demand.Client.FirstName = readerInside.IsDBNull(1) ? "Нет" : readerInside.GetString(1);
                        deal.Demand.Client.SecondName = readerInside.IsDBNull(2) ? "Нет" : readerInside.GetString(2);
                        deal.Demand.Client.LastName = readerInside.IsDBNull(3) ? "Нет" : readerInside.GetString(3);
                        deal.Demand.Client.Phone = readerInside.IsDBNull(4) ? "Нет" : readerInside.GetString(4);
                        deal.Demand.Client.Email = readerInside.IsDBNull(5) ? "Нет" : readerInside.GetString(5);
                    }
                }
                readerInside.Close();
                
                checkComand.CommandText = query3;
                readerInside = checkComand.ExecuteReader();
                if (readerInside.HasRows)
                {
                    while (readerInside.Read())
                    {
                        deal.Demand.Realtor.FirstName = readerInside.IsDBNull(1) ? "Нет" : readerInside.GetString(1);
                        deal.Demand.Realtor.SecondName = readerInside.IsDBNull(2) ? "Нет" : readerInside.GetString(2);
                        deal.Demand.Realtor.LastName = readerInside.IsDBNull(3) ? "Нет" : readerInside.GetString(3);
                        deal.Demand.Realtor.Share = readerInside.IsDBNull(4) ? 0 : readerInside.GetInt32(4);
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
            OnPropertyChanged(nameof(Supplies));
        }
    }

    private bool IsTwoSuppliesEven(Supply supply1, Supply supply2)
    {
        if (supply1.Id != supply2.Id || supply1.Cost != supply2.Cost ||
            supply1.Client.Id != supply2.Client.Id ||
            supply1.Realtor.Id != supply2.Realtor.Id ||
            supply1.RealEstate.Id != supply2.RealEstate.Id)
            return false;
        return true;
    }
}