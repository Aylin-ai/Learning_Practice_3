using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DemoApplication.Infrastructure.Commands;
using DemoApplication.Infrastructure.DB;
using DemoApplication.Models;
using MySqlConnector;
using ReactiveUI;

namespace DemoApplication.ViewModels.PageViewModels;

public class CreateRealEstateViewModel : ViewModelBase
{
    public CreateRealEstateViewModel()
    {
        SaveRealEstateCommand = new LambdaCommand(OnSaveRealEstateCommandExecuted);
    }

    private RealEstate _realEstate = new RealEstate()
    {
        Address = new Address(),
        MoreInformation = new RealEstateMoreInformation()
    };
    public RealEstate RealEstate
    {
        get => _realEstate;
        set => this.RaiseAndSetIfChanged(ref _realEstate, value);
    }
    
    #region Коллекции
    
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
    
    private string _selectedComboBoxRealEstateType = "Квартира";
    public string SelectedComboBoxRealEstateType
    {
        get => _selectedComboBoxRealEstateType;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedComboBoxRealEstateType, value);
            RealEstate.Type = value;
            if (value == "Земля")
                IsLandChosen = true;
            else
                IsLandChosen = false;
        }
    }
    

    #endregion

    #region Контроль видимости
    
    private bool _isLandChosen;
    public bool IsLandChosen
    {
        get => _isLandChosen;
        set => this.RaiseAndSetIfChanged(ref _isLandChosen, value);
    }

    #endregion
    
    #region Команды

    #region Команды изменения бд

    public ICommand SaveRealEstateCommand { get; }

    private void OnSaveRealEstateCommandExecuted(object parameter)
    {
        if (RealEstate.Coordinates.Latitude > 90 ||
            RealEstate.Coordinates.Latitude < -90 ||
            RealEstate.Coordinates.Longitude > 180 ||
            RealEstate.Coordinates.Longitude < -180)
        {
            Console.WriteLine("Широта или долгота приняли некорректное значение");
            return;
        }
        
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();

            string query = "insert into realEstate values (null);";
            string query0 = "select id from realEstate order by id desc limit 1;";
            
            string query1 = "insert into address values (@id, " +
                            "@newCity, @newStreet, @newHouse, " +
                            "@newApartment);";
            string query2 = "insert into coordinates (@id, " +
                            "@newLatitude, Longitude = @newLongitude)";
            string query3 = "";
            if (RealEstate.Type == "Квартира")
                query3 = "insert into apartment values (@id, " +
                         "@newFloor, @newRooms, @newTotalArea);";
            else if (RealEstate.Type == "Дом")
            {
                query3 = "insert into house values (@id, " +
                         "@newFloor, @newRooms, @newTotalArea);";
            }
            else if (RealEstate.Type == "Земля")
            {
                query3 = "insert into land values (@id, @newTotalArea);";
            }

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();
            
            cmd.CommandText = query0;

            int id = 0;

            var reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    id = reader.GetInt32(0);
                }
            }
            reader.Close();

            cmd.CommandText = query1;

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@newCity", RealEstate.Address.City);
            cmd.Parameters.AddWithValue("@newStreet", RealEstate.Address.Street);
            cmd.Parameters.AddWithValue("@newHouse", RealEstate.Address.House);
            cmd.Parameters.AddWithValue("@newApartment", RealEstate.Address.Apartment);
            cmd.Parameters.AddWithValue("@newLatitude", RealEstate.Coordinates.Latitude);
            cmd.Parameters.AddWithValue("@newLongitude", RealEstate.Coordinates.Longitude);
            cmd.Parameters.AddWithValue("@newFloor", RealEstate.MoreInformation.Floor);
            cmd.Parameters.AddWithValue("@newRooms", RealEstate.MoreInformation.Rooms);
            cmd.Parameters.AddWithValue("@newTotalArea", RealEstate.MoreInformation.TotalArea);

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

    #endregion

    #endregion
}