using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DemoApplication.Infrastructure.Commands;
using DemoApplication.Infrastructure.DB;
using DemoApplication.Models;
using MySqlConnector;
using ReactiveUI;

namespace DemoApplication.ViewModels.PageViewModels;

public class ClientsViewModel : ViewModelBase
{
    public ClientsViewModel()
    {
        GetAllClients();

        #region Команды

        SaveClientCommand = new LambdaCommand(OnSaveClientCommandExecuted);
        DeleteClientCommand = new LambdaCommand(OnDeleteClientCommandExecuted);
        CancelClientEditionCommand = new LambdaCommand(OnCancelClientEditionCommandExecuted);

        #endregion
    }
    
    private ObservableCollection<Client> _clients = new ObservableCollection<Client>();
    public ObservableCollection<Client> Clients
    {
        get => _clients;
        set => this.RaiseAndSetIfChanged(ref _clients, value);
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

    private Client _oldSelectedClient = new Client();
    public Client OldSelectedClient
    {
        get => _oldSelectedClient;
        set => this.RaiseAndSetIfChanged(ref _oldSelectedClient, value);
    }

    private int _countOfSelections = 1;
    
    private Client _selectedClient;
    public Client SelectedClient
    {
        get => _selectedClient;
        set
        {
            if (value == null)
            {
                this.RaiseAndSetIfChanged(ref _selectedClient, value);
                return;
            }
            if (_countOfSelections == 1)
            {
                this.RaiseAndSetIfChanged(ref _selectedClient, value);
                _countOfSelections++;
                OldSelectedClient = GetSpecificClient(SelectedClient.Id);
                IsClientSelected = true;
                SelectedClientDemands.Clear();
                SelectedClientSupplies.Clear();
                SelectedComboBoxItem = "Нет";
                GetSpecificClientSupplies(SelectedClient.Id);
                GetSpecificClientDemands(SelectedClient.Id);
                return;
            }
            if (IsCancellingHappening)
            {
                SelectedClientDemands.Clear();
                SelectedClientSupplies.Clear();
                SelectedComboBoxItem = "Нет";
                this.RaiseAndSetIfChanged(ref _selectedClient, OldSelectedClient);
                IsCancellingHappening = false;
                return;
            }
            OldSelectedClient = GetSpecificClient(SelectedClient.Id);
            if (!IsTwoClientsEven(OldSelectedClient, SelectedClient))
                IsEditionSaved = false;
            if (IsEditionSaved == false)
            { 
                Console.WriteLine("Вы не сохранили изменения. Либо сохраните, либо отмените их");
            }
            else
            {
                this.RaiseAndSetIfChanged(ref _selectedClient, value);
                IsClientSelected = true;
                SelectedClientDemands.Clear();
                SelectedClientSupplies.Clear();
                SelectedComboBoxItem = "Нет";
                GetSpecificClientSupplies(SelectedClient.Id);
                GetSpecificClientDemands(SelectedClient.Id);
            }
        }
    }

    private bool _isClientSelected = false;
    public bool IsClientSelected
    {
        get => _isClientSelected;
        set => this.RaiseAndSetIfChanged(ref _isClientSelected, value);
    }

    private ObservableCollection<string> _comboBoxCollection = new ObservableCollection<string>()
    {
        "Нет", "Предложения", "Потребности", "Изменить данные"
    };
    public ObservableCollection<string> ComboBoxCollection
    {
        get => _comboBoxCollection;
        set => this.RaiseAndSetIfChanged(ref _comboBoxCollection, value);
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
                ListBoxSupplyIsVisible = false;
                ListBoxDemandIsVisible = false;
                IsUserEditingVisible = false;
            }
            else if (value == "Предложения")
            {
                ListBoxDemandIsVisible = false;
                ListBoxSupplyIsVisible = true;
                IsUserEditingVisible = false;
            }
            else if (value == "Потребности")
            {
                ListBoxDemandIsVisible = true;
                ListBoxSupplyIsVisible = false;
                IsUserEditingVisible = false;
            }
            else if (value == "Изменить данные")
            {
                ListBoxDemandIsVisible = false;
                ListBoxSupplyIsVisible = false;
                IsUserEditingVisible = true;
            }
        }
    }

    private bool _listBoxDemandIsVisible = false;
    public bool ListBoxDemandIsVisible
    {
        get => _listBoxDemandIsVisible;
        set => this.RaiseAndSetIfChanged(ref _listBoxDemandIsVisible, value);
    }

    private bool _listBoxSupplyIsVisible = false;
    public bool ListBoxSupplyIsVisible
    {
        get => _listBoxSupplyIsVisible;
        set => this.RaiseAndSetIfChanged(ref _listBoxSupplyIsVisible, value);
    }

    private bool _isUserEditingVisible = false;
    public bool IsUserEditingVisible
    {
        get => _isUserEditingVisible;
        set => this.RaiseAndSetIfChanged(ref _isUserEditingVisible, value);
    }

    private ObservableCollection<Demand> _selectedClientDemands = new ObservableCollection<Demand>();
    public ObservableCollection<Demand> SelectedClientDemands
    {
        get => _selectedClientDemands;
        set => this.RaiseAndSetIfChanged(ref _selectedClientDemands, value);
    }
    
    private ObservableCollection<Supply> _selectedClientSupplies = new ObservableCollection<Supply>();
    public ObservableCollection<Supply> SelectedClientSupplies
    {
        get => _selectedClientSupplies;
        set => this.RaiseAndSetIfChanged(ref _selectedClientSupplies, value);
    }

    #region Команды

    #region Команды изменения бд

    public ICommand SaveClientCommand { get; }
    public ICommand DeleteClientCommand { get; }
    public ICommand CancelClientEditionCommand { get; }

    private void OnSaveClientCommandExecuted(object parameter)
    {
        if ((SelectedClient.Email == "" || SelectedClient.Email == "Нет") && 
            (SelectedClient.Phone == "" || SelectedClient.Phone == "Нет"))
        {
            Console.WriteLine("Введите телефон или email");
            OnCancelClientEditionCommandExecuted();
        }
        
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query1 = "update client set " +
                            "FirstName = @firstName, " +
                            "SecondName = @secondName, " +
                            "LastName = @lastName, " +
                            "Phone = @phone, " +
                            "Email = @email " +
                            "where id = @id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query1;

            cmd.Parameters.AddWithValue("@id", SelectedClient.Id);
            cmd.Parameters.AddWithValue("@firstName", SelectedClient.FirstName);
            cmd.Parameters.AddWithValue("@secondName", SelectedClient.SecondName);
            cmd.Parameters.AddWithValue("@lastName", SelectedClient.LastName);
            cmd.Parameters.AddWithValue("@phone", SelectedClient.Phone);
            cmd.Parameters.AddWithValue("@email", SelectedClient.Email);

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
    private void OnDeleteClientCommandExecuted(object parameter)
    {
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query1 = "select * from supply " +
                            "where ClientId = @id;";
            string query2 = "select * from demand " +
                            "where ClientId = @id;";
            string query3 = "delete from client " +
                            "where id = @id;";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query1;
            cmd.Parameters.AddWithValue("@id", SelectedClient.Id);

            var reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Console.WriteLine($"Невозможно удалить клиента, " +
                                      $"участвующего в предложении №{reader.GetInt32(0)}");
                    return;
                }
            }
            reader.Close();

            cmd.CommandText = query2;
            reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Console.WriteLine($"Невозможно удалить клиента, " +
                                      $"участвующего в потребности №{reader.GetInt32(0)}");
                    return;
                }
            }
            reader.Close();

            cmd.CommandText = query3;
            cmd.ExecuteNonQuery();
            SelectedComboBoxItem = "Нет";
            GetAllClients();
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
    private void OnCancelClientEditionCommandExecuted(object parameter = null)
    {
        IsCancellingHappening = true;
        SelectedClient = OldSelectedClient;
        IsEditionSaved = true;
        GetAllClients();
        _countOfSelections = 1;
    }

    #endregion

    #endregion

    private void GetAllClients()
    {
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

    private void GetSpecificClientDemands(int id)
    {
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT * from demand where ClientId = @clientId;";

            string query1 = "Select * from realtor where id = @realtorId;";
            
            string checkQuery1 = "Select * from apartmentDemand where DemandId = @id";
            string checkQuery2 = "Select * from houseDemand where DemandId = @id";
            string checkQuery3 = "Select * from landDemand where DemandId = @id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@clientId", id);

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
                    SelectedClientDemands.Add(demand);
                }
            }
            reader.Close();

            foreach (var demand in SelectedClientDemands)
            {
                MySqlCommand checkComand = new MySqlCommand();
                checkComand.Connection = connection;
                checkComand.CommandText = query1;
                
                checkComand.Parameters.AddWithValue("@realtorId", demand.Realtor.Id); 
                
                var readerInside = checkComand.ExecuteReader(); 
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
            OnPropertyChanged(nameof(SelectedClientDemands));
        }
    }
    
    private void GetSpecificClientSupplies(int id)
    {
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT * from supply where ClientId = @clientId;";

            string query1 = "Select * from realtor where id = @realtorId;";
            string query2 = "Select * from address where RealEstateId = @realEstateId;";
            
            string checkQuery1 = "Select * from apartment where RealEstateId = @id";
            string checkQuery2 = "Select * from house where RealEstateId = @id";
            string checkQuery3 = "Select * from land where RealEstateId = @id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@clientId", id); 

            var reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Supply supply = new Supply()
                    {
                        Id = reader.GetInt32(0),
                        Cost = reader.GetInt32(1),
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
                    SelectedClientSupplies.Add(supply);
                }
            }
            reader.Close();

            foreach (var supply in SelectedClientSupplies)
            {
                MySqlCommand checkComand = new MySqlCommand();
                checkComand.Connection = connection;
                checkComand.CommandText = query1;
                
                checkComand.Parameters.AddWithValue("@realtorId", supply.Realtor.Id); 
                checkComand.Parameters.AddWithValue("@realEstateId", supply.RealEstate.Id); 
                
                var readerInside = checkComand.ExecuteReader(); 
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
                
                checkComand.CommandText = query2; 
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
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            connection.Dispose();
            connection.Close();
            OnPropertyChanged(nameof(SelectedClientSupplies));
        }
    }
    
    private Client GetSpecificClient(int id)
    {
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "select * from client where id = @id;";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@id", id);

            var reader = cmd.ExecuteReader();

            Client client = new Client();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    client = new Client()
                    {
                        Id = reader.GetInt32(0),
                        FirstName = reader.IsDBNull(1) ? "Нет" : reader.GetString(1),
                        SecondName = reader.IsDBNull(2) ? "Нет" : reader.GetString(2),
                        LastName = reader.IsDBNull(3) ? "Нет" : reader.GetString(3),
                        Phone = reader.IsDBNull(4) ? "Нет" : reader.GetString(4),
                        Email = reader.IsDBNull(5) ? "Нет" : reader.GetString(5)
                    };
                }
            }

            return client;
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
    
    private bool IsTwoClientsEven(Client client1, Client client2)
    {
        if (client1.FirstName != client2.FirstName ||
            client1.SecondName != client2.SecondName ||
            client1.LastName != client2.LastName ||
            client1.Phone != client2.Phone ||
            client1.Email != client2.Email ||
            client1.Id != client2.Id)
            return false;
        return true;
    }
}