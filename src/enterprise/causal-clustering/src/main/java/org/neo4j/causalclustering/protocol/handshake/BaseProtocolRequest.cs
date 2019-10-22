using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.protocol.handshake
{

	public abstract class BaseProtocolRequest<IMPL> : ServerMessage where IMPL : IComparable<IMPL>
	{
		public abstract void Dispatch( ServerMessageHandler handler );
		 private readonly string _protocolName;
		 private readonly ISet<IMPL> _versions;

		 internal BaseProtocolRequest( string protocolName, ISet<IMPL> versions )
		 {
			  this._protocolName = protocolName;
			  this._versions = versions;
		 }

		 public virtual string ProtocolName()
		 {
			  return _protocolName;
		 }

		 public virtual ISet<IMPL> Versions()
		 {
			  return _versions;
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
			  BaseProtocolRequest that = ( BaseProtocolRequest ) o;
			  return Objects.Equals( _protocolName, that._protocolName ) && Objects.Equals( _versions, that._versions );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _protocolName, _versions );
		 }

		 public override string ToString()
		 {
			  return "BaseProtocolRequest{" + "protocolName='" + _protocolName + '\'' + ", versions=" + _versions + '}';
		 }
	}

}