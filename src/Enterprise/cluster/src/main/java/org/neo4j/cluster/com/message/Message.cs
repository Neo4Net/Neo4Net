using System;
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
namespace Neo4Net.cluster.com.message
{

	/// <summary>
	/// Message for state machines which can be sent out to instances in the cluster as well.
	/// <para>
	/// These are typically produced and consumed by a <seealso cref="org.neo4j.cluster.statemachine.StateMachine"/>.
	/// </para>
	/// </summary>
	[Serializable]
	public class Message<MESSAGETYPE> where MESSAGETYPE : MessageType
	{
		 private const long SERIAL_VERSION_UID = 7043669983188264476L;

		 public static Message<MESSAGETYPE> To<MESSAGETYPE>( MESSAGETYPE messageType, URI to ) where MESSAGETYPE : MessageType
		 {
			  return to( messageType, to, null );
		 }

		 public static Message<MESSAGETYPE> To<MESSAGETYPE>( MESSAGETYPE messageType, URI to, object payload ) where MESSAGETYPE : MessageType
		 {
			  return ( new Message<MESSAGETYPE>( messageType, payload ) ).SetHeader( HEADER_TO, to.ToString() );
		 }

		 public static Message<MESSAGETYPE> Respond<MESSAGETYPE, T1>( MESSAGETYPE messageType, Message<T1> message, object payload ) where MESSAGETYPE : MessageType
		 {
			  return message.HasHeader( Message.HEADER_FROM ) ? ( new Message<MESSAGETYPE>( messageType, payload ) ).SetHeader( HEADER_TO, message.GetHeader( Message.HEADER_FROM ) ) : Internal( messageType, payload );
		 }

		 public static Message<MESSAGETYPE> Internal<MESSAGETYPE>( MESSAGETYPE message ) where MESSAGETYPE : MessageType
		 {
			  return Internal( message, null );
		 }

		 public static Message<MESSAGETYPE> Internal<MESSAGETYPE>( MESSAGETYPE message, object payload ) where MESSAGETYPE : MessageType
		 {
			  return new Message<MESSAGETYPE>( message, payload );
		 }

		 public static Message<MESSAGETYPE> Timeout<MESSAGETYPE, T1>( MESSAGETYPE message, Message<T1> causedBy ) where MESSAGETYPE : MessageType
		 {
			  return Timeout( message, causedBy, null );
		 }

		 public static Message<MESSAGETYPE> Timeout<MESSAGETYPE, T1>( MESSAGETYPE message, Message<T1> causedBy, object payload ) where MESSAGETYPE : MessageType
		 {
			  Message<MESSAGETYPE> timeout = causedBy.CopyHeadersTo( new Message<MESSAGETYPE>( message, payload ), Message.HEADER_CONVERSATION_ID, Message.HEADER_CREATED_BY );
			  int timeoutCount = 0;
			  if ( causedBy.HasHeader( HEADER_TIMEOUT_COUNT ) )
			  {
					timeoutCount = int.Parse( causedBy.GetHeader( HEADER_TIMEOUT_COUNT ) ) + 1;
			  }
			  timeout.SetHeader( HEADER_TIMEOUT_COUNT, "" + timeoutCount );
			  return timeout;
		 }

		 // Standard headers
		 public const string HEADER_CONVERSATION_ID = "conversation-id";
		 public const string HEADER_CREATED_BY = "created-by";
		 public const string HEADER_TIMEOUT_COUNT = "timeout-count";
		 public const string HEADER_FROM = "from";
		 public const string HEADER_TO = "to";
		 public const string HEADER_INSTANCE_ID = "instance-id";
		 // Should be present only in configurationRequest messages. Value is a comma separated list of instance ids.
		 // Added in 3.0.9.
		 public const string DISCOVERED = "discovered";

		 private MESSAGETYPE _messageType;
		 private object _payload;
		 private IDictionary<string, string> _headers = new Dictionary<string, string>();

		 protected internal Message( MESSAGETYPE messageType, object payload )
		 {
			  this._messageType = messageType;
			  this._payload = payload;
		 }

		 public virtual MESSAGETYPE MessageType
		 {
			 get
			 {
				  return _messageType;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public <T> T getPayload()
		 public virtual T getPayload<T>()
		 {
			 get
			 {
				  return ( T ) _payload;
			 }
		 }

		 public virtual Message<MESSAGETYPE> SetHeader( string name, string value )
		 {
			  if ( string.ReferenceEquals( value, null ) )
			  {
					throw new System.ArgumentException( string.Format( "Header {0} may not be set to null", name ) );
			  }

			  _headers[name] = value;
			  return this;
		 }

		 public virtual bool HasHeader( string name )
		 {
			  return _headers.ContainsKey( name );
		 }

		 public virtual bool Internal
		 {
			 get
			 {
				  return !_headers.ContainsKey( Message.HEADER_TO );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getHeader(String name) throws IllegalArgumentException
		 public virtual string GetHeader( string name )
		 {
			  string value = GetHeader( name, null );
			  if ( string.ReferenceEquals( value, null ) )
			  {
					throw new System.ArgumentException( "No such header:" + name );
			  }
			  return value;
		 }

		 public virtual string GetHeader( string name, string defaultValue )
		 {
			  string value = _headers[name];
			  if ( string.ReferenceEquals( value, null ) )
			  {
					return defaultValue;
			  }
			  return value;
		 }

		 public virtual Message<MSGTYPE> CopyHeadersTo<MSGTYPE>( Message<MSGTYPE> message, params string[] names ) where MSGTYPE : MessageType
		 {
			  if ( names.Length == 0 )
			  {
					foreach ( KeyValuePair<string, string> header in _headers.SetOfKeyValuePairs() )
					{
						 if ( !message.HasHeader( header.Key ) )
						 {
							  message.SetHeader( header.Key, header.Value );
						 }
					}
			  }
			  else
			  {
					foreach ( string name in names )
					{
						 string value = _headers[name];
						 if ( !string.ReferenceEquals( value, null ) && !message.HasHeader( name ) )
						 {
							  message.SetHeader( name, value );
						 }
					}
			  }
			  return message;
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

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Message<?> message = (Message<?>) o;
			  Message<object> message = ( Message<object> ) o;

			  if ( _headers != null ?!_headers.Equals( message._headers ) : message._headers != null )
			  {
					return false;
			  }
			  if ( _messageType != null ?!_messageType.Equals( message._messageType ) : message._messageType != null )
			  {
					return false;
			  }
			  return _payload != null ? _payload.Equals( message._payload ) : message._payload == null;
		 }

		 public override int GetHashCode()
		 {
			  int result = _messageType != null ? _messageType.GetHashCode() : 0;
			  result = 31 * result + ( _payload != null ? _payload.GetHashCode() : 0 );
			  result = 31 * result + ( _headers != null ? _headers.GetHashCode() : 0 );
			  return result;
		 }

		 public override string ToString()
		 {
			  return _messageType.name() + _headers + (_payload is string ? ": " + _payload : "");
		 }
	}

}