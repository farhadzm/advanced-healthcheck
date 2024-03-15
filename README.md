Implementing advanced health check
```CSharp
services
    .AddHealthChecks()
    .AddRedis(redisConnection, "redis")
    .AddSqlServer(new SqlServerHealthCheckOptions
    {
        ConnectionString = sqlConnection,
    })
    .AddTcpHealthCheck(option =>
    {
        var uri = new Uri(externalServiceUri);
        option.AddHost(uri.Host, uri.Port);
    })
    .AddCheck<PingApiHealthCheck>("PingApi");

```
Response
```json
{
    "status": "Healthy",
    "duration": "00:00:00.5265831",
    "info":
    [
        {
            "key": "redis",
            "duration": "00:00:00.4325025",
            "status": "Healthy",
            "data":
            {}
        },
        {
            "key": "sqlserver",
            "duration": "00:00:00.4965963",
            "status": "Healthy",
            "data":
            {}
        },
        {
            "key": "tcp",
            "duration": "00:00:00.1144368",
            "status": "Healthy",
            "data":
            {}
        },
        {
            "key": "PingApi",
            "duration": "00:00:00.3450499",
            "status": "Healthy",
            "data":
            {}
        }
    ]
}
```
