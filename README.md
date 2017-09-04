# GarudaUtil

[![License](https://img.shields.io/badge/license-APACHE-red.svg)](http://www.apache.org/licenses/LICENSE-2.0)

[![NuGet Pre Release](https://img.shields.io/nuget/vpre/Garuda.Data.svg)](https://www.nuget.org/packages/Garuda.Data/)

## Garuda.Data

A .NET assembly which provides System.Data interface implementations for [Apache Phoenix](https://phoenix.apache.org/)
 via the [Microsoft.Phoenix.Client](https://www.nuget.org/packages/Microsoft.Phoenix.Client/).

 Classes include:

 * PhoenixConnection implementing IDbConnection
 * PhoenixCommand implementing IDbCommand
 * PhoenixDataReader implementing IDataReader
 * PhoenixTransaction implementing IDbTransaction

### Connection string

The familiar connection string format is used for connections:

#### VNET / Apache Phoenix direct connections

```
Server=myphoenixserver.domain.com,8765;User ID=myuser;Password=mypwd;Request Timeout=30000 
```

#### HDInsight Gateway connections

```
# HDI 3.4
Server=https://mycluster.azurehdinsight.net/hbasephoenix0/;User ID=myuser;Password=mypwd;Mode=hdi-gateway;Request Timeout=30000

# HDI 3.6
Server=https://mycluster.azurehdinsight.net/hbasephoenix/;User ID=myuser;Password=mypwd;Mode=hdi-gateway;Request Timeout=30000
```

* Server: The DNS name of the Phoenix Query Server (for VNET mode, or standard HDP Phoenix/Hbase systems). 
** In HDInsight gateway mode, specify the complete URL. For HDI 3.4 this should include the worker node reference: hbasephoenixN where N specify the work node index. For example hbasephoenix0 is worker node 0. For HDI 3.6, simply hbasephoenix without the worker node reference.

* User ID: Your gateway credential user name. Only specify the User ID and Password when using HDInsight gateway mode.

* Password: Your gateway credential password.

* Request Timeout: The timeout in milliseconds of a given phoenix command or request to the phoenix server. I use 30000 in my tests and development.

* Mode: vnet or hdi-gateway. Use vnet (the default) for standard Apache Phoenix connections. Use hdi-gateway for HDInsight Hbase clusters when accessed externally.

### Example

 Refer the the GarudaUtil Program.cs file for a more complete example.

 ```{csharp}
using (IDbConnection phConn = new PhoenixConnection())
{
    phConn.ConnectionString = cmdLine.ConnectionString;

    phConn.Open();

    using (IDbCommand cmd = phConn.CreateCommand())
    {
        cmd.CommandText = "SELECT * FROM GARUDATEST";
        using (IDataReader reader = cmd.ExecuteReader())
        {
            while(reader.Read())
            {
                for(int i = 0; i < reader.FieldCount; i++)
                {
                    Console.WriteLine(string.Format("{0}: {1}", reader.GetName(i), reader.GetValue(i)));
                }
            }
        }
    }                        
}
```
 
### Transactions

At any given time, only a single transaction is supported on a given connection.  This appears to be a limitation of 
the Phoenix client or Phoenix itself (to be determined). Currently PhoenixConnection.BeginTransaction will allow creation
of > 1 transaction, but which ever is last active wins.

### Parameters

Support for parameters has been added. Refer to the Phoenix documentation for details on usage. Basically,
the question mark (?) and positional (:1) notations are supported:
 
```
UPSERT INTO GARUDATEST (ID, AircraftIcaoNumber) VALUES (NEXT VALUE FOR garuda.testsequence, :1)
```
 
The following data types have been tested successfully:

* string
* short
* int
* uint
* long
* float
* DateTime (timestamp)

### Known Issues / Deficiencies

Phoenix's [BINARY](https://phoenix.apache.org/language/datatypes.html#binary_type), VARBINARY and 
[ARRAY](https://phoenix.apache.org/array_type.html) data types are 
not yet handled. 

Phoenix's [UNSIGNED_LONG](https://phoenix.apache.org/language/datatypes.html#unsigned_long_type) is not supported. 
This appears to be an underlying issue with the Microsoft Phoenix Client's Avatica mapping using a long data types
for integer related data types. An issue/question needs to be opened with the [hdinsight-phoenix-sharp](https://github.com/Azure/hdinsight-phoenix-sharp)
project to get their input and understand possible resolutions.

The PhoenixDataReader GetByte, GetBytes, GetChar, GetChars, GetDecimal, GetGuid and 
GetEnumerator methods are not implemented yet.


## GarudaQuery

The solution also includes a Windows Forms-based user interface which uses Garuda.Data to interface
with the Phoenix Query Server. 

![Garuda Query](http://dwdii.github.io/img/GarudaQueryScreenshot.png)

## Related Links

Is there a way to connect to HBase using C#? 
https://community.hortonworks.com/questions/25101/is-there-a-way-to-connect-to-hbase-using-c.html

Use Apache Phoenix with Linux-based HBase clusters in HDinsight
https://azure.microsoft.com/en-us/documentation/articles/hdinsight-hbase-phoenix-squirrel-linux/

How to connect to HBase / Hadoop Database using C#
http://stackoverflow.com/questions/17866600/how-to-connect-to-hbase-hadoop-database-using-c-sharp/39217348#39217348


