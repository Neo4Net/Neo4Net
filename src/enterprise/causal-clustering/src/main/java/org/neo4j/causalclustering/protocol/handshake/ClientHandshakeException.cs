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

	using Neo4Net.causalclustering.protocol;
	using Neo4Net.Helpers.Collections;

	public class ClientHandshakeException : Exception
	{
		 public ClientHandshakeException( string message ) : base( message )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public ClientHandshakeException(String message, @Nullable Protocol.ApplicationProtocol negotiatedApplicationProtocol, java.util.List<org.Neo4Net.helpers.collection.Pair<String,java.util.Optional<org.Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol>>> negotiatedModifierProtocols)
		 public ClientHandshakeException( string message, Protocol.ApplicationProtocol negotiatedApplicationProtocol, IList<Pair<string, Optional<Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol>>> negotiatedModifierProtocols ) : base( message + " Negotiated application protocol: " + negotiatedApplicationProtocol + " Negotiated modifier protocols: " + negotiatedModifierProtocols )
		 {
		 }
	}

}