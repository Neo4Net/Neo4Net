using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.protocol.handshake
{

	using Neo4Net.Helpers.Collections;

	public class SwitchOverRequest : ServerMessage
	{
		 private readonly string _protocolName;
		 private readonly int? _version;
		 private readonly IList<Pair<string, string>> _modifierProtocols;

		 public SwitchOverRequest( string applicationProtocolName, int applicationProtocolVersion, IList<Pair<string, string>> modifierProtocols )
		 {
			  this._protocolName = applicationProtocolName;
			  this._version = applicationProtocolVersion;
			  this._modifierProtocols = modifierProtocols;
		 }

		 public override void Dispatch( ServerMessageHandler handler )
		 {
			  handler.Handle( this );
		 }

		 public virtual string ProtocolName()
		 {
			  return _protocolName;
		 }

		 public virtual IList<Pair<string, string>> ModifierProtocols()
		 {
			  return _modifierProtocols;
		 }

		 public virtual int Version()
		 {
			  return _version.Value;
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
			  SwitchOverRequest that = ( SwitchOverRequest ) o;
			  return Objects.Equals( _version, that._version ) && Objects.Equals( _protocolName, that._protocolName ) && Objects.Equals( _modifierProtocols, that._modifierProtocols );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _protocolName, _version, _modifierProtocols );
		 }

		 public override string ToString()
		 {
			  return "SwitchOverRequest{" + "protocolName='" + _protocolName + '\'' + ", version=" + _version + ", modifierProtocols=" + _modifierProtocols + '}';
		 }
	}

}