﻿namespace Dashing.Tools.Tests.Migration {
    using System;
    using System.Configuration;

    using Dashing.Tools.Migration;

    using Xunit;

    public class ConnectionStringManipulatorTests {
        [Fact]
        public void RootConnectionStringWorks() {
            var connectionStrings = new[] {
                                              "Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;",
                                              "Server=myServerAddress;Uid=myUsername;Pwd=myPassword;",
                                              "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;",
                                              "Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;",
                                              "Data Source=myServerAddress;Failover Partner=myMirrorServerAddress;Initial Catalog=myDataBase;Integrated Security=True;",
                                              @"Data Source=myServerAddress;Integrated Security=SSPI;User ID=myDomain\myUsername;Password=myPassword;"
                                          };
            var correctResults = new[] {
                                           "server=myServerAddress;user id=myUsername;password=myPassword",
                                           "server=myServerAddress;user id=myUsername;password=myPassword",
                                           "Data Source=myServerAddress;User ID=myUsername;Password=myPassword",
                                           "Data Source=myServerAddress;Integrated Security=True",
                                           "Data Source=myServerAddress;Failover Partner=myMirrorServerAddress;Integrated Security=True",
                                           @"Data Source=myServerAddress;Integrated Security=True;User ID=myDomain\myUsername;Password=myPassword"
                                       };
            var providers = new[] {
                                      "MySql.Data.MySqlClient", "MySql.Data.MySqlClient", "System.Data.SqlClient", "System.Data.SqlClient",
                                      "System.Data.SqlClient", "System.Data.SqlClient"
                                  };
            for (var i = 0; i < connectionStrings.Length; i++) {
                var connectionStringManipulator =
                    new ConnectionStringManipulator(new ConnectionStringSettings("Default", connectionStrings[i], providers[i]));
                Assert.Equal(connectionStringManipulator.GetRootConnectionString().ConnectionString, correctResults[i]);
            }
        }

        [Fact]
        public void GetDatabaseNameWorks() {
            var connectionStrings = new[] {
                                              "Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;",
                                              "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;",
                                              "Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;",
                                              "Data Source=myServerAddress;Failover Partner=myMirrorServerAddress;Initial Catalog=myDataBase;Integrated Security=True;"
                                          };
            var providers = new[] {
                                      "MySql.Data.MySqlClient", "MySql.Data.MySqlClient", "System.Data.SqlClient", "System.Data.SqlClient",
                                      "System.Data.SqlClient", "System.Data.SqlClient"
                                  };
            for (var i = 0; i < connectionStrings.Length; i++) {
                var connectionStringManipulator =
                    new ConnectionStringManipulator(new ConnectionStringSettings("Default", connectionStrings[i], providers[i]));
                Assert.Equal(connectionStringManipulator.GetDatabaseName(), "myDataBase");
            }
        }

        [Fact]
        public void NoDatabaseNameThrows() {
            var connectionStrings = new[] {
                                              "Server=myServerAddress;Uid=myUsername;Pwd=myPassword;",
                                              @"Data Source=myServerAddress;Integrated Security=SSPI;User ID=myDomain\myUsername;Password=myPassword;"
                                          };
            var providers = new[] {
                                      "MySql.Data.MySqlClient", "System.Data.SqlClient"
                                  };
            for (var i = 0; i < connectionStrings.Length; i++) {
                var connectionStringManipulator =
                    new ConnectionStringManipulator(new ConnectionStringSettings("Default", connectionStrings[i], providers[i]));
                Assert.Throws<NotSupportedException>(() => connectionStringManipulator.GetDatabaseName());
            }
        }
    }
}