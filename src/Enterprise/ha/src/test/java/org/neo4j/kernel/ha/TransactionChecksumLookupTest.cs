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
namespace Neo4Net.Kernel.ha
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using TransactionId = Neo4Net.Kernel.impl.store.TransactionId;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using TransactionMetadataCache = Neo4Net.Kernel.impl.transaction.log.TransactionMetadataCache;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class TransactionChecksumLookupTest
	{

		 private TransactionIdStore _transactionIdStore = mock( typeof( TransactionIdStore ) );
		 private LogicalTransactionStore _transactionStore = mock( typeof( LogicalTransactionStore ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  when( _transactionIdStore.LastCommittedTransaction ).thenReturn( new TransactionId( 1, 1, 1 ) );
			  when( _transactionIdStore.UpgradeTransaction ).thenReturn( new TransactionId( 2, 2, 2 ) );
			  when( _transactionStore.getMetadataFor( 3 ) ).thenReturn( new TransactionMetadataCache.TransactionMetadata( 1, 1, mock( typeof( LogPosition ) ), 3, 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lookupChecksumUsingUpgradeTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LookupChecksumUsingUpgradeTransaction()
		 {
			  TransactionChecksumLookup checksumLookup = new TransactionChecksumLookup( _transactionIdStore, _transactionStore );
			  assertEquals( 2, checksumLookup.Lookup( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lookupChecksumUsingCommittedTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LookupChecksumUsingCommittedTransaction()
		 {
			  TransactionChecksumLookup checksumLookup = new TransactionChecksumLookup( _transactionIdStore, _transactionStore );
			  assertEquals( 1, checksumLookup.Lookup( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lookupChecksumUsingTransactionStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LookupChecksumUsingTransactionStore()
		 {
			  TransactionChecksumLookup checksumLookup = new TransactionChecksumLookup( _transactionIdStore, _transactionStore );
			  assertEquals( 3, checksumLookup.Lookup( 3 ) );
		 }
	}

}