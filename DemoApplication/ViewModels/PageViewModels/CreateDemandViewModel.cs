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
 
public class CreateDemandViewModel : ViewModelBase
{
    public CreateDemandViewModel()
    {
        GetAllClients();
        GetAllRealtors();
        
        SaveDemandCommand = new LambdaCommand(OnSaveDemandCommandExecuted);
    }

    private Demand _demand = new Demand()
    {
        Client = new Client(),
        Realtor = new Realtor(),
        Address = new Address(),
        MoreInformation = new DemandMoreInformation(),
        RealEstateType = "Квартира"
    };
    public Demand Demand
    {
        get => _demand;
        set => this.RaiseAndSetIfChanged(ref _demand, value);
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

    private Client _selectedClient;
    public Client SelectedClient
    {
        get => _selectedClient;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedClient, value);
            Demand.Client = value;
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
            Demand.Realtor = value;
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
    
    private string _selectedComboBoxRealEstateType = "Квартира";
    public string SelectedComboBoxRealEstateType
    {
        get => _selectedComboBoxRealEstateType;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedComboBoxRealEstateType, value);
            Demand.RealEstateType = value;
            if (value == "Земля")
                IsLandChosen = true;
            else
                IsLandChosen = false;
        }
    }

    #endregion
    
    #region Контроль видимости и доступности

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

    private void OnSaveDemandCommandExecuted(object parameter)
    {
        if ((Demand.MinCost != 0 && Demand.MaxCost != 0) && 
            Demand.MinCost > Demand.MaxCost)
        {
            Console.WriteLine("Указан некорректный разброс цен");
            return;
        }
        if ((Demand.MoreInformation.MinFloor != 0 &&
             Demand.MoreInformation.MaxFloor != 0) &&
            Demand.MoreInformation.MinFloor > Demand.MoreInformation.MaxFloor)
        {
            Console.WriteLine("Указан некорректный разброс этажей");
            return;
        }
        if ((Demand.MoreInformation.MinRooms != 0 &&
             Demand.MoreInformation.MaxRooms != 0) &&
            Demand.MoreInformation.MinRooms > Demand.MoreInformation.MaxRooms)
        {
            Console.WriteLine("Указан некорректный разброс количества комнат");
            return;
        }
        if ((Demand.MoreInformation.MinArea != 0 &&
             Demand.MoreInformation.MaxArea != 0) &&
            Demand.MoreInformation.MinArea > Demand.MoreInformation.MaxArea)
        {
            Console.WriteLine("Указан некорректный разброс площади");
            return;
        }
        
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query1 = "insert into demand (RealEstateType, MinCost, MaxCost, Address, CLientId, RealtorId) " +
                            "values (@realEstateType, @minCost, @maxCost, @address, @clientId, @realtorId);";
            string query2 = "select id from demand order by id desc limit 1;";
            string query3 = "";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query1;
            
            if (Demand.RealEstateType == "Квартира")
            {
                cmd.Parameters.AddWithValue("@realEstateType", 1);
            }
            else if (Demand.RealEstateType == "Дом")
            {
                cmd.Parameters.AddWithValue("@realEstateType", 2);
            }
            else if (Demand.RealEstateType == "Земля")
            {
                cmd.Parameters.AddWithValue("@realEstateType", 3);
            }

            cmd.Parameters.AddWithValue("@minCost", Demand.MinCost == null ? 0 : Demand.MinCost);
            cmd.Parameters.AddWithValue("@maxCost", Demand.MaxCost == null ? 0 : Demand.MaxCost);
            cmd.Parameters.AddWithValue("@address", $"{Demand.Address.City}," +
                                                    $"{Demand.Address.Street}," +
                                                    $"{Demand.Address.House}," +
                                                    $"{Demand.Address.Apartment}");
            cmd.Parameters.AddWithValue("@clientId", Demand.Client.Id);
            cmd.Parameters.AddWithValue("@realtorId", Demand.Realtor.Id);

            cmd.ExecuteNonQuery();

            int id = 0;
            
            cmd.CommandText = query2;
            var reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    id = reader.GetInt32(0);
                }
            }
            reader.Close();

            cmd.Parameters.AddWithValue("@demandId", id);
            
            if (Demand.RealEstateType == "Квартира")
            {
                query3 = "insert into apartmentDemand values (@demandId, @minArea, @maxArea, @minRooms, @maxRooms, " +
                         "@minFloor, @maxFloor);";

                cmd.Parameters.AddWithValue("@minArea", Demand.MoreInformation.MinArea);
                cmd.Parameters.AddWithValue("@maxArea", Demand.MoreInformation.MaxArea);
                cmd.Parameters.AddWithValue("@minRooms", Demand.MoreInformation.MinRooms);
                cmd.Parameters.AddWithValue("@maxRooms", Demand.MoreInformation.MaxRooms);
                cmd.Parameters.AddWithValue("@minFloor", Demand.MoreInformation.MinFloor);
                cmd.Parameters.AddWithValue("@maxFloor", Demand.MoreInformation.MaxFloor);
            }
            else if (Demand.RealEstateType == "Дом")
            {
                query3 = "insert into houseDemand values (@demandId, @minArea, @maxArea, @minRooms, @maxRooms, " +
                         "@minFloor, @maxFloor);";
                cmd.Parameters.AddWithValue("@minArea", Demand.MoreInformation.MinArea);
                cmd.Parameters.AddWithValue("@maxArea", Demand.MoreInformation.MaxArea);
                cmd.Parameters.AddWithValue("@minRooms", Demand.MoreInformation.MinRooms);
                cmd.Parameters.AddWithValue("@maxRooms", Demand.MoreInformation.MaxRooms);
                cmd.Parameters.AddWithValue("@minFloor", Demand.MoreInformation.MinFloor);
                cmd.Parameters.AddWithValue("@maxFloor", Demand.MoreInformation.MaxFloor);
            }
            else if (Demand.RealEstateType == "Земля")
            {
                query3 = "insert into landDemand values (@demandId, @minArea, @maxArea);";
                cmd.Parameters.AddWithValue("@minArea", Demand.MoreInformation.MinArea);
                cmd.Parameters.AddWithValue("@maxArea", Demand.MoreInformation.MaxArea);
            }

            cmd.CommandText = query3;
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
}