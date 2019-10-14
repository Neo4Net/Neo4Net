/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.ha.@lock
{

	public class LockResult
	{
		 private readonly LockStatus _status;
		 private readonly string _message;

		 public LockResult( LockStatus status )
		 {
			  this._status = status;
			  this._message = null;
		 }

		 public LockResult( LockStatus status, string message )
		 {
			  this._status = status;
			  this._message = message;
		 }

		 public virtual LockStatus Status
		 {
			 get
			 {
				  return _status;
			 }
		 }

		 public virtual string Message
		 {
			 get
			 {
				  return _message;
			 }
		 }

		 public override string ToString()
		 {
			  return "LockResult[" + _status + ", " + _message + "]";
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
			  LockResult that = ( LockResult ) o;
			  return Objects.Equals( _status, that._status ) && Objects.Equals( _message, that._message );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _status, _message );
		 }
	}

}