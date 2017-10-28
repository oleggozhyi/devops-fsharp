namespace Devops
open System

module Printf =
    let private consoleColor (fc : ConsoleColor) = 
        let current = Console.ForegroundColor
        Console.ForegroundColor <- fc
        { new IDisposable with
              member x.Dispose() = Console.ForegroundColor <- current }

    let colorPrintf color str = Printf.kprintf (fun s -> use c = consoleColor color in printf "%s" s) str
    let colorPrintfn color str = Printf.kprintf (fun s -> use c = consoleColor color in printfn "%s" s) str
    let printReplicated length s = colorPrintfn ConsoleColor.DarkGray "%s" (String.replicate length s)
    let printSingleLine() = printReplicated 50 "-"
    let printDoubleLine() = printReplicated 60 "="

module Command =
    type ReturnCode = int
    let TimeOutCode: ReturnCode = -1
    type CommandOutput = string
    type CommandName = string
    type CommandResult = Result<ReturnCode, ReturnCode>
    let execute onStart onFinished (commandName: CommandName) commandAction: CommandResult * CommandOutput =
        onStart commandName  |> commandAction |> onFinished commandName
    
    let getFormattedTime() = DateTime.UtcNow.ToString "yyyy-MM-dd HH:mm:ss.fff"
    let printStartCommand commandName = Printf.colorPrintfn ConsoleColor.White "%s [START] %s" (getFormattedTime()) commandName
    let printStartCommandSeq commandSeqName =
        Printf.printDoubleLine()
        Printf.colorPrintfn ConsoleColor.White "       [START SEQUENCE] %s" commandSeqName
        Printf.printDoubleLine()

    let printFinishedCommand commandName (result, output) =
        Printf.colorPrintf ConsoleColor.DarkGray "%s" output
        match result with
        | Ok errorCode -> Printf.colorPrintfn ConsoleColor.Green "%s [SUCCESS] '%s' completed with code %d" (getFormattedTime()) commandName errorCode
        | Error errorCode -> Printf.colorPrintfn ConsoleColor.Red "%s [FAILURE| '%s' completed with code %d" (getFormattedTime()) commandName errorCode
        Printf.printSingleLine()
        (result, output)

    let printFinishCommandSeq commandSeqName (result, output) =
        Printf.printDoubleLine()
        match result with
        | Ok _ -> Printf.colorPrintfn ConsoleColor.Green "      [SEQUENCE SUCCEEDED] '%s'" commandSeqName
        | Error _ -> Printf.colorPrintfn ConsoleColor.Red "      [SEQUENCE FAILED| '%s'" commandSeqName
        Printf.printDoubleLine()
        (result, output)

    let executeCommand = execute printStartCommand printFinishedCommand
    let executeCommandSeq = execute printStartCommandSeq printFinishCommandSeq

open Command

module FileSystem =
    open System.Diagnostics

    let private createStartInfo cmd workDir =
        new ProcessStartInfo("CMD.exe", "/c "+ cmd,
            UseShellExecute = false, WorkingDirectory = workDir,
            RedirectStandardOutput = true, RedirectStandardError = true )

    let getLeafDir (path: string) = path.Split([|'/';'\\'|]) |> Array.last

    let runCmd cmd workDir = 
        let p = createStartInfo cmd workDir|> Process.Start 
        let output = sprintf "%s\n%s" (p.StandardOutput.ReadToEnd()) (p.StandardError.ReadToEnd())
        match p.WaitForExit 60000 with 
        | true -> let resultF = if p.ExitCode =0 then  Ok else Error in resultF p.ExitCode, output
        | false -> Error Command.TimeOutCode, output + "\n PROCESS TIMED OUT"

module Http =
    open System.Net.Http

    let send (method: HttpMethod) (url: string) (payload: string option) =
        use httpClient = new HttpClient()
        let message = match payload with 
                      | Some p -> new HttpRequestMessage(method, url, Content = new StringContent(p))
                      | None -> new HttpRequestMessage(method, url)
        let response = message |> httpClient.SendAsync |> Async.AwaitTask |> Async.RunSynchronously
        let content = response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
        match response.IsSuccessStatusCode with
        | true -> Ok (int response.StatusCode), content
        | false -> Error (int response.StatusCode), content
    
    let get url = send HttpMethod.Get url None
    let post url payload = send HttpMethod.Post url (Some payload)
    let put url payload = send HttpMethod.Put url (Some payload)
