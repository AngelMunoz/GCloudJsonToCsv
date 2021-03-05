namespace JsonToCsv

open FSharp.Control.Tasks
open Google.Cloud.Functions.Framework
open Microsoft.AspNetCore.Http
open System.Text.Json
open System.Text.Json.Serialization
open System
open Microsoft.Net.Http.Headers
open Microsoft.Extensions.Primitives
open ClosedXML
open ClosedXML.Excel
open ClosedXML.SimpleSheets


type Todo =
    { userId: int
      id: int
      title: string
      completed: bool }

type Post =
    { userId: int
      id: int
      title: string
      body: string }

type Address =
    { street: string
      suite: string
      city: string
      zipcode: string
      geo: {| lat: string; lng: string |}
      phone: string
      website: string
      company: {| name: string
                  catchPhrase: string
                  bs: string |} }

type User =
    { id: int
      name: string
      username: string
      email: string
      address: Address }

type RequestData =
    { todos: Todo list option
      posts: Post list option


      users: User list option }

[<AutoOpen>]
module FileWriter =
    let private writeTodos (todos: Todo list) (worksheet: IXLWorksheet) =
        Excel.populate (
            worksheet,
            todos,
            [ Excel
                .field(fun (todo: Todo) -> todo.id)
                  .header("Id")
              Excel
                  .field(fun (todo: Todo) -> todo.userId)
                  .header("User Id")
              Excel
                  .field(fun (todo: Todo) -> todo.title)
                  .header("Title")
              Excel
                  .field(fun (todo: Todo) -> todo.completed)
                  .header("Is Completed") ]
        )


    let private writePosts (posts: Post list) (worksheet: IXLWorksheet) =
        Excel.populate (
            worksheet,
            posts,
            [ Excel
                .field(fun (post: Post) -> post.id)
                  .header("Id")
              Excel
                  .field(fun (post: Post) -> post.userId)
                  .header("User Id")
              Excel
                  .field(fun (post: Post) -> post.title)
                  .header("Title")
              Excel
                  .field(fun (post: Post) -> post.body)
                  .header("Content") ]
        )

    let private writeUsers (users: User list) (worksheet: IXLWorksheet) =
        Excel.populate (
            worksheet,
            users,
            [ Excel
                .field(fun (user: User) -> user.id)
                  .header("Id")
              Excel
                  .field(fun (user: User) -> user.email)
                  .header("E-Mail")
              Excel
                  .field(fun (user: User) -> user.name)
                  .header("Name")
              Excel
                  .field(fun (user: User) -> user.username)
                  .header("Username") ]
        )

    let writeExcelFile (data: RequestData) =
        use workbook = new XLWorkbook()
        // just write the sheets that are in the payload

        data.todos
        |> Option.map (fun todos -> writeTodos todos (workbook.AddWorksheet("Todos")))
        |> ignore

        data.posts
        |> Option.map (fun posts -> writePosts posts (workbook.AddWorksheet("Posts")))
        |> ignore

        data.users
        |> Option.map (fun users -> writeUsers users (workbook.AddWorksheet("Users")))
        |> ignore
        // once we've added out worksheets we can write our excel file
        Excel.createFrom (workbook)


type Function() =
    let jsonOptions =
        let opts = JsonSerializerOptions()
        opts.Converters.Add(JsonFSharpConverter())
        opts.AllowTrailingCommas <- true
        opts.IgnoreNullValues <- true
        opts

    interface IHttpFunction with
        /// <summary>
        /// This function takes a request with a json body of the type RequestData then creates an Excel file from
        /// that which gets written into the response of the request
        /// </summary>
        /// <param name="context">The HTTP context, containing the request and the response.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        member this.HandleAsync context =
            task {
                try
                    let! payload = JsonSerializer.DeserializeAsync<RequestData>(context.Request.Body, jsonOptions)
                    let excel = writeExcelFile payload
                    let bytes = ReadOnlyMemory excel

                    do! context.Response.Body.WriteAsync bytes

                    context.Response.Headers.Add(
                        HeaderNames.ContentDisposition,
                        StringValues("""attachment;filename="asExcel.xlsx";""")
                    )

                    context.Response.Headers.Add(
                        "Content-Type",
                        StringValues("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    )

                with ex ->
                    eprintfn "%O" ex
                    do! context.Response.WriteAsync("""{ "message": "Something went wrong" }""")
                    context.Response.Headers.Add("Content-Type", StringValues("application/json"))
            }
            :> _
