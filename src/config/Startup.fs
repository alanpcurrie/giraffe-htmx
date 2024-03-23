let configureServices (services: IServiceCollection) =
    services.AddGiraffe() |> ignore

    services.AddSingleton<IConfiguration>(fun serviceProvider ->
        let env = serviceProvider.GetRequiredService<IWebHostEnvironment>()
        let builder = new ConfigurationBuilder()

        builder
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional = false, reloadOnChange = true)
            .AddJsonFile(sprintf "appsettings.%s.json" env.EnvironmentName, optional = true)
            .AddEnvironmentVariables()
        |> ignore

        builder.Build())
    |> ignore
