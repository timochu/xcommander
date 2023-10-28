module xcommander.Steam
open System
open System.Threading.Tasks
open System.Threading
open Steamworks

let fetchModDetails (modIds: uint64 seq) : Async<Result<SteamUGCDetails_t list, string>> = async {
    try
        SteamAPI.Init() |> ignore // Initialize the Steam API
        let cts = new CancellationTokenSource()
        let token = cts.Token

        // Start a task that runs SteamAPI.RunCallbacks() periodically
        let runCallbacksTask = Task.Run(fun () ->
            while not token.IsCancellationRequested do
                SteamAPI.RunCallbacks()
                Thread.Sleep(100)  // This delay controls how often callbacks are processed, adjust as necessary
        )

        let tcs = new TaskCompletionSource<SteamUGCDetails_t list>()

        try
            // Convert list of mod IDs to an array for the API request
            let modIdsArray = modIds |> Seq.map(PublishedFileId_t) |> Seq.toArray

            // Create a query with multiple mod IDs
            let queryHandle = SteamUGC.CreateQueryUGCDetailsRequest(modIdsArray, uint32 modIdsArray.Length)

            // Send the query
            let call = SteamUGC.SendQueryUGCRequest(queryHandle)
            let callResult = CallResult<SteamUGCQueryCompleted_t>.Create()

            // Set the callback with proper error checking
            callResult.Set(call, fun r e ->
                try
                    if e || r.m_eResult <> EResult.k_EResultOK then
                        tcs.SetException(new Exception("Failed to fetch UGC details"))
                    else
                        try
                            // Collect details for each item
                            let detailsList =
                                [0 .. modIdsArray.Length - 1]
                                |> List.choose (fun i ->
                                    let mutable details = Unchecked.defaultof<SteamUGCDetails_t>
                                    if SteamUGC.GetQueryUGCResult(queryHandle, uint32 i, &details) then
                                        Some details // Successfully retrieved details
                                    else
                                        None // Skip failed entries
                                )
                            tcs.SetResult(detailsList)
                        with
                            | ex -> tcs.SetException(ex) // In case of an exception, set it for the task
                finally
                    SteamUGC.ReleaseQueryUGCRequest(queryHandle) |> ignore // Ensure the query handle is released
            )

            // Await the task completion source within the async workflow
            let! detailsList = Async.AwaitTask <| tcs.Task

            // Once you've received the response, you can cancel the callbacks task
            cts.Cancel()

            // Await the runCallbacksTask to ensure it has ended after cancellation
            // do! Async.AwaitTask <| runCallbacksTask.ContinueWith(fun _ -> ()) // Safe continuation
            do! Async.AwaitTask(runCallbacksTask)

            return Ok(detailsList) // Return the list of details as a successful result

        with
            | ex ->
                // If there's an exception, ensure to cancel the ongoing task before returning
                cts.Cancel()
                return Error(ex.Message) // Return the error message as a failed result
    finally
        SteamAPI.Shutdown() |> ignore // Shutdown the Steam API
}


// let results = fetchModDetails modIds |> Async.RunSynchronously

// Handle the results
// match results with
// | Ok(detailsList) ->
//     // Process or print details for each mod
//     detailsList |> List.iter (fun details -> printfn "Mod title: %s" details.m_rgchTitle)
// | Error(error) ->
//     printfn "Error: %s" error

// let initialized = SteamAPI.Init()
// if not initialized then
//     printf "Steam API failed to initialize."
//     Environment.Exit(1)
// let user = SteamUser.GetSteamID() // Get the user's Steam ID
// printfn "Your Steam ID: %A" user
