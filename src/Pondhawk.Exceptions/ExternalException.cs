// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Exceptions
{

    /// <summary>
    /// Base exception for external/application-facing errors with error kind, code, explanation, and event details.
    /// </summary>
    public abstract class ExternalException : Exception
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalException"/> class with the specified message.
        /// </summary>
        /// <param name="message">The error message.</param>
        protected ExternalException(string message) : base(message)
        {

            ErrorCode = GetType().Name.Replace("Exception", "");
            Explanation = message;

        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalException"/> class with the specified message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner exception that caused this exception.</param>
        protected ExternalException(string message, Exception inner) : base(message, inner)
        {

            ErrorCode = GetType().Name.Replace("Exception", "");
            Explanation = message;

            if (inner is not InternalException intra)
                return;

            InnerExplanation = intra.Explanation;

            foreach (var detail in intra.Details)
                Details.Add(detail);

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalException"/> class from an <see cref="IExceptionInfo"/>.
        /// </summary>
        /// <param name="info">The exception info to populate this exception from.</param>
        protected ExternalException(IExceptionInfo info) : base(info.Explanation)
        {

            Kind = info.Kind;
            ErrorCode = GetType().Name.Replace("Exception", "");
            Explanation = info.Explanation;
            InnerExplanation = info.Explanation;

            Details = new List<EventDetail>(info.Details);

        }



        /// <summary>
        /// Gets the classification of this error.
        /// </summary>
        public ErrorKind Kind { get; protected set; } = ErrorKind.System;

        /// <summary>
        /// Gets the error code, derived from the exception type name by default.
        /// </summary>
        public string ErrorCode { get; protected set; }

        /// <summary>
        /// Gets the human-readable explanation of the error.
        /// </summary>
        public string Explanation { get; protected set; }

        /// <summary>
        /// Gets the explanation from the inner exception, if available.
        /// </summary>
        public string InnerExplanation { get; protected set; } = "";

        /// <summary>
        /// Gets the correlation identifier for tracing this error across systems.
        /// </summary>
        public string CorrelationId { get; protected set; } = "";


        /// <summary>
        /// Gets the list of <see cref="EventDetail"/> instances associated with this exception.
        /// </summary>
        public IList<EventDetail> Details { get; protected set; } = new List<EventDetail>();

    }

}
