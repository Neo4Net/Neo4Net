using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.cluster.protocol
{
	using ArgumentMatcher = org.mockito.ArgumentMatcher;


	using Org.Neo4j.cluster.com.message;
	using MessageType = Org.Neo4j.cluster.com.message.MessageType;

	public class MessageArgumentMatcher<T> : ArgumentMatcher<Message<T>> where T : Org.Neo4j.cluster.com.message.MessageType
	{
		 private URI _from;
		 private URI _to;
		 private T _theMessageType;
		 private Serializable _payload;
		 private readonly IList<string> _headers = new LinkedList<string>();
		 private readonly IList<string> _headerValues = new LinkedList<string>();

		 public virtual MessageArgumentMatcher<T> From( URI from )
		 {
			  this._from = from;
			  return this;
		 }

		 public virtual MessageArgumentMatcher<T> To( URI to )
		 {
			  this._to = to;
			  return this;
		 }

		 public virtual MessageArgumentMatcher<T> OnMessageType( T messageType )
		 {
			  this._theMessageType = messageType;
			  return this;
		 }

		 public virtual MessageArgumentMatcher<T> WithPayload( Serializable payload )
		 {
			  this._payload = payload;
			  return this;
		 }

		 /// <summary>
		 /// Use this for matching on headers other than HEADER_TO and HEADER_FROM, for which there are dedicated methods. The value
		 /// of the header is mandatory - if you don't care about it, set it to the empty string.
		 /// </summary>
		 public virtual MessageArgumentMatcher<T> WithHeader( string headerName, string headerValue )
		 {
			  this._headers.Add( headerName );
			  this._headerValues.Add( headerValue );
			  return this;
		 }

		 public override bool Matches( Message<T> message )
		 {
			  if ( message == null )
			  {
					return false;
			  }
			  bool toMatches = _to == null || _to.ToString().Equals(message.GetHeader(Message.HEADER_TO));
			  bool fromMatches = _from == null || _from.ToString().Equals(message.GetHeader(Message.HEADER_FROM));
			  bool typeMatches = _theMessageType == null || _theMessageType == ( ( Message ) message ).MessageType;
			  bool payloadMatches = _payload == null || _payload.Equals( message.Payload );
			  bool headersMatch = true;
			  foreach ( string header in _headers )
			  {
					headersMatch = headersMatch && MatchHeaderAndValue( header, message.GetHeader( header ) );
			  }
			  return fromMatches && toMatches && typeMatches && payloadMatches && headersMatch;
		 }

		 private bool MatchHeaderAndValue( string headerName, string headerValue )
		 {
			  int headerIndex = _headers.IndexOf( headerName );
			  if ( headerIndex == -1 )
			  {
					// Header not present
					return false;
			  }
			  if ( _headerValues[headerIndex].Equals( "" ) )
			  {
					// header name was present and value does not matter
					return true;
			  }
			  return _headerValues[headerIndex].Equals( headerValue );
		 }

		 public override string ToString()
		 {
			  return ( _theMessageType != null ? _theMessageType.name() : "<no particular message type>" ) + "{from=" + _from +
						 ", to=" + _to + ", payload=" + _payload + "}";
		 }
	}

}