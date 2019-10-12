/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Bolt.v1.messaging.encoder
{

	using Neo4jPack = Org.Neo4j.Bolt.messaging.Neo4jPack;
	using Org.Neo4j.Bolt.messaging;
	using IgnoredMessage = Org.Neo4j.Bolt.v1.messaging.response.IgnoredMessage;

	public class IgnoredMessageEncoder : ResponseMessageEncoder<IgnoredMessage>
	{
		 public IgnoredMessageEncoder()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void encode(org.neo4j.bolt.messaging.Neo4jPack_Packer packer, org.neo4j.bolt.v1.messaging.response.IgnoredMessage message) throws java.io.IOException
		 public override void Encode( Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer, IgnoredMessage message )
		 {
			  packer.PackStructHeader( 0, message.Signature() );
		 }
	}

}