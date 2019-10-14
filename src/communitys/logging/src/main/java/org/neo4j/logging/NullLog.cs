using System;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
	/// A <seealso cref="Log"/> implementation that discards all messages
	/// </summary>
	public sealed class NullLog : Log
	{
		 private static readonly NullLog _instance = new NullLog();

		 private NullLog()
		 {
		 }

		 /// <returns> A singleton <seealso cref="NullLog"/> instance </returns>
		 public static NullLog Instance
		 {
			 get
			 {
				  return _instance;
			 }
		 }

		 public bool DebugEnabled
		 {
			 get
			 {
				  return false;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger debugLogger()
		 public override Logger DebugLogger()
		 {
			  return NullLogger.Instance;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void debug(@Nonnull String message)
		 public override void Debug( string message )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void debug(@Nonnull String message, @Nonnull Throwable throwable)
		 public override void Debug( string message, Exception throwable )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void debug(@Nonnull String format, @Nullable Object... arguments)
		 public override void Debug( string format, params object[] arguments )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger infoLogger()
		 public override Logger InfoLogger()
		 {
			  return NullLogger.Instance;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void info(@Nonnull String message)
		 public override void Info( string message )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void info(@Nonnull String message, @Nonnull Throwable throwable)
		 public override void Info( string message, Exception throwable )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void info(@Nonnull String format, @Nullable Object... arguments)
		 public override void Info( string format, params object[] arguments )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger warnLogger()
		 public override Logger WarnLogger()
		 {
			  return NullLogger.Instance;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void warn(@Nonnull String message)
		 public override void Warn( string message )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void warn(@Nonnull String message, @Nonnull Throwable throwable)
		 public override void Warn( string message, Exception throwable )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void warn(@Nonnull String format, @Nullable Object... arguments)
		 public override void Warn( string format, params object[] arguments )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger errorLogger()
		 public override Logger ErrorLogger()
		 {
			  return NullLogger.Instance;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void error(@Nonnull String message)
		 public override void Error( string message )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void error(@Nonnull String message, @Nonnull Throwable throwable)
		 public override void Error( string message, Exception throwable )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void error(@Nonnull String format, @Nullable Object... arguments)
		 public override void Error( string format, params object[] arguments )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void bulk(@Nonnull Consumer<Log> consumer)
		 public override void Bulk( Consumer<Log> consumer )
		 {
			  consumer.accept( this );
		 }
	}

}