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
namespace Org.Neo4j.Logging
{

	/// <summary>
	/// An abstract implementation of <seealso cref="Log"/>, providing implementations
	/// for the shortcut methods (debug, info, warn, error) that delegate
	/// to the appropriate <seealso cref="Logger"/> (as obtained by <seealso cref="Log.debugLogger()"/>,
	/// <seealso cref="Log.infoLogger()"/>, <seealso cref="Log.warnLogger()"/> and
	/// <seealso cref="Log.errorLogger()"/> respectively).
	/// </summary>
	public abstract class AbstractLog : Log
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void bulk(@Nonnull Consumer<Log> consumer);
		public abstract void Bulk( Consumer<Log> consumer );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void error(@Nonnull String format, @Nullable Object... arguments);
		public abstract void Error( string format, params object[] arguments );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void error(@Nonnull String message, @Nonnull Throwable throwable);
		public abstract void Error( string message, Exception throwable );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void error(@Nonnull String message);
		public abstract void Error( string message );
		public abstract Logger ErrorLogger();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void warn(@Nonnull String format, @Nullable Object... arguments);
		public abstract void Warn( string format, params object[] arguments );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void warn(@Nonnull String message, @Nonnull Throwable throwable);
		public abstract void Warn( string message, Exception throwable );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void warn(@Nonnull String message);
		public abstract void Warn( string message );
		public abstract Logger WarnLogger();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void info(@Nonnull String format, @Nullable Object... arguments);
		public abstract void Info( string format, params object[] arguments );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void info(@Nonnull String message, @Nonnull Throwable throwable);
		public abstract void Info( string message, Exception throwable );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void info(@Nonnull String message);
		public abstract void Info( string message );
		public abstract Logger InfoLogger();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void debug(@Nonnull String format, @Nullable Object... arguments);
		public abstract void Debug( string format, params object[] arguments );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void debug(@Nonnull String message, @Nonnull Throwable throwable);
		public abstract void Debug( string message, Exception throwable );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void debug(@Nonnull String message);
		public abstract void Debug( string message );
		public abstract Logger DebugLogger();
		public abstract bool DebugEnabled { get; }
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void debug(@Nonnull String message)
		 public override void Debug( string message )
		 {
			  DebugLogger().log(message);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void debug(@Nonnull String message, @Nonnull Throwable throwable)
		 public override void Debug( string message, Exception throwable )
		 {
			  DebugLogger().log(message, throwable);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void debug(@Nonnull String format, @Nullable Object... arguments)
		 public override void Debug( string format, params object[] arguments )
		 {
			  DebugLogger().log(format, arguments);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void info(@Nonnull String message)
		 public override void Info( string message )
		 {
			  InfoLogger().log(message);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void info(@Nonnull String message, @Nonnull Throwable throwable)
		 public override void Info( string message, Exception throwable )
		 {
			  InfoLogger().log(message, throwable);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void info(@Nonnull String format, @Nullable Object... arguments)
		 public override void Info( string format, params object[] arguments )
		 {
			  InfoLogger().log(format, arguments);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void warn(@Nonnull String message)
		 public override void Warn( string message )
		 {
			  WarnLogger().log(message);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void warn(@Nonnull String message, @Nonnull Throwable throwable)
		 public override void Warn( string message, Exception throwable )
		 {
			  WarnLogger().log(message, throwable);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void warn(@Nonnull String format, @Nullable Object... arguments)
		 public override void Warn( string format, params object[] arguments )
		 {
			  WarnLogger().log(format, arguments);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void error(@Nonnull String message)
		 public override void Error( string message )
		 {
			  ErrorLogger().log(message);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void error(@Nonnull String message, @Nonnull Throwable throwable)
		 public override void Error( string message, Exception throwable )
		 {
			  ErrorLogger().log(message, throwable);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void error(@Nonnull String format, @Nullable Object... arguments)
		 public override void Error( string format, params object[] arguments )
		 {
			  ErrorLogger().log(format, arguments);
		 }
	}

}