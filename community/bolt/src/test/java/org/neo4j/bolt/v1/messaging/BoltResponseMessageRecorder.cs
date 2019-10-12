using System.Collections.Generic;

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
namespace Org.Neo4j.Bolt.v1.messaging
{

	using ResponseMessage = Org.Neo4j.Bolt.messaging.ResponseMessage;
	using BoltResponseMessageWriter = Org.Neo4j.Bolt.messaging.BoltResponseMessageWriter;

	public class BoltResponseMessageRecorder : BoltResponseMessageWriter
	{
		 private readonly IList<ResponseMessage> _messages = new List<ResponseMessage>();

		 public virtual IList<ResponseMessage> AsList()
		 {
			  return _messages;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(org.neo4j.bolt.messaging.ResponseMessage message) throws java.io.IOException
		 public override void Write( ResponseMessage message )
		 {
			  _messages.Add( message );
		 }
	}

}