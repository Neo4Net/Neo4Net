using System;

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
namespace Neo4Net.Kernel.impl.transaction
{
	using Test = org.junit.Test;

	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using TransactionMetadataCache = Neo4Net.Kernel.impl.transaction.log.TransactionMetadataCache;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class TransactionMetadataCacheTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNullWhenMissingATxInTheCache()
		 public virtual void ShouldReturnNullWhenMissingATxInTheCache()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.TransactionMetadataCache cache = new Neo4Net.kernel.impl.transaction.log.TransactionMetadataCache();
			  TransactionMetadataCache cache = new TransactionMetadataCache();

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.TransactionMetadataCache.TransactionMetadata metadata = cache.getTransactionMetadata(42);
			  TransactionMetadataCache.TransactionMetadata metadata = cache.GetTransactionMetadata( 42 );

			  // then
			  assertNull( metadata );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTheTxValueTIfInTheCached()
		 public virtual void ShouldReturnTheTxValueTIfInTheCached()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.TransactionMetadataCache cache = new Neo4Net.kernel.impl.transaction.log.TransactionMetadataCache();
			  TransactionMetadataCache cache = new TransactionMetadataCache();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.LogPosition position = new Neo4Net.kernel.impl.transaction.log.LogPosition(3, 4);
			  LogPosition position = new LogPosition( 3, 4 );
			  const int txId = 42;
			  const int masterId = 0;
			  const int authorId = 1;
			  const int checksum = 2;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long timestamp = System.currentTimeMillis();
			  long timestamp = DateTimeHelper.CurrentUnixTimeMillis();

			  // when
			  cache.CacheTransactionMetadata( txId, position, masterId, authorId, checksum, timestamp );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.TransactionMetadataCache.TransactionMetadata metadata = cache.getTransactionMetadata(txId);
			  TransactionMetadataCache.TransactionMetadata metadata = cache.GetTransactionMetadata( txId );

			  // then
			  assertEquals( new TransactionMetadataCache.TransactionMetadata( masterId, authorId, position, checksum, timestamp ), metadata );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWhenCachingATxWithNegativeOffsetPosition()
		 public virtual void ShouldThrowWhenCachingATxWithNegativeOffsetPosition()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.TransactionMetadataCache cache = new Neo4Net.kernel.impl.transaction.log.TransactionMetadataCache();
			  TransactionMetadataCache cache = new TransactionMetadataCache();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.LogPosition position = new Neo4Net.kernel.impl.transaction.log.LogPosition(3, -1);
			  LogPosition position = new LogPosition( 3, -1 );
			  const int txId = 42;
			  const int masterId = 0;
			  const int authorId = 1;
			  const int checksum = 2;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long timestamp = System.currentTimeMillis();
			  long timestamp = DateTimeHelper.CurrentUnixTimeMillis();

			  // when
			  try
			  {
					cache.CacheTransactionMetadata( txId, position, masterId, authorId, checksum, timestamp );
					fail();
			  }
			  catch ( Exception ex )
			  {
					assertEquals( "StartEntry.position is " + position, ex.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearTheCache()
		 public virtual void ShouldClearTheCache()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.TransactionMetadataCache cache = new Neo4Net.kernel.impl.transaction.log.TransactionMetadataCache();
			  TransactionMetadataCache cache = new TransactionMetadataCache();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.LogPosition position = new Neo4Net.kernel.impl.transaction.log.LogPosition(3, 4);
			  LogPosition position = new LogPosition( 3, 4 );
			  const int txId = 42;
			  const int masterId = 0;
			  const int authorId = 1;
			  const int checksum = 2;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long timestamp = System.currentTimeMillis();
			  long timestamp = DateTimeHelper.CurrentUnixTimeMillis();

			  // when
			  cache.CacheTransactionMetadata( txId, position, masterId, authorId, checksum, timestamp );
			  cache.Clear();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.TransactionMetadataCache.TransactionMetadata metadata = cache.getTransactionMetadata(txId);
			  TransactionMetadataCache.TransactionMetadata metadata = cache.GetTransactionMetadata( txId );

			  // then
			  assertNull( metadata );
		 }
	}

}