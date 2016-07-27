# GarudaUtil

[![License](https://img.shields.io/badge/license-APACHE-red.svg)](http://www.apache.org/licenses/LICENSE-2.0)

[![NuGet Pre Release](https://img.shields.io/nuget/vpre/Garuda.Data.svg?maxAge=2592000)]()

## Garuda.Data

A .NET assembly which provides System.Data interface implementations for [Apache Phoenix](https://phoenix.apache.org/)
 via the [Microsoft.Phoenix.Client](https://www.nuget.org/packages/Microsoft.Phoenix.Client/).

 Classes include:

 * PhoenixConnection implementing IDbConnection
 * PhoenixCommand implementing IDbCommand
 * PhoenixDataReader implementing IDbDataReader

### Connection string

The familiar connection string format is used for connections:

```
Server=myphoenixserver.domain.com,8765;User ID=myuser;Password=mypwd;CredentialUri=http://myazurecredurl;Request Timeout=30000" 
```

* Credentials are only used by the Microsoft.Phoenix.Client in gateway-mode (Azure), which requires port 443.
* Request Timeout is in milliseconds.

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
 