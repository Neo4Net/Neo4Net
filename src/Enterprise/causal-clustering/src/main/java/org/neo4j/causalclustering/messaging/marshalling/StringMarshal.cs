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
namespace Neo4Net.causalclustering.messaging.marshalling
{
	using ByteBuf = io.netty.buffer.ByteBuf;

	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

	public class StringMarshal
	{
		 private const int NULL_STRING_LENGTH = -1;

		 private StringMarshal()
		 {
		 }

		 public static void Marshal( ByteBuf buffer, string @string )
		 {
			  if ( string.ReferenceEquals( @string, null ) )
			  {
					buffer.writeInt( NULL_STRING_LENGTH );
			  }
			  else
			  {
					sbyte[] bytes = @string.GetBytes( UTF_8 );
					buffer.writeInt( bytes.Length );
					buffer.writeBytes( bytes );
			  }
		 }

		 public static void Marshal( ByteBuffer buffer, string @string )
		 {
			  if ( string.ReferenceEquals( @string, null ) )
			  {
					buffer.putInt( NULL_STRING_LENGTH );
			  }
			  else
			  {
					sbyte[] bytes = @string.GetBytes( UTF_8 );
					buffer.putInt( bytes.Length );
					buffer.put( bytes );
			  }
		 }

		 public static string Unmarshal( ByteBuf buffer )
		 {
			  int len = buffer.readInt();
			  if ( len == NULL_STRING_LENGTH )
			  {
					return null;
			  }

			  sbyte[] bytes = new sbyte[len];
			  buffer.readBytes( bytes );
			  return StringHelper.NewString( bytes, UTF_8 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void marshal(org.neo4j.storageengine.api.WritableChannel channel, String string) throws java.io.IOException
		 public static void Marshal( WritableChannel channel, string @string )
		 {
			  if ( string.ReferenceEquals( @string, null ) )
			  {
					channel.PutInt( NULL_STRING_LENGTH );
			  }
			  else
			  {
					sbyte[] bytes = @string.GetBytes( UTF_8 );
					channel.PutInt( bytes.Length );
					channel.Put( bytes, bytes.Length );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static String unmarshal(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
		 public static string Unmarshal( ReadableChannel channel )
		 {
			  int len = channel.Int;
			  if ( len == NULL_STRING_LENGTH )
			  {
					return null;
			  }

			  sbyte[] stringBytes = new sbyte[len];
			  channel.Get( stringBytes, stringBytes.Length );

			  return StringHelper.NewString( stringBytes, UTF_8 );
		 }
	}

}