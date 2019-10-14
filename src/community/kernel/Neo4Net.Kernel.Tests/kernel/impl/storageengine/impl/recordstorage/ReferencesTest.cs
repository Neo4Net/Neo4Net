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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using Test = org.junit.Test;

	using RelationshipReferenceEncoding = Neo4Net.Kernel.Impl.Newapi.RelationshipReferenceEncoding;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.References.clearEncoding;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.RelationshipReferenceEncoding.FILTER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.RelationshipReferenceEncoding.FILTER_TX_STATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.RelationshipReferenceEncoding.GROUP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.RelationshipReferenceEncoding.NO_INCOMING_OF_TYPE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.RelationshipReferenceEncoding.NO_LOOP_OF_TYPE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.RelationshipReferenceEncoding.NO_OUTGOING_OF_TYPE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.RelationshipReferenceEncoding.parseEncoding;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.AbstractBaseRecord.NO_ID;

	public class ReferencesTest
	{
		 // This value the largest possible high limit id +1 (see HighLimitV3_1_0)
		 private static long _maxIdLimit = 1L << 50;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPreserveNoId()
		 public virtual void ShouldPreserveNoId()
		 {
			  assertThat( RelationshipReferenceEncoding.encodeForFiltering( NO_ID ), equalTo( ( long ) NO_ID ) );
			  assertThat( RelationshipReferenceEncoding.encodeForTxStateFiltering( NO_ID ), equalTo( ( long ) NO_ID ) );
			  assertThat( RelationshipReferenceEncoding.encodeGroup( NO_ID ), equalTo( ( long ) NO_ID ) );
			  assertThat( RelationshipReferenceEncoding.encodeNoIncomingRels( NO_ID ), equalTo( ( long ) NO_ID ) );
			  assertThat( RelationshipReferenceEncoding.encodeNoOutgoingRels( NO_ID ), equalTo( ( long ) NO_ID ) );
			  assertThat( RelationshipReferenceEncoding.encodeNoLoopRels( NO_ID ), equalTo( ( long ) NO_ID ) );

			  assertThat( GroupReferenceEncoding.EncodeRelationship( NO_ID ), equalTo( ( long ) NO_ID ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearFlags()
		 public virtual void ShouldClearFlags()
		 {
			  ThreadLocalRandom random = ThreadLocalRandom.current();
			  for ( int i = 0; i < 1000; i++ )
			  {
					long reference = random.nextLong( _maxIdLimit );
					int token = random.Next( int.MaxValue );

					assertThat( clearEncoding( RelationshipReferenceEncoding.encodeGroup( reference ) ), equalTo( reference ) );
					assertThat( clearEncoding( RelationshipReferenceEncoding.encodeForFiltering( reference ) ), equalTo( reference ) );
					assertThat( clearEncoding( RelationshipReferenceEncoding.encodeForTxStateFiltering( reference ) ), equalTo( reference ) );
					assertThat( clearEncoding( RelationshipReferenceEncoding.encodeNoIncomingRels( token ) ), equalTo( ( long ) token ) );
					assertThat( clearEncoding( RelationshipReferenceEncoding.encodeNoOutgoingRels( token ) ), equalTo( ( long ) token ) );
					assertThat( clearEncoding( RelationshipReferenceEncoding.encodeNoLoopRels( token ) ), equalTo( ( long ) token ) );

					assertThat( clearEncoding( GroupReferenceEncoding.EncodeRelationship( reference ) ), equalTo( reference ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void encodeForFiltering()
		 public virtual void EncodeForFiltering()
		 {
			  ThreadLocalRandom random = ThreadLocalRandom.current();
			  for ( int i = 0; i < 1000; i++ )
			  {
					long reference = random.nextLong( _maxIdLimit );
					assertNotEquals( FILTER, parseEncoding( reference ) );
					assertEquals( FILTER, parseEncoding( RelationshipReferenceEncoding.encodeForFiltering( reference ) ) );
					assertTrue( "encoded reference is negative", RelationshipReferenceEncoding.encodeForFiltering( reference ) < 0 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void encodeForTxStateFiltering()
		 public virtual void EncodeForTxStateFiltering()
		 {
			  ThreadLocalRandom random = ThreadLocalRandom.current();
			  for ( int i = 0; i < 1000; i++ )
			  {
					long reference = random.nextLong( _maxIdLimit );
					assertNotEquals( FILTER_TX_STATE, parseEncoding( reference ) );
					assertEquals( FILTER_TX_STATE, parseEncoding( RelationshipReferenceEncoding.encodeForTxStateFiltering( reference ) ) );
					assertTrue( "encoded reference is negative", RelationshipReferenceEncoding.encodeForTxStateFiltering( reference ) < 0 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void encodeFromGroup()
		 public virtual void EncodeFromGroup()
		 {
			  ThreadLocalRandom random = ThreadLocalRandom.current();
			  for ( int i = 0; i < 1000; i++ )
			  {
					long reference = random.nextLong( _maxIdLimit );
					assertNotEquals( GROUP, parseEncoding( reference ) );
					assertEquals( GROUP, parseEncoding( RelationshipReferenceEncoding.encodeGroup( reference ) ) );
					assertTrue( "encoded reference is negative", RelationshipReferenceEncoding.encodeGroup( reference ) < 0 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void encodeNoIncomingRels()
		 public virtual void EncodeNoIncomingRels()
		 {
			  ThreadLocalRandom random = ThreadLocalRandom.current();
			  for ( int i = 0; i < 1000; i++ )
			  {
					int token = random.Next( int.MaxValue );
					assertNotEquals( NO_INCOMING_OF_TYPE, parseEncoding( token ) );
					assertEquals( NO_INCOMING_OF_TYPE, parseEncoding( RelationshipReferenceEncoding.encodeNoIncomingRels( token ) ) );
					assertTrue( "encoded reference is negative", RelationshipReferenceEncoding.encodeNoIncomingRels( token ) < 0 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void encodeNoOutgoingRels()
		 public virtual void EncodeNoOutgoingRels()
		 {
			  ThreadLocalRandom random = ThreadLocalRandom.current();
			  for ( int i = 0; i < 1000; i++ )
			  {
					int token = random.Next( int.MaxValue );
					assertNotEquals( NO_OUTGOING_OF_TYPE, parseEncoding( token ) );
					assertEquals( NO_OUTGOING_OF_TYPE, parseEncoding( RelationshipReferenceEncoding.encodeNoOutgoingRels( token ) ) );
					assertTrue( "encoded reference is negative", RelationshipReferenceEncoding.encodeNoOutgoingRels( token ) < 0 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void encodeNoLoopRels()
		 public virtual void EncodeNoLoopRels()
		 {
			  ThreadLocalRandom random = ThreadLocalRandom.current();
			  for ( int i = 0; i < 1000; i++ )
			  {
					int token = random.Next( int.MaxValue );
					assertNotEquals( NO_LOOP_OF_TYPE, parseEncoding( token ) );
					assertEquals( NO_LOOP_OF_TYPE, parseEncoding( RelationshipReferenceEncoding.encodeNoLoopRels( token ) ) );
					assertTrue( "encoded reference is negative", RelationshipReferenceEncoding.encodeNoLoopRels( token ) < 0 );
			  }
		 }
	}

}