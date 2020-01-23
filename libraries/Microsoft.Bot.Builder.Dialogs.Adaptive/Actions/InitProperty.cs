﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Expressions.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Sets a property with the result of evaluating a value expression.
    /// </summary>
    public class InitProperty : Dialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.InitProperty";

        [JsonConstructor]
        public InitProperty([CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; } 

        /// <summary>
        /// Gets or sets property path to initialize.
        /// </summary>
        /// <value>
        /// Property path to initialize.
        /// </value>
        [JsonProperty("property")]
        public StringExpression Property { get; set; }

        /// <summary>
        ///  Gets or sets type, either Array or Object.
        /// </summary>
        /// <value>
        /// Type, either Array or Object.
        /// </value>
        [JsonProperty("type")]
        public string Type { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            var dcState = dc.GetState();

            if (this.Disabled != null && this.Disabled.GetValue(dcState) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            // Ensure planning context
            if (dc is SequenceContext planning)
            {
                var state = dcState;

                switch (Type.ToLower())
                {
                    case "array":
                        state.SetValue(this.Property.GetValue(state), new JArray());
                        break;
                    case "object":
                        state.SetValue(this.Property.GetValue(state), new JObject());
                        break;
                }

                return await planning.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new Exception("`InitProperty` should only be used in the context of an adaptive dialog.");
            }
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[${this.Property.ToString() ?? string.Empty}]";
        }
    }
}
