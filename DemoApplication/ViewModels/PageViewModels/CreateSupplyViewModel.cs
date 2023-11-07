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

public class CreateSupplyViewModel : ViewModelBase
{
    public CreateSupplyViewModel()
    {
        GetAllRealtors();
        GetAllClients();
        GetAllFreeRealEstates();
        
        SaveSupplyCommand = new LambdaCommand(OnSaveSupplyCommandExecuted);
    }

    private Supply _supply = new Supply()
    {
        Client = new Client(),
        RealEstate = new RealEstate(),
        Realtor = new Realtor()
    };
    public Supply Supply
    {
        get => _supply;
        set => this.RaiseAndSetIfChanged(ref _supply, value);
    }
    
    #region Коллекции

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

    #endregion

    #region Выбираемые переменные

    private Client _selectedClient;
    public Client SelectedClient
    {
        get => _selectedClient;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedClient, value);
            Supply.Client = value;
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
            Supply.Realtor = value;
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
            Supply.RealEstate = value;
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

    #endregion
    
    #region Команды

    #region Команды изменения бд

    public ICommand SaveSupplyCommand { get; }

    private void OnSaveSupplyCommandExecuted(object parameter)
    {
        if (Supply.Cost == null || Supply.Cost == 0 || 
            SelectedRealtor == null || SelectedClient == null || 
            SelectedRealEstate == null)
        {
            Console.WriteLine("Введите все данные");
            return;
        }
        
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query1 = "insert into supply (Cost, ClientId, RealtorId, RealEstateId) " +
                            "values (@cost, @clientId, @realtorId, @realEstateId);";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query1;

            cmd.Parameters.AddWithValue("@cost", Supply.Cost);
            cmd.Parameters.AddWithValue("@clientId", SelectedClient.Id);
            cmd.Parameters.AddWithValue("@realtorId", SelectedRealtor.Id);
            cmd.Parameters.AddWithValue("@realEstateId", SelectedRealEstate.Id);

            cmd.ExecuteNonQuery();
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

    #endregion

    #endregion
    
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
}