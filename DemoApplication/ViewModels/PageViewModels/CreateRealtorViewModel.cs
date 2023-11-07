using System;
using System.Windows.Input;
using DemoApplication.Infrastructure.Commands;
using DemoApplication.Infrastructure.DB;
using DemoApplication.Models;
using MySqlConnector;
using ReactiveUI;

namespace DemoApplication.ViewModels.PageViewModels;

public class CreateRealtorViewModel : ViewModelBase
{
    public CreateRealtorViewModel()
    {
        SaveRealtorCommand = new LambdaCommand(OnSaveRealtorCommandExecuted);
    }
    
    private Realtor _realtor = new Realtor();
    public Realtor Realtor
    {
        get => _realtor;
        set => this.RaiseAndSetIfChanged(ref _realtor, value);
    }
    
    #region Команды

    #region Команды изменения бд

    public ICommand SaveRealtorCommand { get; }

    private void OnSaveRealtorCommandExecuted(object parameter)
    {
        if ((Realtor.FirstName == "" || Realtor.FirstName == "Нет") && 
            (Realtor.SecondName == "" || Realtor.SecondName == "Нет") && 
            (Realtor.LastName == "" || Realtor.LastName == "Нет") && 
            (Realtor.Share < 0 || Realtor.Share > 100))
        {
            Console.WriteLine("Введите все данные");
        }
        
        MySqlConnection connection = DBUtils.GetDBConnection();

        try
        {
            connection.Open();
            string query1 = "insert into realtor (FirstName, SecondName, LastName, Share) values " +
                            "(@firstName, @secondName, @lastName, @share)";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = query1;

            cmd.Parameters.AddWithValue("@id", Realtor.Id);
            cmd.Parameters.AddWithValue("@firstName", Realtor.FirstName);
            cmd.Parameters.AddWithValue("@secondName", Realtor.SecondName);
            cmd.Parameters.AddWithValue("@lastName", Realtor.LastName);
            cmd.Parameters.AddWithValue("@share", Realtor.Share == 0 ? 45 : Realtor.Share);

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