using SQLite;

namespace BrickController2.Database
{
    public delegate SQLiteConnection SQLiteConnectionFactory(string databaseFileNama);
    public delegate SQLiteAsyncConnection SQLiteAsyncConnectionFactory(string databaseFileName);
}
