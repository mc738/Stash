namespace Stash.Persistence

open System
open System.Text.Json.Serialization
open Freql.Core.Common
open Freql.Sqlite

/// Module generated on 14/12/2021 23:47:20 (utc) via Freql.Sqlite.Tools.
module Records =
    type ItemAccessRecord =
        { [<JsonPropertyName("item")>] Item: string
          [<JsonPropertyName("username")>] Username: string }
    
        static member Blank() =
            { Item = String.Empty
              Username = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE item_access (
	item TEXT NOT NULL,
	username TEXT NOT NULL,
	CONSTRAINT item_access_PK PRIMARY KEY (item,username),
	CONSTRAINT item_access_FK FOREIGN KEY (item) REFERENCES stash(name),
	CONSTRAINT item_access_FK_1 FOREIGN KEY (username) REFERENCES users(username)
)
        """
    
        static member SelectSql() = """
        SELECT
              item,
              username
        FROM item_access
        """
    
        static member TableName() = "item_access"
    
    type StashItemRecord =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("itemData")>] ItemData: BlobField }
    
        static member Blank() =
            { Name = String.Empty
              ItemData = BlobField.Empty() }
    
        static member CreateTableSql() = """
        CREATE TABLE stash (
	name TEXT NOT NULL,
	item_data BLOB NOT NULL,
	CONSTRAINT stash_PK PRIMARY KEY (name)
)
        """
    
        static member SelectSql() = """
        SELECT
              name,
              item_data
        FROM stash
        """
    
        static member TableName() = "stash"
    
    type UserRecord =
        { [<JsonPropertyName("username")>] Username: string
          [<JsonPropertyName("password")>] Password: BlobField }
    
        static member Blank() =
            { Username = String.Empty
              Password = BlobField.Empty() }
    
        static member CreateTableSql() = """
        CREATE TABLE users (
	username TEXT NOT NULL,
	password BLOB NOT NULL,
	CONSTRAINT users_PK PRIMARY KEY (username)
)
        """
    
        static member SelectSql() = """
        SELECT
              username,
              password
        FROM users
        """
    
        static member TableName() = "users"
    
module Operations =
    type AddItemAccessParameters =
        { [<JsonPropertyName("item")>] Item: string
          [<JsonPropertyName("username")>] Username: string }
    
        static member Blank() =
            { Item = String.Empty
              Username = String.Empty }
    
    let insertItemAccess (parameters: AddItemAccessParameters) (context: SqliteContext) =
        context.Insert("item_access", parameters)
    
    type AddStashItemParameters =
        { [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("itemData")>] ItemData: BlobField }
    
        static member Blank() =
            { Name = String.Empty
              ItemData = BlobField.Empty() }
    
    let insertStashItem (parameters: AddStashItemParameters) (context: SqliteContext) =
        context.Insert("stash", parameters)
    
    type AddUserParameters =
        { [<JsonPropertyName("username")>] Username: string
          [<JsonPropertyName("password")>] Password: BlobField }
    
        static member Blank() =
            { Username = String.Empty
              Password = BlobField.Empty() }
    
    let insertUser (parameters: AddUserParameters) (context: SqliteContext) =
        context.Insert("users", parameters)
    