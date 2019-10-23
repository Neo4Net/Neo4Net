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
namespace Neo4Net.causalclustering.core.state
{

	using Neo4Net.causalclustering.core.state.storage;
	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;
	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;

	/// <summary>
	/// A marshal for an index that starts with -1 at the empty slot before the first real entry at 0.
	/// </summary>
	public class LongIndexMarshal : SafeStateMarshal<long>
	{
		 public override long? StartState()
		 {
			  return -1L;
		 }

		 public override long Ordinal( long? index )
		 {
			  return index.Value;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(System.Nullable<long> index, org.Neo4Net.Kernel.Api.StorageEngine.WritableChannel channel) throws java.io.IOException
		 public override void Marshal( long? index, WritableChannel channel )
		 {
			  channel.PutLong( index.Value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected System.Nullable<long> unmarshal0(org.Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException
		 protected internal override long? Unmarshal0( ReadableChannel channel )
		 {
			  return channel.Long;
		 }
	}

}