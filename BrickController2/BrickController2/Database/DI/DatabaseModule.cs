using Autofac;
using BrickController2.Helpers;
using SQLite;

namespace BrickController2.Database.DI
{
    public class DatabaseModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SQLiteConnection>().WithParameter(new TypedParameter(typeof(string), "databasePath"));
            builder.RegisterType<SQLiteAsyncConnection>().WithParameter(new TypedParameter(typeof(string), "databasePath"));

            builder.Register<SQLiteConnectionFactory>(componentContext =>
            {
                return (databaseFilename) =>
                {
                    var databasePath = PathHelper.AddAppDataPathToFilename(databaseFilename);
                    return componentContext.Resolve<SQLiteConnection>(new TypedParameter(typeof(string), databasePath));
                };
            });

            builder.Register<SQLiteAsyncConnectionFactory>(componentContext =>
            {
                return (databaseFilename) =>
                {
                    var databasePath = PathHelper.AddAppDataPathToFilename(databaseFilename);
                    return componentContext.Resolve<SQLiteAsyncConnection>(new TypedParameter(typeof(string), databasePath));
                };
            });
        }
    }
}
