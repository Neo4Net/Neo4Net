﻿/*
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
	using RecordMessage = Org.Neo4j.Bolt.v1.messaging.response.RecordMessage;
	using AnyValue = Org.Neo4j.Values.AnyValue;

	public class RecordMessageEncoder : ResponseMessageEncoder<RecordMessage>
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void encode(org.neo4j.bolt.messaging.Neo4jPack_Packer packer, org.neo4j.bolt.v1.messaging.response.RecordMessage message) throws java.io.IOException
		 public override void Encode( Org.Neo4j.Bolt.messaging.Neo4jPack_Packer packer, RecordMessage message )
		 {
			  AnyValue[] fields = message.Fields();
			  packer.PackStructHeader( 1, message.Signature() );
			  packer.PackListHeader( fields.Length );
			  foreach ( AnyValue field in fields )
			  {
					packer.Pack( field );
			  }
		 }
	}

}