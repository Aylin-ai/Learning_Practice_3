using System;
using System.Windows.Input;
using DemoApplication.Infrastructure.Commands;
using DemoApplication.Infrastructure.DB;
using DemoApplication.Models;
using MySqlConnector;
using ReactiveUI;

namespace DemoApplication.ViewModels.PageViewModels;

public class CreateClientViewModel : ViewModelBase
{
    public CreateClientViewModel()
    {
        SaveClientCommand = new LambdaCommand(OnSaveClientCommandExecuted);
    }

    private Client _client = new Client();
    public Client Client
    {
        get => _client;
        set => this.RaiseAndSetIfChanged(ref _client, value);
    }
    
    
    #region Команды

    #region Команды изменения бд

    public ICommand SaveClientCommand { get; }
    
    private void OnSaveClientCommandExecuted(object parameter)
    {
        if ((Client.Email == "" || Client.Email == "Нет") && 
            (Client.Phone == "" || Client.Phone == "Нет"))
        {
            Console.WriteLine("Введите телефон или email");
        }
        
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query1 = "insert into client (FirstName, SecondName, LastName, Phone, Email) " +
                            "values (@firstName, @secondName, @lastName, @phone, @email)";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query1;

            cmd.Parameters.AddWithValue("@firstName", Client.FirstName == "" ? "Нет" : Client.FirstName);
            cmd.Parameters.AddWithValue("@secondName", Client.SecondName == "" ? "Нет" : Client.SecondName);
            cmd.Parameters.AddWithValue("@lastName", Client.LastName == "" ? "Нет" : Client.LastName);
            cmd.Parameters.AddWithValue("@phone", Client.Phone == "" ? "Нет" : Client.Phone);
            cmd.Parameters.AddWithValue("@email", Client.Email == "" ? "Нет" : Client.Email);

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