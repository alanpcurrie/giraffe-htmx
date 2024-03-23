// Open namespaces to access types and functions from these libraries/modules.
open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Npgsql
open Dapper
open Giraffe.ViewEngine
open Microsoft.Extensions.Configuration

type Message = { Text: string }

type MasterPlan =
    { StartTimeUtc: string
      Duration: string
      Team: string }

let fetchMasterPlanData (configuration: IConfiguration) () =
    async {
        let connectionString = configuration.GetConnectionString("DefaultConnection")

        use connection = new NpgsqlConnection(connectionString)
        do! connection.OpenAsync() |> Async.AwaitIAsyncResult |> Async.Ignore

        let sql =
            """
                SELECT start_time_utc AS "StartTimeUtc", duration AS "Duration", team AS Team
                FROM master_plan
                LIMIT 1;
            """

        let! queryResult = connection.QueryAsync<MasterPlan>(sql) |> Async.AwaitTask
        return queryResult |> Seq.tryHead
    }


let jsonHandler (configuration: IConfiguration) : HttpHandler =
    fun next ctx ->
        task {
            let! maybeData = fetchMasterPlanData configuration () |> Async.StartAsTask

            match maybeData with
            | Some data -> return! json data next ctx
            | None -> return! RequestErrors.NOT_FOUND "Data not found" next ctx
        }





let layout pageTitle content =
    html
        []
        [ head
              []
              [ title [] [ encodedText pageTitle ]
                link [ _rel "stylesheet"; _href "https://unpkg.com/open-props" ]
                link [ _rel "stylesheet"; _href "/css/styles.css" ]
                script [ _src "https://unpkg.com/htmx.org@1.9.11"; _type "text/javascript" ] [] ]
          body
              []
              [ header
                    []
                    [ h1 [] [ encodedText "Giraffe HTMX" ]
                      img [ _src "/images/logo.WEBP"; _alt "Giraffe HTMX Logo" ] ]
                div
                    [ _class "container" ]
                    [ aside
                          [ _class "sidebar" ]
                          [ ul
                                []
                                [ li [] [ encodedText "Item 1" ]
                                  li [] [ encodedText "Item 2" ]
                                  li [] [ encodedText "Item 3" ] ] ]
                      button
                          [ _class "btn btn-primary"
                            attr "hx-get" "/api/data"
                            attr "hx-target" "#masterPlanData"
                            attr "hx-trigger" "click" ]
                          [ encodedText "Load Master Plan Data" ]
                      div [ _id "masterPlanData" ] []
                      main [ _class "main-content" ] content ]
                footer [] [ p [] [ encodedText "Footer content here..." ] ] ] ]



let partial () = h1 [] [ encodedText "backend" ]

let index (masterPlan: MasterPlan option) =
    let content =
        match masterPlan with
        | Some plan ->
            [ p [] [ encodedText $"Start Time: {plan.StartTimeUtc}" ]
              p [] [ encodedText $"Duration: {plan.Duration}" ]
              p [] [ encodedText $"Duration: {plan.Team}" ] ]

        | None -> [ p [] [ encodedText "No data found." ] ]

    layout "Master Plan Overview" content

let indexHandler (configuration: IConfiguration) (name: string) : HttpHandler =
    fun next ctx ->
        task {
            let! maybeData = fetchMasterPlanData configuration () |> Async.StartAsTask
            return! htmlView (index maybeData) next ctx
        }


let webApp (configuration: IConfiguration) =
    choose
        [ GET
          >=> choose
                  [ route "/" >=> indexHandler configuration "world"
                    route "/api/data" >=> jsonHandler configuration
                    routef "/hello/%s" (indexHandler configuration) ]
          setStatusCode 404 >=> text "Not Found" ]





let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

let configureCors (builder: CorsPolicyBuilder) =
    builder
        .WithOrigins(
            "http://localhost:5000",
            "https://localhost:5001",
            "https://localhost:3000",
            "https://localhost:4321",
            "https://giraffe-htmx.azurewebsites.net"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
    |> ignore

let configureApp (app: IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    let configuration = app.ApplicationServices.GetService<IConfiguration>()

    match env.IsDevelopment() with
    | true -> app.UseDeveloperExceptionPage() |> ignore
    | false ->
        app.UseGiraffeErrorHandler(errorHandler) |> ignore
        app.UseHttpsRedirection() |> ignore

    app.UseCors(configureCors) |> ignore
    app.UseStaticFiles() |> ignore
    app.UseGiraffe(webApp configuration) |> ignore


let configureServices (services: IServiceCollection) =
    services.AddCors() |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder: ILoggingBuilder) =
    builder.AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main args =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot = Path.Combine(contentRoot, "WebRoot")

    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .UseContentRoot(contentRoot)
                .UseWebRoot(webRoot)
                .Configure(configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
            |> ignore)
        .Build()
        .Run()

    0
