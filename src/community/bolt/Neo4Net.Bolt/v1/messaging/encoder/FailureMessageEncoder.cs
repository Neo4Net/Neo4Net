﻿/*
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
namespace Neo4Net.Bolt.v1.messaging.encoder
{

	using Neo4NetPack = Neo4Net.Bolt.messaging.Neo4NetPack;
	using Neo4Net.Bolt.messaging;
	using FailureMessage = Neo4Net.Bolt.v1.messaging.response.FailureMessage;
	using FatalFailureMessage = Neo4Net.Bolt.v1.messaging.response.FatalFailureMessage;
	using Log = Neo4Net.Logging.Log;

	public class FailureMessageEncoder : ResponseMessageEncoder<FailureMessage>
	{
		 private readonly Log _log;

		 public FailureMessageEncoder( Log log )
		 {
			  this._log = log;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void encode(org.Neo4Net.bolt.messaging.Neo4NetPack_Packer packer, org.Neo4Net.bolt.v1.messaging.response.FailureMessage message) throws java.io.IOException
		 public override void Encode( Neo4Net.Bolt.messaging.Neo4NetPack_Packer packer, FailureMessage message )
		 {
			  if ( message is FatalFailureMessage )
			  {
					_log.debug( "Encoding a fatal failure message to send. Message: %s", message );
			  }
			  EncodeFailure( message, packer );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void encodeFailure(org.Neo4Net.bolt.v1.messaging.response.FailureMessage message, org.Neo4Net.bolt.messaging.Neo4NetPack_Packer packer) throws java.io.IOException
		 private void EncodeFailure( FailureMessage message, Neo4Net.Bolt.messaging.Neo4NetPack_Packer packer )
		 {
			  packer.PackStructHeader( 1, message.Signature() );
			  packer.PackMapHeader( 2 );

			  packer.Pack( "code" );
			  packer.Pack( message.Status().code().serialize() );

			  packer.Pack( "message" );
			  packer.Pack( message.Message() );
		 }
	}

}