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
namespace Neo4Net.Kernel.@internal
{
	using ErrorState = Neo4Net.Graphdb.@event.ErrorState;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using DatabasePanicEventGenerator = Neo4Net.Kernel.impl.core.DatabasePanicEventGenerator;
	using Log = Neo4Net.Logging.Log;

	public class DatabaseHealth
	{
		 private static readonly string _panicMessage = "The database has encountered a critical error, " +
					"and needs to be restarted. Please see database logs for more details.";
		 private static readonly Type[] _criticalExceptions = new Type[]{ typeof( System.OutOfMemoryException ) };

		 private volatile bool _healthy = true;
		 private readonly DatabasePanicEventGenerator _dbpe;
		 private readonly Log _log;
		 private Exception _causeOfPanic;

		 public DatabaseHealth( DatabasePanicEventGenerator dbpe, Log log )
		 {
			  this._dbpe = dbpe;
			  this._log = log;
		 }

		 /// <summary>
		 /// Asserts that the database is in good health. If that is not the case then the cause of the
		 /// unhealthy state is wrapped in an exception of the given type, i.e. the panic disguise.
		 /// </summary>
		 /// <param name="panicDisguise"> the cause of the unhealthy state wrapped in an exception of this type. </param>
		 /// <exception cref="EXCEPTION"> exception type to wrap cause in. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <EXCEPTION extends Throwable> void assertHealthy(Class<EXCEPTION> panicDisguise) throws EXCEPTION
		 public virtual void AssertHealthy<EXCEPTION>( Type panicDisguise ) where EXCEPTION : Exception
		 {
				 panicDisguise = typeof( EXCEPTION );
			  if ( !_healthy )
			  {
					EXCEPTION exception;
					try
					{
						 try
						 {
							  exception = panicDisguise.GetConstructor( typeof( string ), typeof( Exception ) ).newInstance( _panicMessage, _causeOfPanic );
						 }
						 catch ( NoSuchMethodException )
						 {
							  exception = panicDisguise.GetConstructor( typeof( string ) ).newInstance( _panicMessage );
							  try
							  {
									exception.initCause( _causeOfPanic );
							  }
							  catch ( System.InvalidOperationException )
							  {
							  }
						 }
					}
					catch ( Exception e )
					{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						 throw new Exception( _panicMessage + ". An exception of type " + panicDisguise.FullName + " was requested to be thrown but that proved impossible", e );
					}
					throw exception;
			  }
		 }

		 public virtual void Panic( Exception cause )
		 {
			  if ( !_healthy )
			  {
					return;
			  }

			  if ( cause == null )
			  {
					throw new System.ArgumentException( "Must provide a cause for the database panic" );
			  }
			  this._causeOfPanic = cause;
			  this._healthy = false;
			  _log.error( "Database panic: " + _panicMessage, cause );
			  _dbpe.generateEvent( ErrorState.TX_MANAGER_NOT_OK, _causeOfPanic );
		 }

		 public virtual bool Healthy
		 {
			 get
			 {
				  return _healthy;
			 }
		 }

		 public virtual bool Healed()
		 {
			  if ( HasCriticalFailure() )
			  {
					_log.error( "Database encountered a critical error and can't be healed. Restart required." );
					return false;
			  }
			  else
			  {
					_healthy = true;
					_causeOfPanic = null;
					_log.info( "Database health set to OK" );
					return true;
			  }
		 }

		 private bool HasCriticalFailure()
		 {
			  return !Healthy && Exceptions.contains( _causeOfPanic, _criticalExceptions );
		 }

		 public virtual Exception Cause()
		 {
			  return _causeOfPanic;
		 }
	}

}