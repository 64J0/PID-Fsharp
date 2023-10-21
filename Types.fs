module PID.Types

/// <summary>PID: A record type encapsulating the controller parameters.</summary>
type PID =
    {
        /// <summary>Proportional gain constant</summary>
        Kp: float

        /// <summary>Integral gain constant</summary>
        Ki: float

        /// <summary>Derivative gain constant</summary>
        Kd: float

        /// <summary>Anti-windup gain constant</summary>
        Kaw: float

        /// <summary>Time constant for derivative filtering</summary>
        T_C: float

        /// <summary>Time step</summary>
        T: float

        /// <summary>Max command</summary>
        max: float

        /// <summary>Min command</summary>
        min: float

        /// <summary>Max rate of change of the command</summary>
        max_rate: float

        /// <summary>Integral term</summary>
        integral: float

        /// <summary>Previous error</summary>
        err_prev: float

        /// <summary>Previous derivative</summary>
        deriv_prev: float

        /// <summary>Previous saturated command</summary>
        command_sat_prev: float

        /// <summary>Previous command</summary>
        command_prev: float
    }

/// <summary>DynamicSystem: A record type encapsulating the system parameters.</summary>
type DynamicSystem =
    {
        /// <summary>Mass of the object</summary>
        m: float

        /// <summary>Damping constant</summary>
        k: float

        /// <summary>Max force applied to the object</summary>
        F_max: float

        /// <summary>Min force applied to the object</summary>
        F_min: float

        /// <summary>Time step</summary>
        T: float

        /// <summary>Velocity of the object</summary>
        v: float

        /// <summary>Position of the object</summary>
        z: float
    }
