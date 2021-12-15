namespace Stash

open System
open System.IO
open System.Text
open Freql.Core.Common.Types
open Freql.Sqlite
open Stash.Persistence.Records
open ToolBox.Core

module Store =
    
    open Persistence.Records
    open Persistence.Operations
    
    
    let saveValue (context: SqliteContext) (key: byte array) (name: string) (value: string) =
        let salt = Encryption.getCryptoBytes 16
        let v = Encoding.UTF8.GetBytes value |>  Encryption.encryptBytesAes key salt
        let packed = Encryption.pack v salt
        use ms = new MemoryStream(packed)
        
        context.Insert(StashItemRecord.TableName(), ({
            Name = name
            ItemData = BlobField.FromStream ms
        }: StashItemRecord))
    
    
    let getValue (context: SqliteContext) (key: byte array) (name: string) =
        let sql = [ StashItemRecord.SelectSql(); "WHERE name = @0" ] |> String.concat Environment.NewLine
        match context.SelectSingleAnon<StashItemRecord>(sql, [ name ]) with
        | Some si ->
            match si.ItemData.ToBytes() |> Encryption.unpack with
            | Ok (salt, data) ->
                Encryption.decryptBytesAes key salt data
                |> Encoding.UTF8.GetString
                |> Ok
            | Error e -> Error e
        | None -> Error "Item not found."
         
        
        
    
        
    
    
    
    ()

