using System;
using System.Collections.ObjectModel;
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
    }
    
    private ObservableCollection<RealEstate> _realEstates = new ObservableCollection<RealEstate>();
    public ObservableCollection<RealEstate> RealEstates
    {
        get => _realEstates;
        set => this.RaiseAndSetIfChanged(ref _realEstates, value);
    }

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
}