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
    
    private ObservableCollection<RealEstate> _realEstates = new ObservableCollection<RealEstate>();
    public ObservableCollection<RealEstate> RealEstates
    {
        get => _realEstates;
        set => this.RaiseAndSetIfChanged(ref _realEstates, value);
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
            if (countOfSelections == 1)
            {
                this.RaiseAndSetIfChanged(ref _selectedEstate, value);
                countOfSelections++;
                OldSelectedRealEstate = GetSpecificRealEstate(SelectedEstate.Id);
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
            }
        }
    }

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
            OnCancelRealEstateEditionCommandExecuted();
        }
        
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
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