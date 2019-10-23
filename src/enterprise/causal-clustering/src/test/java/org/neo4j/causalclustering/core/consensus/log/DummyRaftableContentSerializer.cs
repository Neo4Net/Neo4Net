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
namespace Neo4Net.causalclustering.core.consensus.log
{

	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using Neo4Net.causalclustering.core.state.storage;
	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;
	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;

	public class DummyRaftableContentSerializer : SafeChannelMarshal<ReplicatedContent>
	{
		 private const int REPLICATED_INTEGER_TYPE = 0;
		 private const int REPLICATED_STRING_TYPE = 1;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(org.Neo4Net.causalclustering.core.replication.ReplicatedContent content, org.Neo4Net.Kernel.Api.StorageEngine.WritableChannel channel) throws java.io.IOException
		 public override void Marshal( ReplicatedContent content, WritableChannel channel )
		 {
			  if ( content is ReplicatedInteger )
			  {
					channel.Put( ( sbyte ) REPLICATED_INTEGER_TYPE );
					channel.PutInt( ( ( ReplicatedInteger ) content ).get() );
			  }
			  else if ( content is ReplicatedString )
			  {
					string value = ( ( ReplicatedString ) content ).get();
					sbyte[] stringBytes = value.GetBytes();
					channel.Put( ( sbyte ) REPLICATED_STRING_TYPE );
					channel.PutInt( stringBytes.Length );
					channel.Put( stringBytes, stringBytes.Length );
			  }
			  else
			  {
					throw new System.ArgumentException( "Unknown content type: " + content );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.causalclustering.core.replication.ReplicatedContent unmarshal0(org.Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException
		 protected internal override ReplicatedContent Unmarshal0( ReadableChannel channel )
		 {
			  sbyte type = channel.Get();
			  switch ( type )
			  {
			  case REPLICATED_INTEGER_TYPE:
					return ReplicatedInteger.ValueOf( channel.Int );
			  case REPLICATED_STRING_TYPE:
					int length = channel.Int;
					sbyte[] bytes = new sbyte[length];
					channel.Get( bytes, length );
					return ReplicatedString.ValueOf( StringHelper.NewString( bytes ) );
			  default:
					throw new System.ArgumentException( "Unknown content type: " + type );
			  }
		 }
	}

}