﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.store.format
{

	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	internal class FullyCoveringRecordKeys : RecordKeys
	{
		 public static readonly RecordKeys Instance = new FullyCoveringRecordKeys();

		 public override RecordKey<NodeRecord> Node()
		 {
			  return ( written, read ) =>
			  {
				assertEquals( written.NextProp, read.NextProp );
				assertEquals( written.NextRel, read.NextRel );
				assertEquals( written.LabelField, read.LabelField );
				assertEquals( written.Dense, read.Dense );
			  };
		 }

		 public override RecordKey<RelationshipRecord> Relationship()
		 {
			  return ( written, read ) =>
			  {
				assertEquals( written.NextProp, read.NextProp );
				assertEquals( written.FirstNode, read.FirstNode );
				assertEquals( written.SecondNode, read.SecondNode );
				assertEquals( written.Type, read.Type );
				assertEquals( written.FirstPrevRel, read.FirstPrevRel );
				assertEquals( written.FirstNextRel, read.FirstNextRel );
				assertEquals( written.SecondPrevRel, read.SecondPrevRel );
				assertEquals( written.SecondNextRel, read.SecondNextRel );
				assertEquals( written.FirstInFirstChain, read.FirstInFirstChain );
				assertEquals( written.FirstInSecondChain, read.FirstInSecondChain );
			  };
		 }

		 public override RecordKey<PropertyRecord> Property()
		 {
			  return new RecordKeyAnonymousInnerClass( this );
		 }

		 private class RecordKeyAnonymousInnerClass : RecordKey<PropertyRecord>
		 {
			 private readonly FullyCoveringRecordKeys _outerInstance;

			 public RecordKeyAnonymousInnerClass( FullyCoveringRecordKeys outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public void assertRecordsEquals( PropertyRecord written, PropertyRecord read )
			 {
				  assertEquals( written.PrevProp, read.PrevProp );
				  assertEquals( written.NextProp, read.NextProp );
				  assertEquals( written.NodeSet, read.NodeSet );
				  if ( written.NodeSet )
				  {
						assertEquals( written.NodeId, read.NodeId );
				  }
				  else
				  {
						assertEquals( written.RelId, read.RelId );
				  }
				  assertEquals( written.NumberOfProperties(), read.NumberOfProperties() );
				  IEnumerator<PropertyBlock> writtenBlocks = written.GetEnumerator();
				  IEnumerator<PropertyBlock> readBlocks = read.GetEnumerator();
				  while ( writtenBlocks.MoveNext() )
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						assertTrue( readBlocks.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						assertBlocksEquals( writtenBlocks.Current, readBlocks.next() );
				  }
			 }

			 private void assertBlocksEquals( PropertyBlock written, PropertyBlock read )
			 {
				  assertEquals( written.KeyIndexId, read.KeyIndexId );
				  assertEquals( written.Size, read.Size );
				  assertTrue( written.HasSameContentsAs( read ) );
				  assertArrayEquals( written.ValueBlocks, read.ValueBlocks );
			 }
		 }

		 public override RecordKey<RelationshipGroupRecord> RelationshipGroup()
		 {
			  return ( written, read ) =>
			  {
				assertEquals( written.Type, read.Type );
				assertEquals( written.FirstOut, read.FirstOut );
				assertEquals( written.FirstIn, read.FirstIn );
				assertEquals( written.FirstLoop, read.FirstLoop );
				assertEquals( written.Next, read.Next );
				assertEquals( written.OwningNode, read.OwningNode );
			  };
		 }

		 public override RecordKey<RelationshipTypeTokenRecord> RelationshipTypeToken()
		 {
			  return ( written, read ) => assertEquals( written.NameId, read.NameId );
		 }

		 public override RecordKey<PropertyKeyTokenRecord> PropertyKeyToken()
		 {
			  return ( written, read ) =>
			  {
				assertEquals( written.NameId, read.NameId );
				assertEquals( written.PropertyCount, read.PropertyCount );
			  };
		 }

		 public override RecordKey<LabelTokenRecord> LabelToken()
		 {
			  return ( written, read ) => assertEquals( written.NameId, read.NameId );
		 }

		 public override RecordKey<DynamicRecord> Dynamic()
		 {
			  return ( written, read ) =>
			  {
				// Don't assert type, since that's read from the data, and the data in this test
				// is randomly generated. Since we assert that the data is the same then the type
				// is also correct.
				assertEquals( written.Length, read.Length );
				assertEquals( written.NextBlock, read.NextBlock );
				assertArrayEquals( written.Data, read.Data );
				assertEquals( written.StartRecord, read.StartRecord );
			  };
		 }
	}

}