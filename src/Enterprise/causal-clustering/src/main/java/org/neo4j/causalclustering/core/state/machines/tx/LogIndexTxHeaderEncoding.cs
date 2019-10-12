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
namespace Neo4Net.causalclustering.core.state.machines.tx
{
	/// <summary>
	/// Log index is encoded in the header of transactions in the transaction log.
	/// </summary>
	public class LogIndexTxHeaderEncoding
	{
		 private LogIndexTxHeaderEncoding()
		 {
		 }

		 public static sbyte[] EncodeLogIndexAsTxHeader( long logIndex )
		 {
			  sbyte[] b = new sbyte[Long.BYTES];
			  for ( int i = Long.BYTES - 1; i > 0; i-- )
			  {
					b[i] = ( sbyte ) logIndex;
					logIndex = ( long )( ( ulong )logIndex >> ( sizeof( sbyte ) * 8 ) );
			  }
			  b[0] = ( sbyte ) logIndex;
			  return b;
		 }

		 public static long DecodeLogIndexFromTxHeader( sbyte[] bytes )
		 {
			  if ( bytes.Length < Long.BYTES )
			  {
					throw new System.ArgumentException( "Unable to decode RAFT log index from transaction header" );
			  }

			  long logIndex = 0;
			  for ( int i = 0; i < Long.BYTES; i++ )
			  {
					logIndex <<= ( sizeof( sbyte ) * 8 );
					logIndex ^= bytes[i] & 0xFF;
			  }
			  return logIndex;
		 }
	}

}