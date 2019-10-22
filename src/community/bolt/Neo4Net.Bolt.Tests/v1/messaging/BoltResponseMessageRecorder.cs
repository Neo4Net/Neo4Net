using System.Collections.Generic;

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
namespace Neo4Net.Bolt.v1.messaging
{

	using ResponseMessage = Neo4Net.Bolt.messaging.ResponseMessage;
	using BoltResponseMessageWriter = Neo4Net.Bolt.messaging.BoltResponseMessageWriter;

	public class BoltResponseMessageRecorder : BoltResponseMessageWriter
	{
		 private readonly IList<ResponseMessage> _messages = new List<ResponseMessage>();

		 public virtual IList<ResponseMessage> AsList()
		 {
			  return _messages;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(org.Neo4Net.bolt.messaging.ResponseMessage message) throws java.io.IOException
		 public override void Write( ResponseMessage message )
		 {
			  _messages.Add( message );
		 }
	}

}