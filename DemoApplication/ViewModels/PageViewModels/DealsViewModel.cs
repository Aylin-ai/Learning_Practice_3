using System;
using System.Collections.ObjectModel;
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
    }

    private ObservableCollection<Deal> _deals = new ObservableCollection<Deal>();
    public ObservableCollection<Deal> Deals
    {
        get => _deals;
        set => this.RaiseAndSetIfChanged(ref _deals, value);
    }

    private void GetAllDeals()
    {
        Deals.Clear();
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query = "SELECT * from deal;";
            
            string query1 = "SELECT client.FirstName, client.SecondName, client.LastName, " +
                            "realtor.FirstName, realtor.SecondName, realtor.LastName, realtor.Share, demand.RealEstateType " +
                            "from demand INNER JOIN client " +
                            "on demand.CLientId = client.id " +
                            "INNER JOIN realtor " +
                            "on demand.RealtorId = realtor.id " +
                            "WHERE demand.id = @demandId;";
            string query2 = "SELECT client.FirstName, client.SecondName, client.LastName, " +
                            "realtor.FirstName, realtor.SecondName, realtor.LastName, realtor.Share, " +
                            "supply.Cost, address.City, address.Street, address.House, address.Apartment " +
                            "from supply inner join client " +
                            "on supply.ClientId = client.id " +
                            "inner join realtor " +
                            "on supply.RealtorId = realtor.id " +
                            "inner join address " +
                            "on supply.RealEstateId = address.RealEstateId " +
                            "where supply.id = @supplyId;";

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
                MySqlCommand checkComand = new MySqlCommand();
                checkComand.Connection = connection;
                checkComand.CommandText = query1;
                
                checkComand.Parameters.AddWithValue("@demandId", deal.Demand.Id); 
                checkComand.Parameters.AddWithValue("@supplyId", deal.Supply.Id); 
                
                var readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read())
                    {
                        deal.Demand.Client = new Client()
                        {
                            FirstName = readerInside.IsDBNull(0) ? "Нет" : readerInside.GetString(0),
                            SecondName = readerInside.IsDBNull(1) ? "Нет" : readerInside.GetString(1),
                            LastName = readerInside.IsDBNull(2) ? "Нет" : readerInside.GetString(2)
                        };
                        deal.Demand.Realtor = new Realtor()
                        {
                            FirstName = readerInside.IsDBNull(3) ? "Нет" : readerInside.GetString(3),
                            SecondName = readerInside.IsDBNull(4) ? "Нет" : readerInside.GetString(4),
                            LastName = readerInside.IsDBNull(5) ? "Нет" : readerInside.GetString(5),
                            Share = readerInside.IsDBNull(6) ? 0 : readerInside.GetInt32(6)
                        };
                        
                        string type = "";
                        if (reader.GetInt32(7) == 1) type = "Квартира";
                        else if (reader.GetInt32(7) == 2) type = "Дом";
                        else type = "Земля";

                        deal.Supply.RealEstate = new RealEstate()
                        {
                            Type = type
                        };
                    }
                } 
                readerInside.Close();
                
                checkComand.CommandText = query2; 
                readerInside = checkComand.ExecuteReader(); 
                if (readerInside.HasRows) 
                { 
                    while (readerInside.Read())
                    {
                        deal.Supply.Client = new Client()
                        {
                            FirstName = readerInside.IsDBNull(0) ? "Нет" : readerInside.GetString(0),
                            SecondName = readerInside.IsDBNull(1) ? "Нет" : readerInside.GetString(1),
                            LastName = readerInside.IsDBNull(2) ? "Нет" : readerInside.GetString(2)
                        };
                        deal.Supply.Realtor = new Realtor()
                        {
                            FirstName = readerInside.IsDBNull(3) ? "Нет" : readerInside.GetString(3),
                            SecondName = readerInside.IsDBNull(4) ? "Нет" : readerInside.GetString(4),
                            LastName = readerInside.IsDBNull(5) ? "Нет" : readerInside.GetString(5),
                            Share = readerInside.IsDBNull(6) ? 0 : readerInside.GetInt32(6)
                        };
                        deal.Supply.Cost = readerInside.GetInt32(7);
                        deal.Supply.RealEstate.Address = new Address()
                        {
                            City = readerInside.IsDBNull(8) ? "Нет" : readerInside.GetString(8),
                            Street = readerInside.IsDBNull(9) ? "Нет" : readerInside.GetString(9),
                            House = readerInside.IsDBNull(10) ? "Нет" : readerInside.GetString(10),
                            Apartment = readerInside.IsDBNull(11) ? 0 : readerInside.GetInt32(11)
                        };
                    }
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
            OnPropertyChanged(nameof(Deals));
        }
    }
}