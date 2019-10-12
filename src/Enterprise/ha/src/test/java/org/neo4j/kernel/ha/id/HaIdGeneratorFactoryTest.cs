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
namespace Neo4Net.Kernel.ha.id
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using ComException = Neo4Net.com.ComException;
	using Neo4Net.com;
	using TransientTransactionFailureException = Neo4Net.Graphdb.TransientTransactionFailureException;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using Neo4Net.Kernel.ha;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorImpl = Neo4Net.Kernel.impl.store.id.IdGeneratorImpl;
	using IdRange = Neo4Net.Kernel.impl.store.id.IdRange;
	using IdRangeIterator = Neo4Net.Kernel.impl.store.id.IdRangeIterator;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using CommunityIdTypeConfigurationProvider = Neo4Net.Kernel.impl.store.id.configuration.CommunityIdTypeConfigurationProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.isNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.id.IdRangeIterator.VALUE_REPRESENTING_NULL;


	public class HaIdGeneratorFactoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule FileSystemRule = new EphemeralFileSystemRule();
		 private Master _master;
		 private DelegateInvocationHandler<Master> _masterDelegate;
		 private EphemeralFileSystemAbstraction _fs;
		 private HaIdGeneratorFactory _fac;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _master = mock( typeof( Master ) );
			  _masterDelegate = new DelegateInvocationHandler<Master>( typeof( Master ) );
			  _fs = FileSystemRule.get();
			  _fac = new HaIdGeneratorFactory( _masterDelegate, NullLogProvider.Instance, mock( typeof( RequestContextFactory ) ), _fs, new CommunityIdTypeConfigurationProvider() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void slaveIdGeneratorShouldReturnFromAssignedRange()
		 public virtual void SlaveIdGeneratorShouldReturnFromAssignedRange()
		 {
			  // GIVEN
			  IdAllocation firstResult = new IdAllocation( new IdRange( new long[]{}, 42, 123 ), 123, 0 );
			  Response<IdAllocation> response = response( firstResult );
			  when( _master.allocateIds( Null, any( typeof( IdType ) ) ) ).thenReturn( response );

			  // WHEN
			  IdGenerator gen = SwitchToSlave();

			  // THEN
			  for ( long i = firstResult.IdRange.RangeStart; i < firstResult.IdRange.RangeLength; i++ )
			  {
					assertEquals( i, gen.NextId() );
			  }
			  verify( _master, times( 1 ) ).allocateIds( Null, eq( IdType.NODE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void slaveIdGeneratorShouldAskForMoreWhenRangeIsOver()
		 public virtual void SlaveIdGeneratorShouldAskForMoreWhenRangeIsOver()
		 {
			  // GIVEN
			  IdAllocation firstResult = new IdAllocation( new IdRange( new long[]{}, 42, 123 ), 42 + 123, 0 );
			  IdAllocation secondResult = new IdAllocation( new IdRange( new long[]{}, 1042, 223 ), 1042 + 223, 0 );
			  Response<IdAllocation> response = response( firstResult, secondResult );
			  when( _master.allocateIds( Null, any( typeof( IdType ) ) ) ).thenReturn( response );

			  // WHEN
			  IdGenerator gen = SwitchToSlave();

			  // THEN
			  long startAt = firstResult.IdRange.RangeStart;
			  long forThatMany = firstResult.IdRange.RangeLength;
			  for ( long i = startAt; i < startAt + forThatMany; i++ )
			  {
					assertEquals( i, gen.NextId() );
			  }
			  verify( _master, times( 1 ) ).allocateIds( Null, eq( IdType.NODE ) );

			  startAt = secondResult.IdRange.RangeStart;
			  forThatMany = secondResult.IdRange.RangeLength;
			  for ( long i = startAt; i < startAt + forThatMany; i++ )
			  {
					assertEquals( i, gen.NextId() );
			  }

			  verify( _master, times( 2 ) ).allocateIds( Null, eq( IdType.NODE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseDefraggedIfPresent()
		 public virtual void ShouldUseDefraggedIfPresent()
		 {
			  // GIVEN
			  long[] defragIds = new long[] { 42, 27172828, 314159 };
			  IdAllocation firstResult = new IdAllocation( new IdRange( defragIds, 0, 0 ), 0, defragIds.Length );
			  Response<IdAllocation> response = response( firstResult );
			  when( _master.allocateIds( Null, any( typeof( IdType ) ) ) ).thenReturn( response );

			  // WHEN
			  IdGenerator gen = SwitchToSlave();

			  // THEN
			  foreach ( long defragId in defragIds )
			  {
					assertEquals( defragId, gen.NextId() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMoveFromDefraggedToRange()
		 public virtual void ShouldMoveFromDefraggedToRange()
		 {
			  // GIVEN
			  long[] defragIds = new long[] { 42, 27172828, 314159 };
			  IdAllocation firstResult = new IdAllocation( new IdRange( defragIds, 0, 10 ), 100, defragIds.Length );
			  Response<IdAllocation> response = response( firstResult );
			  when( _master.allocateIds( Null, any( typeof( IdType ) ) ) ).thenReturn( response );

			  // WHEN
			  IdGenerator gen = SwitchToSlave();

			  // THEN
			  foreach ( long defragId in defragIds )
			  {
					assertEquals( defragId, gen.NextId() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void slaveShouldNeverAllowReducingHighId()
		 public virtual void SlaveShouldNeverAllowReducingHighId()
		 {
			  // GIVEN
			  const int highIdFromAllocation = 123;
			  IdAllocation firstResult = new IdAllocation( new IdRange( new long[] {}, 42, highIdFromAllocation ), highIdFromAllocation, 0 );
			  Response<IdAllocation> response = response( firstResult );
			  when( _master.allocateIds( Null, any( typeof( IdType ) ) ) ).thenReturn( response );

			  // WHEN
			  IdGenerator gen = SwitchToSlave();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int highIdFromUpdatedRecord = highIdFromAllocation + 1;
			  int highIdFromUpdatedRecord = highIdFromAllocation + 1;
			  gen.HighId = highIdFromUpdatedRecord; // Assume this is from a received transaction
			  gen.NextId(); // that will ask the master for an IdRange

			  // THEN
			  assertEquals( highIdFromUpdatedRecord, gen.HighId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteIdGeneratorsAsPartOfSwitchingToSlave()
		 public virtual void ShouldDeleteIdGeneratorsAsPartOfSwitchingToSlave()
		 {
			  // GIVEN we're in master mode. We do that to allow HaIdGeneratorFactory to open id generators at all
			  _fac.switchToMaster();
			  File idFile = new File( "my.id" );
			  // ... opening an id generator as master
			  _fac.create( idFile, 10, true );
			  IdGenerator idGenerator = _fac.open( idFile, 10, IdType.NODE, () => 10L, Standard.LATEST_RECORD_FORMATS.node().MaxId );
			  assertTrue( _fs.fileExists( idFile ) );
			  idGenerator.Dispose();

			  // WHEN switching to slave
			  _fac.switchToSlave();

			  // THEN the .id file underneath should be deleted
			  assertFalse( "Id file should've been deleted by now", _fs.fileExists( idFile ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteIdGeneratorsAsPartOfOpenAfterSwitchingToSlave()
		 public virtual void ShouldDeleteIdGeneratorsAsPartOfOpenAfterSwitchingToSlave()
		 {
			  // GIVEN we're in master mode. We do that to allow HaIdGeneratorFactory to open id generators at all
			  _fac.switchToSlave();
			  File idFile = new File( "my.id" );
			  // ... opening an id generator as master
			  _fac.create( idFile, 10, true );

			  // WHEN
			  IdGenerator idGenerator = _fac.open( idFile, 10, IdType.NODE, () => 10L, Standard.LATEST_RECORD_FORMATS.node().MaxId );

			  // THEN
			  assertFalse( "Id file should've been deleted by now", _fs.fileExists( idFile ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.graphdb.TransientTransactionFailureException.class) public void shouldTranslateComExceptionsIntoTransientTransactionFailures()
		 public virtual void ShouldTranslateComExceptionsIntoTransientTransactionFailures()
		 {
			  when( _master.allocateIds( Null, any( typeof( IdType ) ) ) ).thenThrow( new ComException() );
			  IdGenerator generator = SwitchToSlave();
			  generator.NextId();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotUseForbiddenMinusOneIdFromIdBatches()
		 public virtual void ShouldNotUseForbiddenMinusOneIdFromIdBatches()
		 {
			  // GIVEN
			  long[] defragIds = new long[] { 3, 5 };
			  int size = 10;
			  long low = IdGeneratorImpl.INTEGER_MINUS_ONE - size / 2;
			  IdRange idRange = new IdRange( defragIds, low, size );

			  // WHEN
			  IdRangeIterator iterator = idRange.GetEnumerator();

			  // THEN
			  foreach ( long id in defragIds )
			  {
					assertEquals( id, iterator.NextId() );
			  }

			  int expectedRangeSize = size - 1; // due to the forbidden id
			  for ( long i = 0, expectedId = low; i < expectedRangeSize; i++, expectedId++ )
			  {
					if ( expectedId == IdGeneratorImpl.INTEGER_MINUS_ONE )
					{
						 expectedId++;
					}

					long id = iterator.NextId();
					assertNotEquals( IdGeneratorImpl.INTEGER_MINUS_ONE, id );
					assertEquals( expectedId, id );
			  }
			  assertEquals( VALUE_REPRESENTING_NULL, iterator.NextId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.neo4j.com.Response<IdAllocation> response(IdAllocation firstValue, IdAllocation... additionalValues)
		 private Response<IdAllocation> Response( IdAllocation firstValue, params IdAllocation[] additionalValues )
		 {
			  Response<IdAllocation> response = mock( typeof( Response ) );
			  when( response.ResponseConflict() ).thenReturn(firstValue, additionalValues);
			  return response;
		 }

		 private IdGenerator SwitchToSlave()
		 {
			  _fac.switchToSlave();
			  IdGenerator gen = _fac.open( new File( "someFile" ), 10, IdType.NODE, () => 1L, Standard.LATEST_RECORD_FORMATS.node().MaxId );
			  _masterDelegate.Delegate = _master;
			  return gen;
		 }
	}

}