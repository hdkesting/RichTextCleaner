﻿// <copyright file="LogLevel.cs" company="Hans Kesting">
// Copyright (c) Hans Kesting. All rights reserved.
// </copyright>

namespace RichTextCleaner.Common.Logging
{
    /// <summary>
    /// The level of the log message.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// A debug message.
        /// </summary>
        Debug,

        /// <summary>
        /// An informational message.
        /// </summary>
        Information,

        /// <summary>
        /// A warning that may require attention.
        /// </summary>
        Warning,

        /// <summary>
        /// An error (probably should include exception).
        /// </summary>
        Error,
    }
}
