namespace Stash

open System
open System.IO
open Freql.Core.Common.Types
open Freql.Sqlite
open Stash.Persistence.Records
open ToolBox.Core
open Stash.Persistence

module Auth =

    let iterations = 100000
    
    let hashSize = 64
    
    let getUser (context: SqliteContext) (username: string) =
        let sql = [ UserRecord.SelectSql(); "WHERE username = @0;" ] |> String.concat Environment.NewLine
        context.SelectSingleAnon<UserRecord>(sql, [ username ])

    let authenticateUser (key: string) (user: UserRecord) (password: string) =
        match Encryption.unpack (user.Password.ToBytes()) with
        | Ok (salt, bytes) ->
            let attempt =
                Passwords.pdkdf2 key iterations hashSize password salt
            //printDebug $"R: {Convert.ToBase64String(r)}"
            //printDebug $"Unpacked bytes: {Convert.ToBase64String(bytes)}"
            //printDebug $"Attempt: {attempt}"

            match
                Convert.ToBase64String(bytes)
                |> Strings.equalOrdinal attempt
                with
            | true -> Ok "Passwords match."
            | false -> Error $"Passwords do not match."
        | Error e -> Error $"Could no unpack password. Possible tampering. Error: {e}"

    let createUser (context: SqliteContext) (key: string) (username: string) (password: string) =
        let salt = Encryption.getCryptoBytes 16
        
        let pwBytes = Passwords.pdkdf2Bytes key 100000 64 password salt
        
        use ms = new MemoryStream(Encryption.pack pwBytes salt)
        
        let ur = ({
            Username = username
            Password = BlobField.FromStream ms
        }: UserRecord)
        
        context.Insert(UserRecord.TableName(), ur)
        
    let signIn (context: SqliteContext) (key: string) (username: string) (password: string) =
        match getUser context username with
        | Some ur -> authenticateUser key ur password
        | None -> Error "User not found."
        
    let loadKey (path: string) =
            match FileIO.readBytes path with
            | Ok b -> Convert.ToBase64String b |> Ok
            | Error e -> Error e