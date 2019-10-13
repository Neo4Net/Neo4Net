using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.store
{
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.Graphdb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using Neo4Net.Kernel.impl.store.format;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using NodeRecordFormat = Neo4Net.Kernel.impl.store.format.standard.NodeRecordFormat;
	using PropertyKeyTokenRecordFormat = Neo4Net.Kernel.impl.store.format.standard.PropertyKeyTokenRecordFormat;
	using PropertyRecordFormat = Neo4Net.Kernel.impl.store.format.standard.PropertyRecordFormat;
	using RelationshipRecordFormat = Neo4Net.Kernel.impl.store.format.standard.RelationshipRecordFormat;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorImpl = Neo4Net.Kernel.impl.store.id.IdGeneratorImpl;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.fs.FileUtils.deleteRecursively;

	public class IdGeneratorTest
	{
		private bool InstanceFieldsInitialized = false;

		public IdGeneratorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fsRule.get() );
			RuleChain = RuleChain.outerRule( _fsRule ).around( _testDirectory );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public static readonly PageCacheRule PageCacheRule = new PageCacheRule();
		 private EphemeralFileSystemRule _fsRule = new EphemeralFileSystemRule();
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fsRule).around(testDirectory);
		 public RuleChain RuleChain;

		 private EphemeralFileSystemAbstraction _fs;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void doBefore()
		 public virtual void DoBefore()
		 {
			  _fs = _fsRule.get();
		 }

		 private void DeleteIdGeneratorFile()
		 {
			  _fs.deleteFile( IdGeneratorFile() );
		 }

		 private File IdGeneratorFile()
		 {
			  return _testDirectory.file( "testIdGenerator.id" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void cannotCreateIdGeneratorWithNullFileSystem()
		 public virtual void CannotCreateIdGeneratorWithNullFileSystem()
		 {
			  IdGeneratorImpl.createGenerator( null, IdGeneratorFile(), 0, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void cannotCreateIdGeneratorWithNullFile()
		 public virtual void CannotCreateIdGeneratorWithNullFile()
		 {
			  IdGeneratorImpl.createGenerator( _fs, null, 0, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void grabSizeCannotBeZero()
		 public virtual void GrabSizeCannotBeZero()
		 {
			  IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
			  ( new IdGeneratorImpl( _fs, IdGeneratorFile(), 0, 100, false, IdType.NODE, () => 0L ) ).Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void grabSizeCannotBeNegative()
		 public virtual void GrabSizeCannotBeNegative()
		 {
			  IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
			  ( new IdGeneratorImpl( _fs, IdGeneratorFile(), -1, 100, false, IdType.NODE, () => 0L ) ).Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void createIdGeneratorMustRefuseOverwritingExistingFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateIdGeneratorMustRefuseOverwritingExistingFile()
		 {
			  IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
			  IdGenerator idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 1008, 1000, false, IdType.NODE, () => 0L );
			  try
			  {
					IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, true );
			  }
			  finally
			  {
					CloseIdGenerator( idGenerator );
					// verify that id generator is ok
					StoreChannel fileChannel = _fs.open( IdGeneratorFile(), OpenMode.READ_WRITE );
					ByteBuffer buffer = ByteBuffer.allocate( 9 );
					fileChannel.ReadAll( buffer );
					buffer.flip();
					assertEquals( ( sbyte ) 0, buffer.get() );
					assertEquals( 0L, buffer.Long );
					buffer.flip();
					int readCount = fileChannel.read( buffer );
					if ( readCount != -1 && readCount != 0 )
					{
						 fail( "Id generator header not ok read 9 + " + readCount + " bytes from file" );
					}
					fileChannel.close();

					File file = IdGeneratorFile();
					if ( file.exists() )
					{
						 assertTrue( file.delete() );
					}
			  }
		 }

		 private void CloseIdGenerator( IdGenerator idGenerator )
		 {
			  idGenerator.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustOverwriteExistingFileIfRequested()
		 public virtual void MustOverwriteExistingFileIfRequested()
		 {
			  IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
			  IdGenerator idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 1008, 1000, false, IdType.NODE, () => 0L );
			  long[] firstFirstIds = new long[]{ idGenerator.NextId(), idGenerator.NextId(), idGenerator.NextId() };
			  idGenerator.Dispose();

			  IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
			  idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 1008, 1000, false, IdType.NODE, () => 0L );
			  long[] secondFirstIds = new long[]{ idGenerator.NextId(), idGenerator.NextId(), idGenerator.NextId() };
			  idGenerator.Dispose();

			  // Basically, recreating the id file should be the same as start over with the ids.
			  assertThat( secondFirstIds, @is( firstFirstIds ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStickyGenerator()
		 public virtual void TestStickyGenerator()
		 {
			  try
			  {
					IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
					IdGenerator idGen = new IdGeneratorImpl( _fs, IdGeneratorFile(), 3, 1000, false, IdType.NODE, () => 0L );
					try
					{
						 new IdGeneratorImpl( _fs, IdGeneratorFile(), 3, 1000, false, IdType.NODE, () => 0L );
						 fail( "Opening sticky id generator should throw exception" );
					}
					catch ( StoreFailureException )
					{ // good
					}
					CloseIdGenerator( idGen );
			  }
			  finally
			  {
					File file = IdGeneratorFile();
					if ( file.exists() )
					{
						 assertTrue( file.delete() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNextId()
		 public virtual void TestNextId()
		 {
			  try
			  {
					IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
					IdGenerator idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 3, 1000, false, IdType.NODE, () => 0L );
					for ( long i = 0; i < 7; i++ )
					{
						 assertEquals( i, idGenerator.NextId() );
					}
					idGenerator.FreeId( 1 );
					idGenerator.FreeId( 3 );
					idGenerator.FreeId( 5 );
					assertEquals( 7L, idGenerator.NextId() );
					idGenerator.FreeId( 6 );
					CloseIdGenerator( idGenerator );
					idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 5, 1000, false, IdType.NODE, () => 0L );
					idGenerator.FreeId( 2 );
					idGenerator.FreeId( 4 );
					assertEquals( 1L, idGenerator.NextId() );
					idGenerator.FreeId( 1 );
					assertEquals( 3L, idGenerator.NextId() );
					idGenerator.FreeId( 3 );
					assertEquals( 5L, idGenerator.NextId() );
					idGenerator.FreeId( 5 );
					assertEquals( 6L, idGenerator.NextId() );
					idGenerator.FreeId( 6 );
					assertEquals( 8L, idGenerator.NextId() );
					idGenerator.FreeId( 8 );
					assertEquals( 9L, idGenerator.NextId() );
					idGenerator.FreeId( 9 );
					CloseIdGenerator( idGenerator );
					idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 3, 1000, false, IdType.NODE, () => 0L );
					assertEquals( 6L, idGenerator.NextId() );
					assertEquals( 8L, idGenerator.NextId() );
					assertEquals( 9L, idGenerator.NextId() );
					assertEquals( 1L, idGenerator.NextId() );
					assertEquals( 3L, idGenerator.NextId() );
					assertEquals( 5L, idGenerator.NextId() );
					assertEquals( 2L, idGenerator.NextId() );
					assertEquals( 4L, idGenerator.NextId() );
					assertEquals( 10L, idGenerator.NextId() );
					assertEquals( 11L, idGenerator.NextId() );
					CloseIdGenerator( idGenerator );
			  }
			  finally
			  {
					File file = IdGeneratorFile();
					if ( file.exists() )
					{
						 assertTrue( file.delete() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFreeId()
		 public virtual void TestFreeId()
		 {
			  try
			  {
					IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
					IdGenerator idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 3, 1000, false, IdType.NODE, () => 0L );
					for ( long i = 0; i < 7; i++ )
					{
						 assertEquals( i, idGenerator.NextId() );
					}
					try
					{
						 idGenerator.FreeId( -1 );
						 fail( "Negative id should throw exception" );
					}
					catch ( System.ArgumentException )
					{ // good
					}
					try
					{
						 idGenerator.FreeId( 7 );
						 fail( "Greater id than ever returned should throw exception" );
					}
					catch ( System.ArgumentException )
					{ // good
					}
					for ( int i = 0; i < 7; i++ )
					{
						 idGenerator.FreeId( i );
					}
					CloseIdGenerator( idGenerator );
					idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 2, 1000, false, IdType.NODE, () => 0L );
					assertEquals( 5L, idGenerator.NextId() );
					assertEquals( 6L, idGenerator.NextId() );
					assertEquals( 3L, idGenerator.NextId() );
					CloseIdGenerator( idGenerator );
					idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 30, 1000, false, IdType.NODE, () => 0L );

					assertEquals( 0L, idGenerator.NextId() );
					assertEquals( 1L, idGenerator.NextId() );
					assertEquals( 2L, idGenerator.NextId() );
					assertEquals( 4L, idGenerator.NextId() );
					CloseIdGenerator( idGenerator );
			  }
			  finally
			  {
					File file = IdGeneratorFile();
					if ( file.exists() )
					{
						 assertTrue( file.delete() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testClose()
		 public virtual void TestClose()
		 {
			  try
			  {
					IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
					IdGenerator idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 2, 1000, false, IdType.NODE, () => 0L );
					CloseIdGenerator( idGenerator );
					try
					{
						 idGenerator.NextId();
						 fail( "nextId after close should throw exception" );
					}
					catch ( System.InvalidOperationException )
					{ // good
					}
					try
					{
						 idGenerator.FreeId( 0 );
						 fail( "freeId after close should throw exception" );
					}
					catch ( System.InvalidOperationException )
					{ // good
					}
					idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 2, 1000, false, IdType.NODE, () => 0L );
					assertEquals( 0L, idGenerator.NextId() );
					assertEquals( 1L, idGenerator.NextId() );
					assertEquals( 2L, idGenerator.NextId() );
					CloseIdGenerator( idGenerator );
					try
					{
						 idGenerator.NextId();
						 fail( "nextId after close should throw exception" );
					}
					catch ( System.InvalidOperationException )
					{ // good
					}
					try
					{
						 idGenerator.FreeId( 0 );
						 fail( "freeId after close should throw exception" );
					}
					catch ( System.InvalidOperationException )
					{ // good
					}
			  }
			  finally
			  {
					File file = IdGeneratorFile();
					if ( file.exists() )
					{
						 assertTrue( file.delete() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testOddAndEvenWorstCase()
		 public virtual void TestOddAndEvenWorstCase()
		 {
			  int capacity = 1024 * 8 + 1;
			  try
			  {
					IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
					IdGenerator idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 128, capacity * 2, false, IdType.NODE, () => 0L );
					for ( int i = 0; i < capacity; i++ )
					{
						 idGenerator.NextId();
					}
					IDictionary<long, object> freedIds = new Dictionary<long, object>();
					for ( long i = 1; i < capacity; i += 2 )
					{
						 idGenerator.FreeId( i );
						 freedIds[i] = this;
					}
					CloseIdGenerator( idGenerator );
					idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 2000, capacity * 2, false, IdType.NODE, () => 0L );
					long oldId = -1;
					for ( int i = 0; i < capacity - 1; i += 2 )
					{
						 long id = idGenerator.NextId();
						 if ( freedIds.Remove( id ) == null )
						 {
							  throw new Exception( "Id=" + id + " prevId=" + oldId + " list.size()=" + freedIds.Count );
						 }
						 oldId = id;
					}
					assertEquals( 0, freedIds.Values.Count );
					CloseIdGenerator( idGenerator );
			  }
			  finally
			  {
					File file = IdGeneratorFile();
					if ( _fs.fileExists( file ) )
					{
						 assertTrue( _fs.deleteFile( file ) );
					}
			  }
			  try
			  {
					IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
					IdGenerator idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 128, capacity * 2, false, IdType.NODE, () => 0L );
					for ( int i = 0; i < capacity; i++ )
					{
						 idGenerator.NextId();
					}
					IDictionary<long, object> freedIds = new Dictionary<long, object>();
					for ( long i = 0; i < capacity; i += 2 )
					{
						 idGenerator.FreeId( i );
						 freedIds[i] = this;
					}
					CloseIdGenerator( idGenerator );
					idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 2000, capacity * 2, false, IdType.NODE, () => 0L );
					for ( int i = 0; i < capacity; i += 2 )
					{
						 assertEquals( this, freedIds.Remove( idGenerator.NextId() ) );
					}
					assertEquals( 0, freedIds.Values.Count );
					CloseIdGenerator( idGenerator );
			  }
			  finally
			  {
					File file = IdGeneratorFile();
					if ( file.exists() )
					{
						 assertTrue( file.delete() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRandomTest()
		 public virtual void TestRandomTest()
		 {
			  Random random = new Random( DateTimeHelper.CurrentUnixTimeMillis() );
			  int capacity = random.Next( 1024 ) + 1024;
			  int grabSize = random.Next( 128 ) + 128;
			  IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
			  IdGenerator idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), grabSize, capacity * 2, false, IdType.NODE, () => 0L );
			  IList<long> idsTaken = new List<long>();
			  float releaseIndex = 0.25f;
			  float closeIndex = 0.05f;
			  int currentIdCount = 0;
			  try
			  {
					while ( currentIdCount < capacity )
					{
						 float rIndex = random.nextFloat();
						 if ( rIndex < releaseIndex && currentIdCount > 0 )
						 {
							  idGenerator.FreeId( idsTaken.Remove( random.Next( currentIdCount ) ).intValue() );
							  currentIdCount--;
						 }
						 else
						 {
							  idsTaken.Add( idGenerator.NextId() );
							  currentIdCount++;
						 }
						 if ( rIndex > ( 1.0f - closeIndex ) || rIndex < closeIndex )
						 {
							  CloseIdGenerator( idGenerator );
							  grabSize = random.Next( 128 ) + 128;
							  idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), grabSize, capacity * 2, false, IdType.NODE, () => 0L );
						 }
					}
					CloseIdGenerator( idGenerator );
			  }
			  finally
			  {
					File file = IdGeneratorFile();
					if ( file.exists() )
					{
						 assertTrue( file.delete() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testUnsignedId()
		 public virtual void TestUnsignedId()
		 {
			  try
			  {
					PropertyKeyTokenRecordFormat recordFormat = new PropertyKeyTokenRecordFormat();
					IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
					IdGenerator idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 1, recordFormat.MaxId, false, IdType.NODE, () => 0L );
					idGenerator.HighId = recordFormat.MaxId;
					long id = idGenerator.NextId();
					assertEquals( recordFormat.MaxId, id );
					idGenerator.FreeId( id );
					try
					{
						 idGenerator.NextId();
						 fail( "Shouldn't be able to get next ID" );
					}
					catch ( StoreFailureException )
					{ // good, capacity exceeded
					}
					CloseIdGenerator( idGenerator );
					idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 1, recordFormat.MaxId, false, IdType.NODE, () => 0L );
					assertEquals( recordFormat.MaxId + 1, idGenerator.HighId );
					id = idGenerator.NextId();
					assertEquals( recordFormat.MaxId, id );
					try
					{
						 idGenerator.NextId();
					}
					catch ( StoreFailureException )
					{ // good, capacity exceeded
					}
					CloseIdGenerator( idGenerator );
			  }
			  finally
			  {
					File file = IdGeneratorFile();
					if ( file.exists() )
					{
						 assertTrue( file.delete() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureIdCapacityCannotBeExceeded()
		 public virtual void MakeSureIdCapacityCannotBeExceeded()
		 {
			  RecordFormats formats = Standard.LATEST_RECORD_FORMATS;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.neo4j.kernel.impl.store.format.RecordFormat<? extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord>> recordFormats = java.util.Arrays.asList(formats.node(), formats.dynamic(), formats.labelToken(), formats.property(), formats.propertyKeyToken(), formats.relationship(), formats.relationshipGroup(), formats.relationshipTypeToken());
			  IList<RecordFormat<AbstractBaseRecord>> recordFormats = Arrays.asList( formats.Node(), formats.Dynamic(), formats.LabelToken(), formats.Property(), formats.PropertyKeyToken(), formats.Relationship(), formats.RelationshipGroup(), formats.RelationshipTypeToken() );

			  foreach ( RecordFormat format in recordFormats )
			  {
					MakeSureIdCapacityCannotBeExceeded( format );
			  }
		 }

		 private void MakeSureIdCapacityCannotBeExceeded( RecordFormat format )
		 {
			  DeleteIdGeneratorFile();
			  IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
			  long maxValue = format.MaxId;
			  IdGenerator idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 1, maxValue - 1, false, IdType.NODE, () => 0L );
			  long id = maxValue - 2;
			  idGenerator.HighId = id;
			  assertEquals( id, idGenerator.NextId() );
			  assertEquals( id + 1, idGenerator.NextId() );
			  try
			  {
					idGenerator.NextId();
					fail( "Id capacity shouldn't be able to be exceeded for " + format );
			  }
			  catch ( StoreFailureException )
			  { // Good
			  }
			  CloseIdGenerator( idGenerator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureMagicMinusOneIsNotReturnedFromNodeIdGenerator()
		 public virtual void MakeSureMagicMinusOneIsNotReturnedFromNodeIdGenerator()
		 {
			  MakeSureMagicMinusOneIsSkipped( new NodeRecordFormat() );
			  MakeSureMagicMinusOneIsSkipped( new RelationshipRecordFormat() );
			  MakeSureMagicMinusOneIsSkipped( new PropertyRecordFormat() );
		 }

		 private void MakeSureMagicMinusOneIsSkipped( RecordFormat format )
		 {
			  DeleteIdGeneratorFile();
			  IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
			  IdGenerator idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 1, format.MaxId, false, IdType.NODE, () => 0L );
			  long id = ( long ) Math.Pow( 2, 32 ) - 3;
			  idGenerator.HighId = id;
			  assertEquals( id, idGenerator.NextId() );
			  assertEquals( id + 1, idGenerator.NextId() );
			  // Here we make sure that id+2 (integer -1) is skipped
			  assertEquals( id + 3, idGenerator.NextId() );
			  assertEquals( id + 4, idGenerator.NextId() );
			  assertEquals( id + 5, idGenerator.NextId() );
			  CloseIdGenerator( idGenerator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureMagicMinusOneCannotBeReturnedEvenIfFreed()
		 public virtual void MakeSureMagicMinusOneCannotBeReturnedEvenIfFreed()
		 {
			  IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
			  IdGenerator idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 1, (new NodeRecordFormat()).MaxId, false, IdType.NODE, () => 0L );
			  long magicMinusOne = ( long ) Math.Pow( 2, 32 ) - 1;
			  idGenerator.HighId = magicMinusOne;
			  assertEquals( magicMinusOne + 1, idGenerator.NextId() );
			  idGenerator.FreeId( magicMinusOne - 1 );
			  idGenerator.FreeId( magicMinusOne );
			  CloseIdGenerator( idGenerator );

			  idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 1, (new NodeRecordFormat()).MaxId, false, IdType.NODE, () => 0L );
			  assertEquals( magicMinusOne - 1, idGenerator.NextId() );
			  assertEquals( magicMinusOne + 2, idGenerator.NextId() );
			  CloseIdGenerator( idGenerator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void commandsGetWrittenOnceSoThatFreedIdsGetsAddedOnlyOnce() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CommandsGetWrittenOnceSoThatFreedIdsGetsAddedOnlyOnce()
		 {
			  File storeDir = new File( "target/var/free-id-once" );
			  deleteRecursively( storeDir );
			  GraphDatabaseService db = CreateTestDatabase( storeDir );
			  RelationshipType type = withName( "SOME_TYPE" );

			  // This transaction will, if some commands may be executed more than
			  // once,
			  // add the freed ids to the defrag list more than once - making the id
			  // generator
			  // return the same id more than once during the next session.
			  ISet<long> createdNodeIds = new HashSet<long>();
			  ISet<long> createdRelationshipIds = new HashSet<long>();
			  Transaction tx = Db.beginTx();
			  Node commonNode = Db.createNode();
			  for ( int i = 0; i < 20; i++ )
			  {
					Node otherNode = Db.createNode();
					Relationship relationship = commonNode.CreateRelationshipTo( otherNode, type );
					if ( i % 5 == 0 )
					{
						 otherNode.Delete();
						 relationship.Delete();
					}
					else
					{
						 createdNodeIds.Add( otherNode.Id );
						 createdRelationshipIds.Add( relationship.Id );
					}
			  }
			  tx.Success();
			  tx.Close();
			  Db.shutdown();

			  // After a clean shutdown, create new nodes and relationships and see so
			  // that
			  // all ids are unique.
			  db = CreateTestDatabase( storeDir );
			  tx = Db.beginTx();
			  commonNode = Db.getNodeById( commonNode.Id );
			  for ( int i = 0; i < 100; i++ )
			  {
					Node otherNode = Db.createNode();
					if ( !createdNodeIds.Add( otherNode.Id ) )
					{
						 fail( "Managed to create a node with an id that was already in use" );
					}
					Relationship relationship = commonNode.CreateRelationshipTo( otherNode, type );
					if ( !createdRelationshipIds.Add( relationship.Id ) )
					{
						 fail( "Managed to create a relationship with an id that was already in use" );
					}
			  }
			  tx.Success();
			  tx.Close();

			  // Verify by loading everything from scratch
			  tx = Db.beginTx();
			  foreach ( Node node in Db.AllNodes )
			  {
					Iterables.lastOrNull( node.Relationships );
			  }
			  tx.Close();
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void delete()
		 public virtual void Delete()
		 {
			  IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
			  IdGeneratorImpl idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 10, 1000, false, IdType.NODE, () => 0L );
			  long id = idGenerator.NextId();
			  idGenerator.NextId();
			  idGenerator.FreeId( id );
			  idGenerator.Dispose();
			  idGenerator.Delete();
			  assertFalse( IdGeneratorFile().exists() );
			  IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
			  idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), 10, 1000, false, IdType.NODE, () => 0L );
			  assertEquals( id, idGenerator.NextId() );
			  idGenerator.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChurnIdBatchAtGrabSize()
		 public virtual void TestChurnIdBatchAtGrabSize()
		 {
			  IdGenerator idGenerator = null;
			  try
			  {
					IdGeneratorImpl.createGenerator( _fs, IdGeneratorFile(), 0, false );
					const int grabSize = 10;
					int rounds = 10;
					idGenerator = new IdGeneratorImpl( _fs, IdGeneratorFile(), grabSize, 1000, true, IdType.NODE, () => 0L );

					for ( int i = 0; i < rounds; i++ )
					{
						 ISet<long> ids = new HashSet<long>();
						 for ( int j = 0; j < grabSize; j++ )
						 {
							  ids.Add( idGenerator.NextId() );
						 }
						 foreach ( long? id in ids )
						 {
							  idGenerator.FreeId( id.Value );
						 }
					}
					long newId = idGenerator.NextId();
					assertTrue( "Expected IDs to be reused (" + grabSize + " at a time). high ID was: " + newId, newId < grabSize * rounds );
			  }
			  finally
			  {
					if ( idGenerator != null )
					{
						 CloseIdGenerator( idGenerator );
					}
					File file = IdGeneratorFile();
					if ( file.exists() )
					{
						 assertTrue( file.delete() );
					}
			  }
		 }

		 private GraphDatabaseService CreateTestDatabase( File storeDir )
		 {
			  return ( new TestGraphDatabaseFactory() ).setFileSystem(new UncloseableDelegatingFileSystemAbstraction(_fs)).newImpermanentDatabase(storeDir);
		 }
	}

}