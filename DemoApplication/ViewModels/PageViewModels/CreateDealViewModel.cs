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

public class CreateDealViewModel : ViewModelBase
{
    public CreateDealViewModel()
    {
        GetAllFreeSupplies();
        
        SaveDealCommand = new LambdaCommand(OnSaveDealCommandExecuted);
    }

    private Deal _deal = new Deal();
    public Deal Deal
    {
        get => _deal;
        set => this.RaiseAndSetIfChanged(ref _deal, value);
    }
    
    #region Коллекции
    
    private ObservableCollection<Demand> _demands = new ObservableCollection<Demand>();
    public ObservableCollection<Demand> Demands
    {
        get => _demands;
        set => this.RaiseAndSetIfChanged(ref _demands, value);
    }

    private ObservableCollection<string> _comboBoxCollectionOfDemands = new ObservableCollection<string>();
    public ObservableCollection<string> ComboBoxCollectionOfDemands
    {
        get => _comboBoxCollectionOfDemands;
        set => this.RaiseAndSetIfChanged(ref _comboBoxCollectionOfDemands, value);
    }

    private ObservableCollection<Supply> _supplies = new ObservableCollection<Supply>();
    public ObservableCollection<Supply> Supplies
    {
        get => _supplies;
        set => this.RaiseAndSetIfChanged(ref _supplies, value);
    }
    
    private ObservableCollection<string> _comboBoxCollectionOfSupplies = new ObservableCollection<string>();
    public ObservableCollection<string> ComboBoxCollectionOfSupplies
    {
        get => _comboBoxCollectionOfSupplies;
        set => this.RaiseAndSetIfChanged(ref _comboBoxCollectionOfSupplies, value);
    }
    
    private ObservableCollection<string> _comboBoxCollectionOfChoice = new ObservableCollection<string>()
    {
        "Предложение", "Потребность"
    };
    public ObservableCollection<string> ComboBoxCollectionOfChoice
    {
        get => _comboBoxCollectionOfChoice;
        set => this.RaiseAndSetIfChanged(ref _comboBoxCollectionOfChoice, value);
    }

    #endregion

    #region Выбираемые переменные

    private Demand _selectedDemand;
    public Demand SelectedDemand
    {
        get => _selectedDemand;
        set
        {
            try
            {
                this.RaiseAndSetIfChanged(ref _selectedDemand, value);
                Deal.Demand = value;
                if (SelectedComboBoxChoice == "Потребность")
                {
                    Supplies = GetSuitableSupplies(SelectedDemand.Id);
                    SelectedComboBoxSupply = "";
                    SelectedSupply = null;
                    Deal.Supply = null;
                    OnPropertyChanged(nameof(SelectedSupply));
                    return;
                }
                if (SelectedDemand != null && SelectedSupply != null)
                {
                    double companyCostOfSeller = 0;
                    double companyCostOfBuyer = 0;

                    if (Deal.Supply.RealEstate.Type == "Квартира")
                        companyCostOfSeller = 36000 + Deal.Supply.Cost * 0.01;
                    else if (Deal.Supply.RealEstate.Type == "Дом")
                        companyCostOfSeller = 30000 + Deal.Supply.Cost * 0.01;
                    else if (Deal.Supply.RealEstate.Type == "Земля")
                        companyCostOfSeller = 36000 + Deal.Supply.Cost * 0.01;
                    companyCostOfBuyer = Deal.Supply.Cost * 0.03;
                    Cost = new Pay()
                    {
                        CostOfRealtorServiceForCustomerSeller =
                            companyCostOfSeller * (Convert.ToDouble(SelectedSupply.Realtor.Share) / 100),
                        CostOfCompanyServiceForCustomerSeller =
                            companyCostOfSeller * ((100 - Convert.ToDouble(SelectedSupply.Realtor.Share)) / 100),
                        CostOfRealtorServiceForCustomerBuyer =
                            companyCostOfBuyer * (Convert.ToDouble(SelectedDemand.Realtor.Share) / 100),
                        CostOfCompanyServiceForCustomerBuyer =
                            companyCostOfBuyer * ((100 - Convert.ToDouble(SelectedDemand.Realtor.Share)) / 100)
                    };
                    OnPropertyChanged(nameof(Cost));   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private string _selectedComboBoxDemand;
    public string SelectedComboBoxDemand
    {
        get => _selectedComboBoxDemand;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedComboBoxDemand, value);
            if (value != null && value != "")
                SelectedDemand = Demands.First(x => x.Id.ToString() == value.Split(' ')[0] &&
                                                    x.RealEstateType == value.Split(' ')[1] &&
                                                    x.MinCost.ToString() == value.Split(' ')[2]
                                                        .Split('-')[0] &&
                                                    x.MaxCost.ToString() == value.Split(' ')[2]
                                                        .Split('-')[1]);
        }
    }

    private Supply _selectedSupply;
    public Supply SelectedSupply
    {
        get => _selectedSupply;
        set
        {
            try
            {
                this.RaiseAndSetIfChanged(ref _selectedSupply, value);
                Deal.Supply = value;
                if (SelectedComboBoxChoice == "Предложение")
                {
                    Demands = GetSuitableDemands(SelectedSupply.Id);
                    SelectedComboBoxDemand = "";
                    SelectedDemand = null;
                    Deal.Demand = null;
                    OnPropertyChanged(nameof(SelectedDemand));
                    return;
                }

                if (SelectedDemand != null && SelectedSupply != null)
                {
                    double companyCostOfSeller = 0;
                    double companyCostOfBuyer = 0;

                    if (Deal.Supply.RealEstate.Type == "Квартира")
                        companyCostOfSeller = 36000 + Deal.Supply.Cost * 0.01;
                    else if (Deal.Supply.RealEstate.Type == "Дом")
                        companyCostOfSeller = 30000 + Deal.Supply.Cost * 0.01;
                    else if (Deal.Supply.RealEstate.Type == "Земля")
                        companyCostOfSeller = 36000 + Deal.Supply.Cost * 0.01;
                    companyCostOfBuyer = Deal.Supply.Cost * 0.03;
                    Cost = new Pay()
                    {
                        CostOfRealtorServiceForCustomerSeller =
                            companyCostOfSeller * (Convert.ToDouble(SelectedSupply.Realtor.Share) / 100),
                        CostOfCompanyServiceForCustomerSeller =
                            companyCostOfSeller * ((100 - Convert.ToDouble(SelectedSupply.Realtor.Share)) / 100),
                        CostOfRealtorServiceForCustomerBuyer =
                            companyCostOfBuyer * (Convert.ToDouble(SelectedDemand.Realtor.Share) / 100),
                        CostOfCompanyServiceForCustomerBuyer =
                            companyCostOfBuyer * ((100 - Convert.ToDouble(SelectedDemand.Realtor.Share)) / 100)
                    };
                    OnPropertyChanged(nameof(Cost));   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private string _selectedComboBoxSupply;
    public string SelectedComboBoxSupply
    {
        get => _selectedComboBoxSupply;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedComboBoxSupply, value);
            if (value != null && value != "")
                SelectedSupply = Supplies.First(x => x.Id.ToString() == value.Split(' ')[0] &&
                                                    x.RealEstate.Type == value.Split(' ')[1] &&
                                                    x.Cost.ToString() == value.Split(' ')[2]);
        }
    }
    
    private string _selectedComboBoxChoice = "Предложение";
    public string SelectedComboBoxChoice
    {
        get => _selectedComboBoxChoice;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedComboBoxChoice, value);
            if (value == "Предложение")
            {
                IsSupplyChosen = true;
                Demands = GetSuitableDemands(SelectedSupply.Id);
            }
            else if (value == "Потребность")
            {
                IsSupplyChosen = false;
                Supplies = GetSuitableSupplies(SelectedDemand.Id);
            }
            else
            {
                GetAllFreeSupplies();
                GetAllFreeDemands();
            }
        }
    }

    #endregion
    
    #region Контроль видимости и доступности

    private bool _isSupplyChosen;
    public bool IsSupplyChosen
    {
        get => _isSupplyChosen;
        set => this.RaiseAndSetIfChanged(ref _isSupplyChosen, value);
    }
    
    private bool _isLandChosenInDemand;
    public bool IsLandChosenInDemand
    {
        get => _isLandChosenInDemand;
        set => this.RaiseAndSetIfChanged(ref _isLandChosenInDemand, value);
    }
    
    private bool _isLandChosenInSupply;
    public bool IsLandChosenInSupply
    {
        get => _isLandChosenInSupply;
        set => this.RaiseAndSetIfChanged(ref _isLandChosenInSupply, value);
    }
    
    #endregion

    #region Отчисления и комиссии

    private Pay _cost;
    public Pay Cost
    {
        get => _cost;
        set => this.RaiseAndSetIfChanged(ref _cost, value);
    }

    #endregion
    
    #region Команды

    #region Команды изменения бд

    public ICommand SaveDealCommand { get; }

    private void OnSaveDealCommandExecuted(object parameter)
    {
        if (Deal.Demand == null || Deal.Supply == null)
        {
            Console.WriteLine("Вы не ввели все данные");
            return;
        }
        
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query1 = "insert into deal (DemandId, SupplyId) values " +
                            "(@demandId, @supplyId);";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query1;

            cmd.Parameters.AddWithValue("@demandId", Deal.Demand.Id);
            cmd.Parameters.AddWithValue("@supplyId", Deal.Supply.Id);

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
            string query4 = "Select * from coordinates where RealEstateId = @realEstateId;";
            
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
                
                checkComand.Parameters.AddWithValue("@id", supply.RealEstate.Id); 
                
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
    
    private void GetAllFreeDemands()
    {
        ComboBoxCollectionOfDemands.Clear();
        Demands.Clear();
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT * from demand where demand.id not in (Select DemandId from deal);";

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
                    
                    string stringForComboBoxCollection = $"{demand.Id} {demand.RealEstateType} {demand.MinCost}-" +
                                                         $"{demand.MaxCost}";
                    ComboBoxCollectionOfDemands.Add(stringForComboBoxCollection);
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
    
    private void GetAllFreeSupplies()
    {
        ComboBoxCollectionOfSupplies.Clear();
        Supplies.Clear();
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT * from supply where supply.id not in (Select SupplyId from deal);";

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
                
                string stringForComboBoxCollection = $"{supply.Id} {supply.RealEstate.Type} {supply.Cost}";
                ComboBoxCollectionOfSupplies.Add(stringForComboBoxCollection);
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

    private ObservableCollection<Demand> GetSuitableDemands(int id)
    {
        Supply supply = GetSpecificSupply(id);
        GetAllFreeDemands();
        ComboBoxCollectionOfDemands.Clear();

        ObservableCollection<Demand> suitableDemands = new ObservableCollection<Demand>();

        foreach (var demand in Demands)
        {
            if ((demand.MinCost == 0 || (supply.Cost >= demand.MinCost)) && 
                (demand.MaxCost == 0 || (supply.Cost <= demand.MaxCost)) &&
                (supply.RealEstate.Type == demand.RealEstateType) &&
                (demand.MoreInformation.MinArea == 0 || (supply.RealEstate.MoreInformation.TotalArea >= demand.MoreInformation.MinArea)) &&
                (demand.MoreInformation.MaxArea == 0 || (supply.RealEstate.MoreInformation.TotalArea <= demand.MoreInformation.MaxArea)) &&
                (demand.MoreInformation.MinRooms == 0 || (supply.RealEstate.MoreInformation.Rooms >= demand.MoreInformation.MinRooms)) &&
                (demand.MoreInformation.MaxRooms == 0 || (supply.RealEstate.MoreInformation.Rooms <= demand.MoreInformation.MaxRooms)) &&
                (demand.MoreInformation.MinFloor == 0 || (supply.RealEstate.MoreInformation.Floor >= demand.MoreInformation.MinFloor)) &&
                (demand.MoreInformation.MaxFloor == 0 || (supply.RealEstate.MoreInformation.Floor <= demand.MoreInformation.MaxFloor)))
                suitableDemands.Add(demand);
        }

        foreach (var demand in suitableDemands)
        {
            string str = $"{demand.Id} {demand.RealEstateType} {demand.MinCost}-{demand.MaxCost}";
            ComboBoxCollectionOfDemands.Add(str);
        }

        return suitableDemands;
    }
    
    private ObservableCollection<Supply> GetSuitableSupplies(int id)
    {
        Demand demand = GetSpecificDemand(id);
        GetAllFreeSupplies();
        ComboBoxCollectionOfSupplies.Clear();

        ObservableCollection<Supply> suitableSupplies = new ObservableCollection<Supply>();

        foreach (var supply in Supplies)
        {
            if ((demand.MinCost == 0 || (supply.Cost >= demand.MinCost)) && 
                (demand.MaxCost == 0 || (supply.Cost <= demand.MaxCost)) &&
                (supply.RealEstate.Type == demand.RealEstateType) &&
                (demand.MoreInformation.MinArea == 0 || (supply.RealEstate.MoreInformation.TotalArea >= demand.MoreInformation.MinArea)) &&
                (demand.MoreInformation.MaxArea == 0 || (supply.RealEstate.MoreInformation.TotalArea <= demand.MoreInformation.MaxArea)) &&
                (demand.MoreInformation.MinRooms == 0 || (supply.RealEstate.MoreInformation.Rooms >= demand.MoreInformation.MinRooms)) &&
                (demand.MoreInformation.MaxRooms == 0 || (supply.RealEstate.MoreInformation.Rooms <= demand.MoreInformation.MaxRooms)) &&
                (demand.MoreInformation.MinFloor == 0 || (supply.RealEstate.MoreInformation.Floor >= demand.MoreInformation.MinFloor)) &&
                (demand.MoreInformation.MaxFloor == 0 || (supply.RealEstate.MoreInformation.Floor <= demand.MoreInformation.MaxFloor)))
                suitableSupplies.Add(supply);
        }

        foreach (var supply in suitableSupplies)
        {
            string str = $"{supply.Id} {supply.RealEstate.Type} {supply.Cost}";
            ComboBoxCollectionOfSupplies.Add(str);
        }

        return suitableSupplies;
    }
}