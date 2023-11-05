using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DemoApplication.Infrastructure.Commands;
using DemoApplication.Infrastructure.DB;
using DemoApplication.Infrastructure.Stores;
using DemoApplication.Models;
using DemoApplication.Views.PageViews;
using MySqlConnector;
using ReactiveUI;

namespace DemoApplication.ViewModels.PageViewModels;

public class DealsViewModel : ViewModelBase
{
    public DealsViewModel()
    {
        GetAllDeals();

        #region Команды

        SaveDealCommand = new LambdaCommand(OnSaveDealCommandExecuted);
        DeleteDealCommand = new LambdaCommand(OnDeleteDealCommandExecuted);
        CancelDealEditionCommand = new LambdaCommand(OnCancelDealEditionCommandExecuted);

        #endregion
    }

    #region Коллекции

    private ObservableCollection<Deal> _deals = new ObservableCollection<Deal>();
    public ObservableCollection<Deal> Deals
    {
        get => _deals;
        set => this.RaiseAndSetIfChanged(ref _deals, value);
    }
    
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

    private ObservableCollection<string> _comboBoxCollection = new ObservableCollection<string>()
    {
        "Нет", "Информация"
    };
    public ObservableCollection<string> ComboBoxCollection
    {
        get => _comboBoxCollection;
        set => this.RaiseAndSetIfChanged(ref _comboBoxCollection, value);
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
            this.RaiseAndSetIfChanged(ref _selectedDemand, value);
            SelectedDeal.Demand = value;
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
            this.RaiseAndSetIfChanged(ref _selectedSupply, value);
            SelectedDeal.Supply = value;
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
    
    private Deal _oldSelectedDeal = new Deal();
    public Deal OldSelectedDeal
    {
        get => _oldSelectedDeal;
        set => this.RaiseAndSetIfChanged(ref _oldSelectedDeal, value);
    }
    
    private int _countOfSelections = 1;

    private Deal _selectedDeal;
    public Deal SelectedDeal
    {
        get => _selectedDeal;
        set
        {
            if (value == null)
            {
                this.RaiseAndSetIfChanged(ref _selectedDeal, value);
                return;
            }
            if (_countOfSelections == 1)
            {
                _countOfSelections++;
                this.RaiseAndSetIfChanged(ref _selectedDeal, value);
                OldSelectedDeal = GetSpecificDeal(_selectedDeal.Id);
                IsDealSelected = true;
                GetAllFreeDemands();
                GetAllFreeSupplies();
                Demands.Add(value.Demand);
                Supplies.Add(value.Supply);
                ComboBoxCollectionOfDemands.Add($"{value.Demand.Id} {value.Demand.RealEstateType} " +
                                                $"{value.Demand.MinCost}-{value.Demand.MaxCost}");
                ComboBoxCollectionOfSupplies.Add($"{value.Supply.Id} {value.Supply.RealEstate.Type} {value.Supply.Cost}");
                SelectedComboBoxSupply = $"{value.Supply.Id} {value.Supply.RealEstate.Type} " +
                                         $"{value.Supply.Cost}";
                SelectedComboBoxDemand = $"{value.Demand.Id} {value.Demand.RealEstateType} " +
                                          $"{value.Demand.MinCost}-{value.Demand.MaxCost}";
                if (SelectedSupply.RealEstate.Type == "Земля")
                    IsLandChosenInSupply = true;
                else
                {
                    IsLandChosenInSupply = false;
                }

                if (SelectedDemand.RealEstateType == "Земля")
                {
                    IsLandChosenInDemand = true;
                }
                else
                {
                    IsLandChosenInDemand = false;
                }
                return;
            }
            if (IsCancellingHappening)
            {
                IsDealSelected = false;
                SelectedComboBoxItem = "Нет";
                SelectedComboBoxSupply = "";
                SelectedComboBoxDemand = "";
                this.RaiseAndSetIfChanged(ref _selectedDeal, OldSelectedDeal);
                IsCancellingHappening = false;
                return;
            }
            OldSelectedDeal = GetSpecificDeal(SelectedDeal.Id);
            if (!IsTwoDealsEven(OldSelectedDeal, SelectedDeal))
                IsEditionSaved = false;
            if (IsEditionSaved == false)
            { 
                Console.WriteLine("Вы не сохранили изменения. Либо сохраните, либо отмените их");
            }
            else
            {
                this.RaiseAndSetIfChanged(ref _selectedDeal, value);
                OldSelectedDeal = GetSpecificDeal(SelectedDeal.Id);
                IsDealSelected = true;
                GetAllFreeDemands();
                GetAllFreeSupplies();
                Demands.Add(value.Demand);
                Supplies.Add(value.Supply);
                ComboBoxCollectionOfDemands.Add($"{value.Demand.Id} {value.Demand.RealEstateType} " +
                                                $"{value.Demand.MinCost}-{value.Demand.MaxCost}");
                ComboBoxCollectionOfSupplies.Add($"{value.Supply.Id} {value.Supply.RealEstate.Type} {value.Supply.Cost}");
                SelectedComboBoxSupply = $"{value.Supply.Id} {value.Supply.RealEstate.Type} " +
                                         $"{value.Supply.Cost}";
                SelectedComboBoxDemand = $"{value.Demand.Id} {value.Demand.RealEstateType} " +
                                         $"{value.Demand.MinCost}-{value.Demand.MaxCost}";
                if (SelectedSupply.RealEstate.Type == "Земля")
                    IsLandChosenInSupply = true;
                else
                {
                    IsLandChosenInSupply = false;
                }

                if (SelectedDemand.RealEstateType == "Земля")
                {
                    IsLandChosenInDemand = true;
                }
                else
                {
                    IsLandChosenInDemand = false;
                }
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
            }
            else if (value == "Информация")
            {
                IsUserEditingVisible = true;
            }
        }
    }

    private string _selectedComboBoxChoice;

    public string SelectedComboBoxChoice
    {
        get => _selectedComboBoxChoice;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedComboBoxChoice, value);
            if (value == "Предложение")
            {
                IsSupplyChosen = true;
            }
            else
            {
                IsSupplyChosen = false;
            }
        }
    }

    #endregion
    
    #region Контроль видимости и доступности

    private bool _isDealSelected = false;
    public bool IsDealSelected
    {
        get => _isDealSelected;
        set => this.RaiseAndSetIfChanged(ref _isDealSelected, value);
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
    
    #region Команды

    #region Команды изменения бд

    public ICommand SaveDealCommand { get; }
    public ICommand DeleteDealCommand { get; }
    public ICommand CancelDealEditionCommand { get; }

    private void OnSaveDealCommandExecuted(object parameter)
    {
        if (SelectedDeal.Demand == null || SelectedDeal.Supply == null)
        {
            Console.WriteLine("Вы не ввели все данные");
            return;
        }
        
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query1 = "update deal set " +
                            "DemandId = @demandId, " +
                            "SupplyId = @supplyId " +
                            "where id = @id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query1;

            cmd.Parameters.AddWithValue("@id", SelectedDeal.Id);
            cmd.Parameters.AddWithValue("@demandId", SelectedDeal.Demand.Id);
            cmd.Parameters.AddWithValue("@supplyId", SelectedDeal.Supply.Id);

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
    private void OnDeleteDealCommandExecuted(object parameter)
    {
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query1 = "delete from deal " +
                            "where id = @id;";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query1;
            cmd.Parameters.AddWithValue("@id", SelectedDeal.Id);
            
            cmd.ExecuteNonQuery();
            
            SelectedComboBoxItem = "Нет";
            SelectedComboBoxDemand = "";
            SelectedComboBoxSupply = "";
            SelectedComboBoxChoice = "";
            GetAllDeals();
            GetAllFreeSupplies();
            GetAllFreeSupplies();
            SelectedDeal = Deals[0];
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
    private void OnCancelDealEditionCommandExecuted(object parameter = null)
    {
        IsCancellingHappening = true;
        SelectedDeal = OldSelectedDeal;
        IsEditionSaved = true;
        GetAllDeals();
        GetAllFreeSupplies();
        GetAllFreeDemands();
        _countOfSelections = 1;
    }

    #endregion

    #endregion

    private void GetAllDeals()
    {
        Deals.Clear();
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT * from deal;";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;

            var reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Deal deal = new Deal()
                    {
                        Id = reader.GetInt32(0),
                        Demand = new Demand()
                        {
                            Id = reader.GetInt32(1)
                        },
                        Supply = new Supply()
                        {
                            Id = reader.GetInt32(2)
                        }
                    };
                    Deals.Add(deal);
                }
            }
            reader.Close();

            foreach (var deal in Deals)
            {
                deal.Supply = GetSpecificSupply(deal.Supply.Id);
                deal.Demand = GetSpecificDemand(deal.Demand.Id);
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
            OnPropertyChanged(nameof(Deals));
        }
    }
    
    private Deal GetSpecificDeal(int id)
    {
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT * from deal where deal.id = @id;";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@id", id);

            Deal deal = new Deal();

            var reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    deal = new Deal()
                    {
                        Id = reader.GetInt32(0),
                        Demand = new Demand()
                        {
                            Id = reader.GetInt32(1)
                        },
                        Supply = new Supply()
                        {
                            Id = reader.GetInt32(2)
                        }
                    };
                }
            }
            reader.Close();

            deal.Supply = GetSpecificSupply(deal.Supply.Id);
            deal.Demand = GetSpecificDemand(deal.Demand.Id);
                
            return deal;
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
            OnPropertyChanged(nameof(Deals));
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
    
    private bool IsTwoDealsEven(Deal deal1, Deal deal2)
    {
        if (deal1.Id != deal2.Id || deal1.Supply.Id != deal2.Supply.Id ||
            deal1.Demand.Id != deal2.Demand.Id)
            return false;
        return true;
    }
}