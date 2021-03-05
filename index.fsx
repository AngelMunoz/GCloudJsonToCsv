open System.IO
open System.Net.Http
open System.Text

let content = File.ReadAllText("./payload.json")

async {
    use client = new HttpClient()

    let! result =
        client.PostAsync("YOUR GCLOUD URL", new StringContent(content, Encoding.UTF8, "application/json"))
        |> Async.AwaitTask

    if result.IsSuccessStatusCode then
        let! stream =
            result.Content.ReadAsStreamAsync()
            |> Async.AwaitTask

        let file = File.OpenWrite("./file.xlsx")
        do! stream.CopyToAsync(file) |> Async.AwaitTask
        do! file.FlushAsync() |> Async.AwaitTask
        file.Close()
    else
        eprintfn $"Status - {result.StatusCode}"
}
|> Async.RunSynchronously
