﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.causalclustering.protocol.handshake
{

	public class SwitchOverResponse : ClientMessage
	{
		 public static readonly SwitchOverResponse Failure = new SwitchOverResponse( StatusCode.Failure );
		 private readonly StatusCode _status;

		 internal SwitchOverResponse( StatusCode status )
		 {
			  this._status = status;
		 }

		 public override void Dispatch( ClientMessageHandler handler )
		 {
			  handler.Handle( this );
		 }

		 public virtual StatusCode Status()
		 {
			  return _status;
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
			  SwitchOverResponse that = ( SwitchOverResponse ) o;
			  return _status == that._status;
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _status );
		 }

		 public override string ToString()
		 {
			  return "SwitchOverResponse{" + "status=" + _status + '}';
		 }
	}

}