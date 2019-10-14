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
namespace Neo4Net.Bolt.v1.messaging.encoder
{

	using Neo4jPack = Neo4Net.Bolt.messaging.Neo4jPack;
	using Neo4Net.Bolt.messaging;
	using SuccessMessage = Neo4Net.Bolt.v1.messaging.response.SuccessMessage;

	public class SuccessMessageEncoder : ResponseMessageEncoder<SuccessMessage>
	{
		 public SuccessMessageEncoder()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void encode(org.neo4j.bolt.messaging.Neo4jPack_Packer packer, org.neo4j.bolt.v1.messaging.response.SuccessMessage message) throws java.io.IOException
		 public override void Encode( Neo4Net.Bolt.messaging.Neo4jPack_Packer packer, SuccessMessage message )
		 {
			  packer.PackStructHeader( 1, message.Signature() );
			  packer.pack( message.Meta() );
		 }
	}

}