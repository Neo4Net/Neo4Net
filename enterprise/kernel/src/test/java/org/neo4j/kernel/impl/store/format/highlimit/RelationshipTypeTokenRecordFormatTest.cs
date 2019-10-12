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
namespace Org.Neo4j.Kernel.impl.store.format.highlimit
{
	using Test = org.junit.Test;

	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using StubPageCursor = Org.Neo4j.Io.pagecache.StubPageCursor;
	using Org.Neo4j.Kernel.impl.store.format;
	using IdSequence = Org.Neo4j.Kernel.impl.store.id.IdSequence;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.kibiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NoStoreHeader.NO_STORE_HEADER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;

	public class RelationshipTypeTokenRecordFormatTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleRelationshipTypesBeyond2Bytes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleRelationshipTypesBeyond2Bytes()
		 {
			  // given
			  RecordFormat<RelationshipTypeTokenRecord> format = HighLimit.RecordFormats.relationshipTypeToken();
			  int typeId = 1 << ( ( sizeof( short ) * 8 ) + ( sizeof( sbyte ) * 8 ) ) - 1;
			  RelationshipTypeTokenRecord record = new RelationshipTypeTokenRecord( typeId );
			  int recordSize = format.GetRecordSize( NO_STORE_HEADER );
			  record.Initialize( true, 10 );
			  IdSequence doubleUnits = mock( typeof( IdSequence ) );
			  PageCursor cursor = new StubPageCursor( 0, ( int ) kibiBytes( 8 ) );

			  // when
			  format.Prepare( record, recordSize, doubleUnits );
			  format.Write( record, cursor, recordSize );
			  verifyNoMoreInteractions( doubleUnits );

			  // then
			  RelationshipTypeTokenRecord read = new RelationshipTypeTokenRecord( typeId );
			  format.Read( record, cursor, NORMAL, recordSize );
			  assertEquals( record, read );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReport3BytesMaxIdForRelationshipTypes()
		 public virtual void ShouldReport3BytesMaxIdForRelationshipTypes()
		 {
			  // given
			  RecordFormat<RelationshipTypeTokenRecord> format = HighLimit.RecordFormats.relationshipTypeToken();

			  // when
			  long maxId = format.MaxId;

			  // then
			  assertEquals( ( 1 << HighLimitFormatSettings.RelationshipTypeTokenMaximumIdBits ) - 1, maxId );
		 }
	}

}