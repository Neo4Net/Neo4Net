using System;
using System.Collections.Generic;

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
namespace Neo4Net.Server.rest.transactional.error
{

	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// This is an initial move towards unified errors - it should not live here in the server, but should probably
	/// exist in the kernel or similar, where it can be shared across surfaces other than the server.
	/// <para>
	/// It's put in place here in order to enforce that the <seealso cref="org.neo4j.server.rest.web.TransactionalService"/>
	/// is strictly tied down towards what errors it handles and returns to the client, to create a waterproof abstraction
	/// between the runtime-exception landscape that lives below, and the errors we send to the user.
	/// </para>
	/// <para>
	/// This way, we make it easy to transition this service over to a unified error code based error scheme.
	/// </para>
	/// </summary>
	public class Neo4jError
	{
		 private readonly Status _status;
		 private readonly Exception _cause;

		 public Neo4jError( Status status, string message ) : this( status, new Exception( message ) )
		 {
		 }

		 public Neo4jError( Status status, Exception cause )
		 {
			  if ( status == null )
			  {
					throw new System.ArgumentException( "statusCode must not be null" );
			  }
			  if ( cause == null )
			  {
					throw new System.ArgumentException( "cause must not be null" );
			  }

			  this._status = status;
			  this._cause = cause;
		 }

		 public override string ToString()
		 {
			  Console.WriteLine( _cause.ToString() );
			  Console.Write( _cause.StackTrace );
			  return string.Format( "{0}[{1}, cause=\"{2}\"]", this.GetType().Name, _status.code(), _cause );
		 }

		 public virtual Exception Cause()
		 {
			  return _cause;
		 }

		 public virtual Status Status()
		 {
			  return _status;
		 }

		 public virtual string Message
		 {
			 get
			 {
				  return _cause.Message;
			 }
		 }

		 public virtual bool ShouldSerializeStackTrace()
		 {
			  switch ( _status.code().classification() )
			  {
			  case ClientError:
					return false;
			  default:
					return true;
			  }
		 }

		 public virtual string StackTraceAsString
		 {
			 get
			 {
				  StringWriter stringWriter = new StringWriter();
				  PrintWriter printWriter = new PrintWriter( stringWriter );
				  _cause.printStackTrace( printWriter );
				  return stringWriter.ToString();
			 }
		 }

		 public static bool ShouldRollBackOn( ICollection<Neo4jError> errors )
		 {
			  if ( errors.Count == 0 )
			  {
					return false;
			  }
			  foreach ( Neo4jError error in errors )
			  {
					if ( error.Status().code().classification().rollbackTransaction() )
					{
						 return true;
					}
			  }
			  return false;
		 }
	}

}