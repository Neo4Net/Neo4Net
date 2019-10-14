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
namespace Neo4Net.Bolt.v1.messaging.response
{
	using ResponseMessage = Neo4Net.Bolt.messaging.ResponseMessage;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	public class FailureMessage : ResponseMessage
	{
		 public const sbyte SIGNATURE = 0x7F;
		 private readonly Status _status;
		 private readonly string _message;

		 public FailureMessage( Status status, string message )
		 {
			  this._status = status;
			  this._message = message;
		 }

		 public virtual Status Status()
		 {
			  return _status;
		 }

		 public virtual string Message()
		 {
			  return _message;
		 }

		 public override sbyte Signature()
		 {
			  return SIGNATURE;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( !( o is FailureMessage ) )
			  {
					return false;
			  }

			  FailureMessage that = ( FailureMessage ) o;

			  return ( !string.ReferenceEquals( _message, null ) ? _message.Equals( that._message ) : string.ReferenceEquals( that._message, null ) ) && ( _status != null ? _status.Equals( that._status ) : that._status == null );
		 }

		 public override int GetHashCode()
		 {
			  int result = _status != null ? _status.GetHashCode() : 0;
			  result = 31 * result + ( !string.ReferenceEquals( _message, null ) ? _message.GetHashCode() : 0 );
			  return result;
		 }

		 public override string ToString()
		 {
			  return "FAILURE " + _status + " " + _message;
		 }

	}

}