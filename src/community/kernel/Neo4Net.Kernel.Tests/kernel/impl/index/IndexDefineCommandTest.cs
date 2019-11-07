﻿/*
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
namespace Neo4Net.Kernel.impl.index
{
	using MutableObjectIntMap = org.eclipse.collections.api.map.primitive.MutableObjectIntMap;
	using ObjectIntHashMap = org.eclipse.collections.impl.map.mutable.primitive.ObjectIntHashMap;
	using Test = org.junit.Test;

	using RecordStorageCommandReaderFactory = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageCommandReaderFactory;
	using InMemoryClosableChannel = Neo4Net.Kernel.impl.transaction.log.InMemoryClosableChannel;
	using LogEntryVersion = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryVersion;
	using CommandReader = Neo4Net.Kernel.Api.StorageEngine.CommandReader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class IndexDefineCommandTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIndexCommandCreationEnforcesLimit()
		 public virtual void TestIndexCommandCreationEnforcesLimit()
		 {
			  // Given
			  IndexDefineCommand idc = new IndexDefineCommand();
			  int count = IndexDefineCommand.HighestPossibleId;

			  // When
			  for ( int i = 0; i < count; i++ )
			  {
					idc.GetOrAssignKeyId( "key" + i );
					idc.GetOrAssignIndexNameId( "index" + i );
			  }

			  // Then
			  // it should break on too many
			  try
			  {
					idc.GetOrAssignKeyId( "dropThatOverflows" );
					fail( "IndexDefineCommand should not allow more than " + count + " indexes per transaction" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// wonderful
			  }

			  try
			  {
					idc.GetOrAssignIndexNameId( "dropThatOverflows" );
					fail( "IndexDefineCommand should not allow more than " + count + " keys per transaction" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// wonderful
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteIndexDefineCommandIfMapWithinShortRange() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteIndexDefineCommandIfMapWithinShortRange()
		 {
			  // GIVEN
			  InMemoryClosableChannel channel = new InMemoryClosableChannel( 10_000 );
			  IndexDefineCommand command = InitIndexDefineCommand( 300 );

			  // WHEN
			  command.Serialize( channel );

			  // THEN
			  CommandReader commandReader = ( new RecordStorageCommandReaderFactory() ).byVersion(LogEntryVersion.CURRENT.byteCode());
			  IndexDefineCommand read = ( IndexDefineCommand ) commandReader.Read( channel );
			  assertEquals( command.IndexNameIdRange, read.IndexNameIdRange );
			  assertEquals( command.KeyIdRange, read.KeyIdRange );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToWriteIndexDefineCommandIfMapIsLargerThanShort() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToWriteIndexDefineCommandIfMapIsLargerThanShort()
		 {
			  // GIVEN
			  InMemoryClosableChannel channel = new InMemoryClosableChannel( 1000 );
			  IndexDefineCommand command = new IndexDefineCommand();
			  MutableObjectIntMap<string> largeMap = InitMap( 0xFFFF + 1 );
			  command.Init( largeMap, largeMap );

			  // WHEN
			  assertTrue( Serialize( channel, command ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean serialize(Neo4Net.kernel.impl.transaction.log.InMemoryClosableChannel channel, IndexDefineCommand command) throws java.io.IOException
		 private bool Serialize( InMemoryClosableChannel channel, IndexDefineCommand command )
		 {
			  try
			  {
					command.Serialize( channel );
			  }
			  catch ( AssertionError )
			  {
					return true;
			  }
			  return false;
		 }

		 private IndexDefineCommand InitIndexDefineCommand( int nbrOfEntries )
		 {
			  IndexDefineCommand command = new IndexDefineCommand();
			  MutableObjectIntMap<string> indexNames = InitMap( nbrOfEntries );
			  MutableObjectIntMap<string> keys = InitMap( nbrOfEntries );
			  command.Init( indexNames, keys );
			  return command;
		 }

		 private MutableObjectIntMap<string> InitMap( int nbrOfEntries )
		 {
			  MutableObjectIntMap<string> toReturn = new ObjectIntHashMap<string>();
			  while ( nbrOfEntries-- > 0 )
			  {
					toReturn.put( "key" + nbrOfEntries, nbrOfEntries );
			  }
			  return toReturn;
		 }
	}

}