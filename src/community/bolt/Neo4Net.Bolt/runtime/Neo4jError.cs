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
namespace Neo4Net.Bolt.runtime
{

	using DatabaseShutdownException = Neo4Net.Graphdb.DatabaseShutdownException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// An error object, represents something having gone wrong that is to be signaled to the user. This is, by design, not
	/// using the java exception system.
	/// </summary>
	public class Neo4jError
	{
		 private readonly Status _status;
		 private readonly string _message;
		 private readonly Exception _cause;
		 private readonly System.Guid _reference;
		 private readonly bool _fatal;

		 private Neo4jError( Status status, string message, Exception cause, bool fatal )
		 {
			  this._status = status;
			  this._message = message;
			  this._cause = cause;
			  this._fatal = fatal;
			  this._reference = System.Guid.randomUUID();
		 }

		 private Neo4jError( Status status, string message, bool fatal ) : this( status, message, null, fatal )
		 {
		 }

		 private Neo4jError( Status status, Exception cause, bool fatal ) : this( status, status.Code().description(), cause, fatal )
		 {
		 }

		 public virtual Status Status()
		 {
			  return _status;
		 }

		 public virtual string Message()
		 {
			  return _message;
		 }

		 public virtual Exception Cause()
		 {
			  return _cause;
		 }

		 public virtual System.Guid Reference()
		 {
			  return _reference;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  Neo4jError that = ( Neo4jError ) o;

			  return ( _status != null ? _status.Equals( that._status ) : that._status == null ) && !( !string.ReferenceEquals( _message, null ) ?!_message.Equals( that._message ) :!string.ReferenceEquals( that._message, null ) );

		 }

		 public override int GetHashCode()
		 {
			  int result = _status != null ? _status.GetHashCode() : 0;
			  result = 31 * result + ( !string.ReferenceEquals( _message, null ) ? _message.GetHashCode() : 0 );
			  return result;
		 }

		 public override string ToString()
		 {
			  return "Neo4jError{" +
						 "status=" + _status +
						 ", message='" + _message + '\'' +
						 ", cause=" + _cause +
						 ", reference=" + _reference +
						 '}';
		 }

		 public static Status CodeFromString( string codeStr )
		 {
			  string[] parts = codeStr.Split( "\\.", true );
			  if ( parts.Length != 4 )
			  {
					return Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError;
			  }

			  string category = parts[2];
			  string error = parts[3];

			  // Note: the input string may contain arbitrary input data, using reflection would open network attack vector
			  switch ( category )
			  {
			  case "Schema":
					return Neo4Net.Kernel.Api.Exceptions.Status_Schema.valueOf( error );
			  case "LegacyIndex":
					return Neo4Net.Kernel.Api.Exceptions.Status_LegacyIndex.valueOf( error );
			  case "General":
					return Neo4Net.Kernel.Api.Exceptions.Status_General.valueOf( error );
			  case "Statement":
					return Neo4Net.Kernel.Api.Exceptions.Status_Statement.valueOf( error );
			  case "Transaction":
					return Neo4Net.Kernel.Api.Exceptions.Status_Transaction.valueOf( error );
			  case "Request":
					return Neo4Net.Kernel.Api.Exceptions.Status_Request.valueOf( error );
			  case "Network":
					return Neo4Net.Kernel.Api.Exceptions.Status_Network.valueOf( error );
			  case "Security":
					return Neo4Net.Kernel.Api.Exceptions.Status_Security.valueOf( error );
			  default:
					return Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError;
			  }
		 }

		 private static Neo4jError FromThrowable( Exception any, bool isFatal )
		 {
			  for ( Exception cause = any; cause != null; cause = cause.InnerException )
			  {
					if ( cause is DatabaseShutdownException )
					{
						 return new Neo4jError( Neo4Net.Kernel.Api.Exceptions.Status_General.DatabaseUnavailable, cause, isFatal );
					}
					if ( _cause is Neo4Net.Kernel.Api.Exceptions.Status_HasStatus )
					{
						 return new Neo4jError( ( ( Neo4Net.Kernel.Api.Exceptions.Status_HasStatus ) _cause ).status(), any.Message, any, isFatal );
					}
					if ( _cause is System.OutOfMemoryException )
					{
						 return new Neo4jError( Neo4Net.Kernel.Api.Exceptions.Status_General.OutOfMemoryError, _cause, isFatal );
					}
					if ( _cause is StackOverflowError )
					{
						 return new Neo4jError( Neo4Net.Kernel.Api.Exceptions.Status_General.StackOverFlowError, _cause, isFatal );
					}
			  }

			  // In this case, an error has "slipped out", and we don't have a good way to handle it. This indicates
			  // a buggy code path, and we need to try to convince whoever ends up here to tell us about it.

			  return new Neo4jError( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError, any != null ? any.Message : null, any, isFatal );
		 }

		 public static Neo4jError From( Status status, string message )
		 {
			  return new Neo4jError( status, message, false );
		 }

		 public static Neo4jError From( Exception any )
		 {
			  return FromThrowable( any, false );
		 }

		 public static Neo4jError FatalFrom( Exception any )
		 {
			  return FromThrowable( any, true );
		 }

		 public static Neo4jError FatalFrom( Status status, string message )
		 {
			  return new Neo4jError( status, message, true );
		 }

		 public virtual bool Fatal
		 {
			 get
			 {
				  return _fatal;
			 }
		 }
	}

}