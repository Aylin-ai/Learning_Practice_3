using System;
using System.Collections.ObjectModel;
using System.Data;
using DemoApplication.Infrastructure.DB;
using DemoApplication.Infrastructure.Stores;
using DemoApplication.Models;
using MySqlConnector;
using ReactiveUI;

namespace DemoApplication.ViewModels.PageViewModels;

public class DemandsViewModel : ViewModelBase
{
    public DemandsViewModel()
    {
        GetAllDemands();
    }
    
    private ObservableCollection<Demand> _demands = new ObservableCollection<Demand>();
    public ObservableCollection<Demand> Demands
    {
        get => _demands;
        set => this.RaiseAndSetIfChanged(ref _demands, value);
    }

    private void GetAllDemands()
    {
        Demands.Clear();
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT * from demand;";

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
}