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

	public class InitialMagicMessage : ServerMessage, ClientMessage
	{
		 // these can never, ever change
		 internal const string CORRECT_MAGIC_VALUE = "Neo4Net_CLUSTER";
		 internal const int MESSAGE_CODE = 0x344F454E; // ASCII/UTF-8 "NEO4"

		 private readonly string _magic;
		 // TODO: clusterId (String?)

		 private static readonly InitialMagicMessage _instance = new InitialMagicMessage( CORRECT_MAGIC_VALUE );

		 internal InitialMagicMessage( string magic )
		 {
			  this._magic = magic;
		 }

		 public static InitialMagicMessage Instance()
		 {
			  return _instance;
		 }

		 public override void Dispatch( ServerMessageHandler handler )
		 {
			  handler.Handle( this );
		 }

		 public override void Dispatch( ClientMessageHandler handler )
		 {
			  handler.Handle( this );
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
			  InitialMagicMessage that = ( InitialMagicMessage ) o;
			  return Objects.Equals( _magic, that._magic );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _magic );
		 }

		 internal virtual bool CorrectMagic
		 {
			 get
			 {
				  return _magic.Equals( CORRECT_MAGIC_VALUE );
			 }
		 }

		 public virtual string Magic()
		 {
			  return _magic;
		 }

		 public override string ToString()
		 {
			  return "InitialMagicMessage{" + "magic='" + _magic + '\'' + '}';
		 }
	}

}