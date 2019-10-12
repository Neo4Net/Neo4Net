using System.Collections;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.impl.index.labelscan
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using LabelScanWriter = Org.Neo4j.Kernel.api.labelscan.LabelScanWriter;
	using LifeRule = Org.Neo4j.Kernel.Lifecycle.LifeRule;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LabelScanReader = Org.Neo4j.Storageengine.Api.schema.LabelScanReader;
	using PageCacheAndDependenciesRule = Org.Neo4j.Test.rule.PageCacheAndDependenciesRule;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.RecoveryCleanupWorkCollector.immediate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.labelscan.NodeLabelUpdate.labelChanges;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.scan.FullStoreChangeStream_Fields.EMPTY;

	public class NativeLabelScanReaderIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheAndDependenciesRule storage = new org.neo4j.test.rule.PageCacheAndDependenciesRule();
		 public readonly PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.kernel.lifecycle.LifeRule life = new org.neo4j.kernel.lifecycle.LifeRule(true);
		 public readonly LifeRule Life = new LifeRule( true );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartFromGivenIdDense() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStartFromGivenIdDense()
		 {
			  ShouldStartFromGivenId( 10 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartFromGivenIdSparse() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStartFromGivenIdSparse()
		 {
			  ShouldStartFromGivenId( 100 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartFromGivenIdSuperSparse() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStartFromGivenIdSuperSparse()
		 {
			  ShouldStartFromGivenId( 1000 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldStartFromGivenId(int sparsity) throws java.io.IOException
		 private void ShouldStartFromGivenId( int sparsity )
		 {
			  // given
			  NativeLabelScanStore store = Life.add( new NativeLabelScanStore( Storage.pageCache(), DatabaseLayout.of(Storage.directory().directory()), Storage.fileSystem(), EMPTY, false, new Monitors(), immediate() ) );
			  int labelId = 1;
			  int highNodeId = 100_000;
			  BitArray expected = new BitArray( highNodeId );
			  using ( LabelScanWriter writer = store.NewWriter() )
			  {
					int updates = highNodeId / sparsity;
					for ( int i = 0; i < updates; i++ )
					{
						 int nodeId = Random.Next( highNodeId );
						 writer.Write( labelChanges( nodeId, EMPTY_LONG_ARRAY, new long[]{ labelId } ) );
						 expected.Set( nodeId, true );
					}
			  }

			  // when
			  long fromId = Random.Next( highNodeId );
			  int nextExpectedId = expected.nextSetBit( toIntExact( fromId + 1 ) );
			  using ( LabelScanReader reader = store.NewReader(), PrimitiveLongResourceIterator ids = reader.NodesWithAnyOfLabels(fromId, new int[] { labelId }) )
			  {
					// then
					while ( nextExpectedId != -1 )
					{
						 assertTrue( ids.hasNext() );
						 long nextId = ids.next();
						 assertEquals( nextExpectedId, toIntExact( nextId ) );
						 nextExpectedId = expected.nextSetBit( nextExpectedId + 1 );
					}
					assertFalse( ids.hasNext() );
			  }
		 }
	}

}