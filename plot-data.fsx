#r "nuget: FSharp.Data, 6.3.0"
#r "nuget: Plotly.NET, 4.2.0"
#r "nuget: Plotly.NET.Interactive, 4.2.1"

open FSharp.Data
open Plotly.NET
open System.IO

[<Literal>]
let ResolutionFolder = __SOURCE_DIRECTORY__

type PIDResults = CsvProvider<"./data_PID_Fsharp.csv", ResolutionFolder=ResolutionFolder>
let pidResults = PIDResults.GetSample().Cache()

type MappedPIDResults =
    { Time: float list
      Command1: float list
      PositionZ1: float list
      Step1: float list
      Command2: float list
      PositionZ2: float list
      Step2: float list }

let initialMappedPIDResults =
    { Time = List.empty
      Command1 = List.empty
      PositionZ1 = List.empty
      Step1 = List.empty
      Command2 = List.empty
      PositionZ2 = List.empty
      Step2 = List.empty }

let mappedPIDResults =
    pidResults.Rows
    |> Seq.fold
        (fun state row ->
            { Time = (state.Time @ [ float row.Time ])
              Command1 = (state.Command1 @ [ float row.Command_1 ])
              PositionZ1 = (state.PositionZ1 @ [ float row.Z_1 ])
              Step1 = (state.Step1 @ [ float row.Step_1 ])
              Command2 = (state.Command2 @ [ float row.Command_2 ])
              PositionZ2 = (state.PositionZ2 @ [ float row.Z_2 ])
              Step2 = (state.Step2 @ [ float row.Step_2 ]) })
        initialMappedPIDResults

[ Chart.Line(mappedPIDResults.Time, mappedPIDResults.Command1)
  |> Chart.withTraceInfo (Name = "Command PID 1")
  |> Chart.withLineStyle (Width = 3.0, Dash = StyleParam.DrawingStyle.Solid)

  Chart.Line(mappedPIDResults.Time, mappedPIDResults.Command2)
  |> Chart.withTraceInfo (Name = "Command PID 2")
  |> Chart.withLineStyle (Width = 3.0, Dash = StyleParam.DrawingStyle.Solid) ]
|> Chart.combine
|> Chart.withXAxisStyle ("t[s]")
|> Chart.withYAxisStyle ("F [N]")
|> Chart.withTitle ("PID Controller command / Time")
|> Chart.withSize (800, 600)
|> Chart.show

[ Chart.Line(mappedPIDResults.Time, mappedPIDResults.Step1)
  |> Chart.withTraceInfo (Name = "Setpoint object 1")
  |> Chart.withLineStyle (Width = 3.0, Dash = StyleParam.DrawingStyle.Dash)

  Chart.Line(mappedPIDResults.Time, mappedPIDResults.Step2)
  |> Chart.withTraceInfo (Name = "Setpoint object 2")
  |> Chart.withLineStyle (Width = 3.0, Dash = StyleParam.DrawingStyle.Dash)

  Chart.Line(mappedPIDResults.Time, mappedPIDResults.PositionZ1)
  |> Chart.withTraceInfo (Name = "Response object 1")
  |> Chart.withLineStyle (Width = 3.0, Dash = StyleParam.DrawingStyle.Solid)

  Chart.Line(mappedPIDResults.Time, mappedPIDResults.PositionZ2)
  |> Chart.withTraceInfo (Name = "Response object 2")
  |> Chart.withLineStyle (Width = 3.0, Dash = StyleParam.DrawingStyle.Solid) ]
|> Chart.combine
|> Chart.withXAxisStyle ("t[s]")
|> Chart.withYAxisStyle ("F [N]")
|> Chart.withTitle ("Setpoint x Response / Time")
|> Chart.withSize (800, 600)
|> Chart.show
