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
namespace Neo4Net.ha.correctness
{

	using Neo4Net.cluster.com.message;
	using MessageType = Neo4Net.cluster.com.message.MessageType;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;

	internal class MessageDeliveryAction : ClusterAction
	{
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 public static readonly System.Func<Message, ClusterAction> MessageToAction = MessageDeliveryAction::new;

		 private readonly Message _message;

		 internal MessageDeliveryAction( Message message )
		 {
			  this._message = message;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Iterable<ClusterAction> perform(ClusterState state) throws java.net.URISyntaxException
		 public override IEnumerable<ClusterAction> Perform( ClusterState state )
		 {
			  string to = _message.getHeader( Message.HEADER_TO );
			  return Iterables.map( MessageToAction, state.Instance( to ).process( MessageCopy() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.cluster.com.message.Message<? extends org.Neo4Net.cluster.com.message.MessageType> messageCopy() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private Message<MessageType> MessageCopy()
		 {
			  URI to = new URI( _message.getHeader( Message.HEADER_TO ) );
			  Message<MessageType> copy = Message.to( _message.MessageType, to, _message.Payload );
			  return _message.copyHeadersTo( copy );
		 }

		 public override string ToString()
		 {
			  return "(" + _message.getHeader( Message.HEADER_FROM ) + ")-[" + _message.MessageType.name() + "]->(" + _message.getHeader(Message.HEADER_TO) + ")";
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

			  return MessageEquals( _message, ( ( MessageDeliveryAction ) o )._message );

		 }

		 private bool MessageEquals( Message first, Message other )
		 {
			  if ( !first.MessageType.Equals( other.MessageType ) )
			  {
					return false;
			  }

			  if ( !first.getHeader( Message.HEADER_FROM ).Equals( other.getHeader( Message.HEADER_FROM ) ) )
			  {
					return false;
			  }

			  if ( !first.getHeader( Message.HEADER_TO ).Equals( other.getHeader( Message.HEADER_TO ) ) )
			  {
					return false;
			  }

			  if ( first.Payload is Message && other.Payload is Message )
			  {
					return MessageEquals( ( Message ) first.Payload, ( Message ) other.Payload );
			  }
			  else if ( first.Payload == null )
			  {
					if ( other.Payload != null )
					{
						 return false;
					}
			  }
			  else if ( !first.Payload.Equals( other.Payload ) )
			  {
					return false;
			  }
			  return true;
		 }

		 public override int GetHashCode()
		 {
			  int result = _message.MessageType.GetHashCode();
			  result = 31 * result + _message.getHeader( Message.HEADER_FROM ).GetHashCode();
			  result = 31 * result + _message.getHeader( Message.HEADER_TO ).GetHashCode();
			  return result;
		 }
	}

}