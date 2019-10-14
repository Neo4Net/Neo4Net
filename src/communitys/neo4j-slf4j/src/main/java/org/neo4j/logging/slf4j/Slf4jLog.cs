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
namespace Neo4Net.Logging.slf4j
{


	/// <summary>
	/// An adapter from a <seealso cref="org.slf4j.Logger"/> to a <seealso cref="Log"/> interface
	/// </summary>
	public class Slf4jLog : AbstractLog
	{
		 private readonly object @lock;
		 private readonly org.slf4j.Logger _slf4jLogger;
		 private readonly Logger _debugLogger;
		 private readonly Logger _infoLogger;
		 private readonly Logger _warnLogger;
		 private readonly Logger _errorLogger;

		 /// <param name="slf4jLogger"> the SLF4J logger to output to </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Slf4jLog(final org.slf4j.Logger slf4jLogger)
		 public Slf4jLog( org.slf4j.Logger slf4jLogger )
		 {
			  this.@lock = this;
			  this._slf4jLogger = slf4jLogger;

			  this._debugLogger = new LoggerAnonymousInnerClass( this, slf4jLogger );

			  this._infoLogger = new LoggerAnonymousInnerClass2( this, slf4jLogger );

			  this._warnLogger = new LoggerAnonymousInnerClass3( this, slf4jLogger );

			  this._errorLogger = new LoggerAnonymousInnerClass4( this, slf4jLogger );
		 }

		 private class LoggerAnonymousInnerClass : Logger
		 {
			 private readonly Slf4jLog _outerInstance;

			 private org.slf4j.Logger _slf4jLogger;

			 public LoggerAnonymousInnerClass( Slf4jLog outerInstance, org.slf4j.Logger slf4jLogger )
			 {
				 this.outerInstance = outerInstance;
				 this._slf4jLogger = slf4jLogger;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message)
			 public void log( string message )
			 {
				  lock ( _outerInstance.@lock )
				  {
						_slf4jLogger.debug( message );
				  }
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message, @Nonnull Throwable throwable)
			 public void log( string message, Exception throwable )
			 {
				  lock ( _outerInstance.@lock )
				  {
						_slf4jLogger.debug( message, throwable );
				  }
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String format, @Nonnull Object... arguments)
			 public void log( string format, params object[] arguments )
			 {
				  lock ( _outerInstance.@lock )
				  {
						_slf4jLogger.debug( outerInstance.convertFormat( format ), arguments );
				  }
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void bulk(@Nonnull Consumer<org.neo4j.logging.Logger> consumer)
			 public void bulk( Consumer<Logger> consumer )
			 {
				  lock ( _outerInstance.@lock )
				  {
						consumer.accept( this );
				  }
			 }
		 }

		 private class LoggerAnonymousInnerClass2 : Logger
		 {
			 private readonly Slf4jLog _outerInstance;

			 private org.slf4j.Logger _slf4jLogger;

			 public LoggerAnonymousInnerClass2( Slf4jLog outerInstance, org.slf4j.Logger slf4jLogger )
			 {
				 this.outerInstance = outerInstance;
				 this._slf4jLogger = slf4jLogger;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message)
			 public void log( string message )
			 {
				  lock ( _outerInstance.@lock )
				  {
						_slf4jLogger.info( message );
				  }
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message, @Nonnull Throwable throwable)
			 public void log( string message, Exception throwable )
			 {
				  lock ( _outerInstance.@lock )
				  {
						_slf4jLogger.info( message, throwable );
				  }
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String format, @Nonnull Object... arguments)
			 public void log( string format, params object[] arguments )
			 {
				  lock ( _outerInstance.@lock )
				  {
						_slf4jLogger.info( outerInstance.convertFormat( format ), arguments );
				  }
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void bulk(@Nonnull Consumer<org.neo4j.logging.Logger> consumer)
			 public void bulk( Consumer<Logger> consumer )
			 {
				  lock ( _outerInstance.@lock )
				  {
						consumer.accept( this );
				  }
			 }
		 }

		 private class LoggerAnonymousInnerClass3 : Logger
		 {
			 private readonly Slf4jLog _outerInstance;

			 private org.slf4j.Logger _slf4jLogger;

			 public LoggerAnonymousInnerClass3( Slf4jLog outerInstance, org.slf4j.Logger slf4jLogger )
			 {
				 this.outerInstance = outerInstance;
				 this._slf4jLogger = slf4jLogger;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message)
			 public void log( string message )
			 {
				  lock ( _outerInstance.@lock )
				  {
						_slf4jLogger.warn( message );
				  }
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message, @Nonnull Throwable throwable)
			 public void log( string message, Exception throwable )
			 {
				  lock ( _outerInstance.@lock )
				  {
						_slf4jLogger.warn( message, throwable );
				  }
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String format, @Nonnull Object... arguments)
			 public void log( string format, params object[] arguments )
			 {
				  lock ( _outerInstance.@lock )
				  {
						_slf4jLogger.warn( outerInstance.convertFormat( format ), arguments );
				  }
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void bulk(@Nonnull Consumer<org.neo4j.logging.Logger> consumer)
			 public void bulk( Consumer<Logger> consumer )
			 {
				  lock ( _outerInstance.@lock )
				  {
						consumer.accept( this );
				  }
			 }
		 }

		 private class LoggerAnonymousInnerClass4 : Logger
		 {
			 private readonly Slf4jLog _outerInstance;

			 private org.slf4j.Logger _slf4jLogger;

			 public LoggerAnonymousInnerClass4( Slf4jLog outerInstance, org.slf4j.Logger slf4jLogger )
			 {
				 this.outerInstance = outerInstance;
				 this._slf4jLogger = slf4jLogger;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message)
			 public void log( string message )
			 {
				  lock ( _outerInstance.@lock )
				  {
						_slf4jLogger.error( message );
				  }
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message, @Nonnull Throwable throwable)
			 public void log( string message, Exception throwable )
			 {
				  lock ( _outerInstance.@lock )
				  {
						_slf4jLogger.error( message, throwable );
				  }
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String format, @Nonnull Object... arguments)
			 public void log( string format, params object[] arguments )
			 {
				  lock ( _outerInstance.@lock )
				  {
						_slf4jLogger.error( outerInstance.convertFormat( format ), arguments );
				  }
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void bulk(@Nonnull Consumer<org.neo4j.logging.Logger> consumer)
			 public void bulk( Consumer<Logger> consumer )
			 {
				  lock ( _outerInstance.@lock )
				  {
						consumer.accept( this );
				  }
			 }
		 }

		 public override bool DebugEnabled
		 {
			 get
			 {
				  return _slf4jLogger.DebugEnabled;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.logging.Logger debugLogger()
		 public override Logger DebugLogger()
		 {
			  return this._debugLogger;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.logging.Logger infoLogger()
		 public override Logger InfoLogger()
		 {
			  return this._infoLogger;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.logging.Logger warnLogger()
		 public override Logger WarnLogger()
		 {
			  return this._warnLogger;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.logging.Logger errorLogger()
		 public override Logger ErrorLogger()
		 {
			  return this._errorLogger;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void bulk(@Nonnull Consumer<org.neo4j.logging.Log> consumer)
		 public override void Bulk( Consumer<Log> consumer )
		 {
			  lock ( @lock )
			  {
					consumer.accept( this );
			  }
		 }

		 private string ConvertFormat( string format )
		 {
			  return format.Replace( "%s", "{}" );
		 }
	}

}