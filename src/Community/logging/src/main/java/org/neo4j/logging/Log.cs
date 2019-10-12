using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Logging
{

	/// <summary>
	/// A log into which various levels of messages can be written
	/// </summary>
	public interface Log
	{
		 /// <returns> true if the current log level enables debug logging </returns>
		 bool DebugEnabled { get; }

		 /// <returns> a <seealso cref="Logger"/> instance for writing debug messages </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull Logger debugLogger();
		 Logger DebugLogger();

		 /// <summary>
		 /// Shorthand for {@code debugLogger().log( message )}
		 /// </summary>
		 /// <param name="message"> The message to be written </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void debug(@Nonnull String message);
		 void Debug( string message );

		 /// <summary>
		 /// Shorthand for {@code debugLogger().log( message, throwable )}
		 /// </summary>
		 /// <param name="message">   The message to be written </param>
		 /// <param name="throwable"> An exception that will also be written </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void debug(@Nonnull String message, @Nonnull Throwable throwable);
		 void Debug( string message, Exception throwable );

		 /// <summary>
		 /// Shorthand for {@code debugLogger().log( format, arguments )}
		 /// </summary>
		 /// <param name="format">    A string format for writing a message </param>
		 /// <param name="arguments"> Arguments to substitute into the message according to the format </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void debug(@Nonnull String format, @Nullable Object... arguments);
		 void Debug( string format, params object[] arguments );

		 /// <returns> a <seealso cref="Logger"/> instance for writing info messages </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull Logger infoLogger();
		 Logger InfoLogger();

		 /// <summary>
		 /// Shorthand for {@code infoLogger().log( message )}
		 /// </summary>
		 /// <param name="message"> The message to be written </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void info(@Nonnull String message);
		 void Info( string message );

		 /// <summary>
		 /// Shorthand for {@code infoLogger().log( message, throwable )}
		 /// </summary>
		 /// <param name="message">   The message to be written </param>
		 /// <param name="throwable"> An exception that will also be written </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void info(@Nonnull String message, @Nonnull Throwable throwable);
		 void Info( string message, Exception throwable );

		 /// <summary>
		 /// Shorthand for {@code infoLogger().log( format, arguments )}
		 /// </summary>
		 /// <param name="format">    A string format for writing a message </param>
		 /// <param name="arguments"> Arguments to substitute into the message according to the format </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void info(@Nonnull String format, @Nullable Object... arguments);
		 void Info( string format, params object[] arguments );

		 /// <returns> a <seealso cref="Logger"/> instance for writing warn messages </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull Logger warnLogger();
		 Logger WarnLogger();

		 /// <summary>
		 /// Shorthand for {@code warnLogger().log( message )}
		 /// </summary>
		 /// <param name="message"> The message to be written </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void warn(@Nonnull String message);
		 void Warn( string message );

		 /// <summary>
		 /// Shorthand for {@code warnLogger().log( message, throwable )}
		 /// </summary>
		 /// <param name="message">   The message to be written </param>
		 /// <param name="throwable"> An exception that will also be written </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void warn(@Nonnull String message, @Nonnull Throwable throwable);
		 void Warn( string message, Exception throwable );

		 /// <summary>
		 /// Shorthand for {@code warnLogger().log( format, arguments )}
		 /// </summary>
		 /// <param name="format">    A string format for writing a message </param>
		 /// <param name="arguments"> Arguments to substitute into the message according to the format </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void warn(@Nonnull String format, @Nullable Object... arguments);
		 void Warn( string format, params object[] arguments );

		 /// <returns> a <seealso cref="Logger"/> instance for writing error messages </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull Logger errorLogger();
		 Logger ErrorLogger();

		 /// <summary>
		 /// Shorthand for {@code errorLogger().log( message )}
		 /// </summary>
		 /// <param name="message"> The message to be written </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void error(@Nonnull String message);
		 void Error( string message );

		 /// <summary>
		 /// Shorthand for {@code errorLogger().log( message, throwable )}
		 /// </summary>
		 /// <param name="message">   The message to be written </param>
		 /// <param name="throwable"> An exception that will also be written </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void error(@Nonnull String message, @Nonnull Throwable throwable);
		 void Error( string message, Exception throwable );

		 /// <summary>
		 /// Shorthand for {@code errorLogger().log( format, arguments )}
		 /// </summary>
		 /// <param name="format">    A string format for writing a message </param>
		 /// <param name="arguments"> Arguments to substitute into the message according to the {@code format} </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void error(@Nonnull String format, @Nullable Object... arguments);
		 void Error( string format, params object[] arguments );

		 /// <summary>
		 /// Used to temporarily log several messages in bulk. The implementation may choose to
		 /// disable flushing, and may also block other operations until the bulk update is completed.
		 /// </summary>
		 /// <param name="consumer"> A consumer for the bulk <seealso cref="Log"/> </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void bulk(@Nonnull Consumer<Log> consumer);
		 void Bulk( Consumer<Log> consumer );
	}

}