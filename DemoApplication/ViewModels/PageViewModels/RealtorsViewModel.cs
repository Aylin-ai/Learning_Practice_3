using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DemoApplication.Infrastructure.Commands;
using DemoApplication.Infrastructure.DB;
using DemoApplication.Models;
using MySqlConnector;
using ReactiveUI;

namespace DemoApplication.ViewModels.PageViewModels;

public class RealtorsViewModel : ViewModelBase
{
    public RealtorsViewModel()
    {
        GetAllRealtors();

        #region Команды

        SaveRealtorCommand = new LambdaCommand(OnSaveRealtorCommandExecuted);
        DeleteRealtorCommand = new LambdaCommand(OnDeleteRealtorCommandExecuted);
        CancelRealtorEditionCommand = new LambdaCommand(OnCancelRealtorEditionCommandExecuted);

        #endregion
    }

    #region Коллекции

    private ObservableCollection<Realtor> _realtors = new ObservableCollection<Realtor>();
    public ObservableCollection<Realtor> Realtors
    {
        get => _realtors;
        set => this.RaiseAndSetIfChanged(ref _realtors, value);
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
    
    private ObservableCollection<Demand> _selectedRealtorDemands = new ObservableCollection<Demand>();
    public ObservableCollection<Demand> SelectedRealtorDemands
    {
        get => _selectedRealtorDemands;
        set => this.RaiseAndSetIfChanged(ref _selectedRealtorDemands, value);
    }
    
    private ObservableCollection<Supply> _selectedRealtorSupplies = new ObservableCollection<Supply>();
    public ObservableCollection<Supply> SelectedRealtorSupplies
    {
        get => _selectedRealtorSupplies;
        set => this.RaiseAndSetIfChanged(ref _selectedRealtorSupplies, value);
    }

    #endregion

    #region Выбираемые переменные

    private Realtor _oldSelectedRealtor = new Realtor();
    public Realtor OldSelectedRealtor
    {
        get => _oldSelectedRealtor;
        set => this.RaiseAndSetIfChanged(ref _oldSelectedRealtor, value);
    }

    private int _countOfSelections = 1;
    
    private Realtor _selectedRealtor;
    public Realtor SelectedRealtor
    {
        get => _selectedRealtor;
        set
        {
            if (value == null)
            {
                this.RaiseAndSetIfChanged(ref _selectedRealtor, value);
                return;
            }
            if (_countOfSelections == 1)
            {
                this.RaiseAndSetIfChanged(ref _selectedRealtor, value);
                _countOfSelections++;
                OldSelectedRealtor = GetSpecificRealtor(SelectedRealtor.Id);
                IsRealtorSelected = true;
                SelectedRealtorDemands.Clear();
                SelectedRealtorSupplies.Clear();
                SelectedComboBoxItem = "Нет";
                GetSpecificRealtorSupplies(SelectedRealtor.Id);
                GetSpecificRealtorDemands(SelectedRealtor.Id);
                return;
            }
            if (IsCancellingHappening)
            {
                SelectedRealtorDemands.Clear();
                SelectedRealtorSupplies.Clear();
                SelectedComboBoxItem = "Нет";
                this.RaiseAndSetIfChanged(ref _selectedRealtor, OldSelectedRealtor);
                IsCancellingHappening = false;
                return;
            }
            OldSelectedRealtor = GetSpecificRealtor(SelectedRealtor.Id);
            if (!IsTwoRealtorsEven(OldSelectedRealtor, SelectedRealtor))
                IsEditionSaved = false;
            if (IsEditionSaved == false)
            { 
                Console.WriteLine("Вы не сохранили изменения. Либо сохраните, либо отмените их");
            }
            else
            {
                this.RaiseAndSetIfChanged(ref _selectedRealtor, value);
                IsRealtorSelected = true;
                SelectedRealtorDemands.Clear();
                SelectedRealtorSupplies.Clear();
                SelectedComboBoxItem = "Нет";
                GetSpecificRealtorSupplies(SelectedRealtor.Id);
                GetSpecificRealtorDemands(SelectedRealtor.Id);
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
    
    private bool _isRealtorSelected = false;
    public bool IsRealtorSelected
    {
        get => _isRealtorSelected;
        set => this.RaiseAndSetIfChanged(ref _isRealtorSelected, value);
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

    #endregion
    
    #region Команды

    #region Команды изменения бд

    public ICommand SaveRealtorCommand { get; }
    public ICommand DeleteRealtorCommand { get; }
    public ICommand CancelRealtorEditionCommand { get; }

    private void OnSaveRealtorCommandExecuted(object parameter)
    {
        if ((SelectedRealtor.FirstName == "" || SelectedRealtor.FirstName == "Нет") && 
            (SelectedRealtor.SecondName == "" || SelectedRealtor.SecondName == "Нет") && 
            (SelectedRealtor.LastName == "" || SelectedRealtor.LastName == "Нет") && 
            (SelectedRealtor.Share < 0 || SelectedRealtor.Share > 100))
        {
            Console.WriteLine("Введите все данные");
            OnCancelRealtorEditionCommandExecuted();
        }
        
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query1 = "update realtor set " +
                            "FirstName = @firstName, " +
                            "SecondName = @secondName, " +
                            "LastName = @lastName, " +
                            "Share = @share " +
                            "where id = @id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query1;

            cmd.Parameters.AddWithValue("@id", SelectedRealtor.Id);
            cmd.Parameters.AddWithValue("@firstName", SelectedRealtor.FirstName);
            cmd.Parameters.AddWithValue("@secondName", SelectedRealtor.SecondName);
            cmd.Parameters.AddWithValue("@lastName", SelectedRealtor.LastName);
            cmd.Parameters.AddWithValue("@share", SelectedRealtor.Share);

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
    private void OnDeleteRealtorCommandExecuted(object parameter)
    {
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query1 = "select * from supply " +
                            "where RealtorId = @id;";
            string query2 = "select * from demand " +
                            "where RealtorId = @id;";
            string query3 = "delete from realtor " +
                            "where id = @id;";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query1;
            cmd.Parameters.AddWithValue("@id", SelectedRealtor.Id);

            var reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Console.WriteLine($"Невозможно удалить риелтора, " +
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
                    Console.WriteLine($"Невозможно удалить риелтора, " +
                                      $"участвующего в потребности №{reader.GetInt32(0)}");
                    return;
                }
            }
            reader.Close();

            cmd.CommandText = query3;
            cmd.ExecuteNonQuery();
            SelectedRealtor = null;
            SelectedComboBoxItem = "Нет";
            GetAllRealtors();
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
    private void OnCancelRealtorEditionCommandExecuted(object parameter = null)
    {
        IsCancellingHappening = true;
        SelectedRealtor = OldSelectedRealtor;
        IsEditionSaved = true;
        GetAllRealtors();
        _countOfSelections = 1;
    }

    #endregion

    #endregion

    private void GetAllRealtors()
    {
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
    
    private void GetSpecificRealtorDemands(int id)
    {
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT * from demand where RealtorId = @realtorId;";

            string query1 = "Select * from client where id = @clientId;";
            string query2 = "Select * from realtor where id = @realtorId;";
            
            string checkQuery1 = "Select * from apartmentDemand where DemandId = @id";
            string checkQuery2 = "Select * from houseDemand where DemandId = @id";
            string checkQuery3 = "Select * from landDemand where DemandId = @id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@realtorId", id);

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
                    };
                    string[] address = reader.GetString(4).Split(',');
                    demand.Address = new Address()
                    {
                        City = address[0] == "" ? "Нет" : address[0],
                        Street = address[1] == "" ? "Нет" : address[1],
                        House = address[2] == "" ? "Нет" : address[2],
                        Apartment = address[3] == "" ? 0 : int.Parse(address[3])
                    };
                    SelectedRealtorDemands.Add(demand);
                }
            }
            reader.Close();

            foreach (var demand in SelectedRealtorDemands)
            {
                MySqlCommand checkComand = new MySqlCommand();
                checkComand.Connection = connection;
                checkComand.CommandText = query1;
                
                checkComand.Parameters.AddWithValue("@clientId", demand.Client.Id); 
                checkComand.Parameters.AddWithValue("@realtorId", id); 
                
                var readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read()) 
                    { 
                        demand.Client.Id = readerInside.IsDBNull(0) ? 0 : readerInside.GetInt32(0);
                        demand.Client.FirstName = readerInside.IsDBNull(1) ? "Нет" : readerInside.GetString(1);
                        demand.Client.SecondName = readerInside.IsDBNull(2) ? "Нет" : readerInside.GetString(2);
                        demand.Client.LastName = readerInside.IsDBNull(3) ? "Нет" :readerInside.GetString(3);
                        demand.Client.Phone = readerInside.IsDBNull(4) ? "Нет" :readerInside.GetString(4);
                        demand.Client.Email = readerInside.IsDBNull(5) ? "Нет" :readerInside.GetString(5);
                    }
                }
                readerInside.Close();

                checkComand.CommandText = query2;
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read())
                    {
                        demand.Realtor.Id = readerInside.IsDBNull(0) ? 0 : readerInside.GetInt32(0);
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
            OnPropertyChanged(nameof(SelectedRealtorDemands));
        }
    }
    
    private void GetSpecificRealtorSupplies(int id)
    {
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT * from supply where RealtorId = @realtorId;";

            string query1 = "Select * from client where id = @clientId;";
            string query2 = "Select * from realtor where id = @realtorId;";
            string query3 = "Select * from address where RealEstateId = @realEstateId;";
            
            string checkQuery1 = "Select * from apartment where RealEstateId = @id";
            string checkQuery2 = "Select * from house where RealEstateId = @id";
            string checkQuery3 = "Select * from land where RealEstateId = @id";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@realtorId", id); 

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
                        Realtor = new Realtor(),
                        RealEstate = new RealEstate()
                        {
                            Id = reader.GetInt32(4),
                            Type = ""
                        }
                    };
                    SelectedRealtorSupplies.Add(supply);
                }
            }
            reader.Close();

            foreach (var supply in SelectedRealtorSupplies)
            {
                MySqlCommand checkComand = new MySqlCommand();
                checkComand.Connection = connection;
                checkComand.CommandText = query1;
                
                checkComand.Parameters.AddWithValue("@clientId", supply.Client.Id); 
                checkComand.Parameters.AddWithValue("@realtorId", id); 
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
                        supply.Client.Email = readerInside.IsDBNull(5) ? "Нет" :readerInside.GetString(5);
                    }
                }
                readerInside.Close();

                checkComand.CommandText = query2;
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read()) 
                    { 
                        supply.Realtor.Id = readerInside.IsDBNull(0) ? 0 : readerInside.GetInt32(0);
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
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            connection.Dispose();
            connection.Close();
            OnPropertyChanged(nameof(SelectedRealtorSupplies));
        }
    }
    
    private Realtor GetSpecificRealtor(int id)
    {
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "select * from realtor where id = @id;";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@id", id);

            var reader = cmd.ExecuteReader();

            Realtor realtor = new Realtor();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    realtor = new Realtor()
                    {
                        Id = reader.GetInt32(0),
                        FirstName = reader.IsDBNull(1) ? "Нет" : reader.GetString(1),
                        SecondName = reader.IsDBNull(2) ? "Нет" : reader.GetString(2),
                        LastName = reader.IsDBNull(3) ? "Нет" : reader.GetString(3),
                        Share = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                    };
                }
            }

            return realtor;
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
    
    private bool IsTwoRealtorsEven(Realtor realtor1, Realtor realtor2)
    {
        if (realtor1.FirstName != realtor2.FirstName ||
            realtor1.SecondName != realtor2.SecondName ||
            realtor1.LastName != realtor2.LastName ||
            realtor1.Share != realtor2.Share ||
            realtor1.Id != realtor2.Id)
            return false;
        return true;
    }
}