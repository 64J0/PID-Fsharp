open System
open System.IO

open PID.Types

[<Literal>]
let LENGTH = 1200

[<Literal>]
let TIME_STEP = 0.1

/// <summary>This function implements a PID controller.</summary>
///
/// <param name="pid">A PID record type</param>
/// <param name="measurement">Current measurement of the process variable</param>
/// <param name="setpoint">Desired value of the process variable</param>
///
/// <returns>
/// Updated PID record and
/// the control output of the PID controller (saturated based on max. min, max_rate):
/// command_sat.
/// </returns>
let pidStep (pid: PID) (measurement: float) (setpoint: float) : (PID * float) =
    // Error calculation
    let err = setpoint - measurement

    // Integral term calculation - including anti-windup
    let newIntegral =
        pid.integral
        + pid.Ki * err * pid.T
        + pid.Kaw * (pid.command_sat_prev - pid.command_prev) * pid.T

    // Derivative term calculation using filtered derivative method
    let derivFilt = (err - pid.err_prev + pid.T_C * pid.deriv_prev) / (pid.T + pid.T_C)
    let newErrPrev = err
    let newDerivPrev = derivFilt

    // Summing the 3 terms
    let newCommand = pid.Kp * err + newIntegral + pid.Kd * derivFilt

    // Remember command at previous step
    let newCommandPrev = newCommand

    // Saturate command
    let commandSat =
        if (newCommand > pid.max) then pid.max
        elif (newCommand < pid.min) then pid.min
        else newCommand

    // Apply rate limiter
    let commandSat' =
        if (commandSat > pid.command_sat_prev + pid.max_rate * pid.T) then
            pid.command_sat_prev + pid.max_rate * pid.T
        elif (commandSat < pid.command_sat_prev - pid.max_rate * pid.T) then
            pid.command_sat_prev - pid.max_rate * pid.T
        else
            commandSat

    // Remember saturated command at previous step
    let newCommandSat = commandSat'

    let newPID =
        { pid with
            integral = newIntegral
            err_prev = newErrPrev
            deriv_prev = newDerivPrev
            command_prev = newCommandPrev
            command_sat_prev = newCommandSat }

    newPID, newCommandSat

/// <summary>
/// This function updates the position of an object in 1D based on the applied force F and
/// the object's mass, viscous damping coefficient k, max/min forces, and time step T.
/// </summary>
///
/// <param name="dynamicSystem">A DynamicSystem record type</param>
/// <param name="F">The force applied to the object</param>
///
/// <returns>
/// A DynamicSystem updated instance,
/// z: the position of the object in meters
/// </returns>
let dynamicSystemStep (dynamicSystem: DynamicSystem) (F: float) : (DynamicSystem * float) =
    // Apply saturation to the input force
    let F_sat =
        if (F > dynamicSystem.F_max) then dynamicSystem.F_max
        elif (F < dynamicSystem.F_min) then dynamicSystem.F_min
        else F

    // Calculate the derivative dv/dt using the input force and the object's velocity and properties
    let dv_dt = (F_sat - dynamicSystem.k * dynamicSystem.v) / dynamicSystem.m

    // Update the velocity and position of the object by integrating the derivative using the time step T
    let newV = dynamicSystem.v + dv_dt * dynamicSystem.T
    let newZ = dynamicSystem.z + newV * dynamicSystem.T

    let newDynamicSystem =
        { dynamicSystem with
            v = newV
            z = newZ }

    newDynamicSystem, newZ

[<EntryPoint>]
let main (_args: string array) : int =
    // Current simulation time
    let mutable t = 0.0

    // Iteration counter
    let mutable i = 0

    // Setpoint and output of the first control loop
    let mutable command1 = 0.0
    let mutable z1 = 0.0

    // Setpoint and output of the second control loop
    let mutable command2 = 0.0
    let mutable z2 = 0.0

    // PID controller parameters for the first control loop
    let mutable pid1: PID =
        { Kp = 1.0
          Ki = 0.1
          Kd = 5.0
          Kaw = 0.1
          T_C = 1.0
          T = TIME_STEP
          max = 100.0
          min = -100.0
          max_rate = 40.0
          integral = 0.0
          err_prev = 0.0
          deriv_prev = 0.0
          command_sat_prev = 0.0
          command_prev = 0.0 }

    // Object parameters for the first control loop
    let mutable dynamicSystem1: DynamicSystem =
        { m = 10.0
          k = 0.5
          F_max = 100.0
          F_min = -100.0
          T = TIME_STEP
          v = 0.0
          z = 0.0 }

    let mutable pid2: PID =
        { Kp = 1.8
          Ki = 0.3
          Kd = 7.0
          Kaw = 0.3
          T_C = 1.0
          T = TIME_STEP
          max = 100.0
          min = -100.0
          max_rate = 40.0
          integral = 0.0
          err_prev = 0.0
          deriv_prev = 0.0
          command_sat_prev = 0.0
          command_prev = 0.0 }

    let mutable dynamicSystem2: DynamicSystem =
        { m = 10.0
          k = 0.5
          F_max = 100.0
          F_min = -100.0
          T = TIME_STEP
          v = 0.0
          z = 0.0 }

    // Write the result to a file
    use outputFile = new StreamWriter("data_PID_Fsharp.csv")
    outputFile.WriteLine("Time,Command_1,Z_1,Step_1,Command_2,Z_2,Step_2")

    // Implement iteration using a while loop
    while (i < LENGTH) do
        // Change setpoint at t = 60 seconds
        let step1, step2 = if (t < 60) then (100.0, 50.0) else (200.0, 150.0)

        // Execute the first control loop
        let newPid1, newCommand1 = pidStep pid1 z1 step1
        pid1 <- newPid1
        command1 <- newCommand1

        let newDynamicSystem1, newZ1 = dynamicSystemStep dynamicSystem1 command1
        dynamicSystem1 <- newDynamicSystem1
        z1 <- newZ1

        // Execute the second control loop
        let newPid2, newCommand2 = pidStep pid2 z2 step2
        pid2 <- newPid2
        command2 <- newCommand2

        let newDynamicSystem2, newZ2 = dynamicSystemStep dynamicSystem2 command2
        dynamicSystem2 <- newDynamicSystem2
        z2 <- newZ2

        outputFile.WriteLine(sprintf "%.6f,%.6f,%.6f,%.6f,%.6f,%.6f,%.6f" t command1 z1 step1 command2 z2 step2)

        // Increment the time and iteration counter
        t <- t + TIME_STEP
        i <- i + 1

    outputFile.Flush()
    0
