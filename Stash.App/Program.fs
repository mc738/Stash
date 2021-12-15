open System
open Freql.Sqlite
open Stash
open ToolBox.AppEnvironment
open ToolBox.Core
open ToolBox.Core.Mapping
open ToolBox.Core.ConsoleIO

let iv = Encryption.getCryptoBytes 16

type StashConfiguration = { KeyPath: string }

type KeyGenOptions =
    { [<ArgValue("-o", "--output")>]
      OutputPath: string }

type HttpOptions =
    { [<ArgValue("-c", "--config")>]
      ConfigPath: string }

type GetOptions =
    { [<ArgValue("-u", "--user")>]
      Username: string

      [<ArgValue("-n", "--name")>]
      Name: string

      [<ArgValue("-k", "--key")>]
      Key: string

      [<ArgValue("-s", "--stash")>]
      StashPath: string }

type SaveOptions =
    { [<ArgValue("-k", "--key")>]
      Key: string

      [<ArgValue("-s", "--stash")>]
      StashPath: string }

type InitOptions =
    { [<ArgValue("-p", "--path")>]
      Path: string }

type AddUserOptions =
    { [<ArgValue("-s", "--stash")>]
      StashPath: string

      [<ArgValue("-k", "--key")>]
      Key: string }

type AuthOptions =
    { [<ArgValue("-s", "--stash")>]
      StashPath: string

      [<ArgValue("-k", "--key")>]
      Key: string }

[<RequireQualifiedAccess>]
type Command =
    | KeyGen of KeyGenOptions
    | Http of HttpOptions
    | Get of GetOptions
    | Save of SaveOptions
    | AddUser of AddUserOptions
    | Init of InitOptions
    | Auth of AuthOptions

let run (command: Command) =
    match command with
    | Command.KeyGen kgo ->
        printDebug $"Running keygen command. Output path: {kgo.OutputPath}"

        Encryption.getCryptoBytes 32
        |> FileIO.writeBytes kgo.OutputPath
        |> fun r ->
            match r with
            | Ok _ -> printSuccess $"Key generated. Path: {kgo.OutputPath}"
            | Error e -> printError $"Error: {e}"
    | Command.Http ho -> ()
    | Command.Get go ->
        match FileIO.readBytes go.Key with
        | Ok key ->
            try
                let context = SqliteContext.Open(go.StashPath)

                match Store.getValue context key go.Name with
                | Ok v ->
                    printSuccess "Found."
                    printfn $"{v}"
                | Error e -> printError $"Error: {e}"
            with
            | exn -> printfn $"Error: {exn.Message}"
        | Error e -> printError $"Error loading key: {e}"
    | Command.Save so ->
        match FileIO.readBytes so.Key with
        | Ok key ->
            printfn "Name"
            printf "> "
            let name = Console.ReadLine()
            printfn "Value"
            printf "> "
            let value = Console.ReadLine()

            try
                let context = SqliteContext.Open(so.StashPath)
                Store.saveValue context key name value
                printSuccess $"`{name}` saved."
            with
            | exn -> printfn $"Error: {exn.Message}"
        | Error e -> printError $"Error loading key: {e}"
    | Command.AddUser au ->
        match Auth.loadKey au.Key with
        | Ok key ->
            printfn "Username"
            printf "> "
            let username = Console.ReadLine()
            printfn "Password"
            printf "> "
            let password = Console.ReadLine()

            try
                let context = SqliteContext.Open(au.StashPath)
                Auth.createUser context key username password
                printSuccess $"User `{username}` added."
            with
            | exn -> printError $"Error: {exn.Message}"
        | Error e -> printError $"Error loading key: {e}"
    | Command.Init io -> ()
    | Command.Auth ao ->
        match Auth.loadKey ao.Key with
        | Ok key ->
            printfn "Username"
            printf "> "
            let username = Console.ReadLine()
            printfn "Password"
            printf "> "
            let password = Console.ReadLine()

            try
                let context = SqliteContext.Open(ao.StashPath)

                match Auth.signIn context key username password with
                | Ok m -> printSuccess m
                | Error e -> printError e
            with
            | exn -> printError $"Error: {exn.Message}"
        | Error e -> printError $"Error loading key: {e}"

match ArgParser.tryGetOptions<Command> (Environment.GetCommandLineArgs() |> List.ofArray) with
| Ok cmd -> run cmd
| Error e -> printfn $"Error: {e}"
