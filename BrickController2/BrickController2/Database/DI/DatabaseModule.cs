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

            builder.Register<SQLiteConnectionFactory>(c =>
            {
                var componentContext = c.Resolve<IComponentContext>();
                return (databaseFilename) =>
                {
                    var databasePath = PathHelper.AddAppDataPathToFilename(databaseFilename);
                    return componentContext.Resolve<SQLiteConnection>(new TypedParameter(typeof(string), databasePath));
                };
            });

            builder.Register<SQLiteAsyncConnectionFactory>(c =>
            {
                var componentContext = c.Resolve<IComponentContext>();
                return (databaseFilename) =>
                {
                    var databasePath = PathHelper.AddAppDataPathToFilename(databaseFilename);
                    return componentContext.Resolve<SQLiteAsyncConnection>(new TypedParameter(typeof(string), databasePath));
                };
            });
        }
    }
}
