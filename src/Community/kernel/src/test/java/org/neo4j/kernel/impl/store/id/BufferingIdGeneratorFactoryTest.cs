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
namespace Neo4Net.Kernel.impl.store.id
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using KernelTransactionsSnapshot = Neo4Net.Kernel.Impl.Api.KernelTransactionsSnapshot;
	using CommunityIdTypeConfigurationProvider = Neo4Net.Kernel.impl.store.id.configuration.CommunityIdTypeConfigurationProvider;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class BufferingIdGeneratorFactoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fs = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDelayFreeingOfAggressivelyReusedIds()
		 public virtual void ShouldDelayFreeingOfAggressivelyReusedIds()
		 {
			  // GIVEN
			  MockedIdGeneratorFactory actual = new MockedIdGeneratorFactory();
			  ControllableSnapshotSupplier boundaries = new ControllableSnapshotSupplier();
			  BufferingIdGeneratorFactory bufferingIdGeneratorFactory = new BufferingIdGeneratorFactory( actual, IdReuseEligibility_Fields.Always, new CommunityIdTypeConfigurationProvider() );
			  bufferingIdGeneratorFactory.Initialize( boundaries );
			  IdGenerator idGenerator = bufferingIdGeneratorFactory.Open( new File( "doesnt-matter" ), 10, IdType.StringBlock, () => 0L, int.MaxValue );

			  // WHEN
			  idGenerator.FreeId( 7 );
			  verifyNoMoreInteractions( actual.Get( IdType.StringBlock ) );

			  // after some maintenance and transaction still not closed
			  bufferingIdGeneratorFactory.Maintenance();
			  verifyNoMoreInteractions( actual.Get( IdType.StringBlock ) );

			  // although after transactions have all closed
			  boundaries.SetMostRecentlyReturnedSnapshotToAllClosed();
			  bufferingIdGeneratorFactory.Maintenance();

			  // THEN
			  verify( actual.Get( IdType.StringBlock ) ).freeId( 7 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDelayFreeingOfAggressivelyReusedIdsConsideringTimeAsWell()
		 public virtual void ShouldDelayFreeingOfAggressivelyReusedIdsConsideringTimeAsWell()
		 {
			  // GIVEN
			  MockedIdGeneratorFactory actual = new MockedIdGeneratorFactory();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.time.FakeClock clock = org.neo4j.time.Clocks.fakeClock();
			  FakeClock clock = Clocks.fakeClock();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long safeZone = MINUTES.toMillis(1);
			  long safeZone = MINUTES.toMillis( 1 );
			  ControllableSnapshotSupplier boundaries = new ControllableSnapshotSupplier();
			  BufferingIdGeneratorFactory bufferingIdGeneratorFactory = new BufferingIdGeneratorFactory( actual, t => clock.Millis() - t.snapshotTime() >= safeZone, new CommunityIdTypeConfigurationProvider() );
			  bufferingIdGeneratorFactory.Initialize( boundaries );

			  IdGenerator idGenerator = bufferingIdGeneratorFactory.Open( new File( "doesnt-matter" ), 10, IdType.StringBlock, () => 0L, int.MaxValue );

			  // WHEN
			  idGenerator.FreeId( 7 );
			  verifyNoMoreInteractions( actual.Get( IdType.StringBlock ) );

			  // after some maintenance and transaction still not closed
			  bufferingIdGeneratorFactory.Maintenance();
			  verifyNoMoreInteractions( actual.Get( IdType.StringBlock ) );

			  // although after transactions have all closed
			  boundaries.SetMostRecentlyReturnedSnapshotToAllClosed();
			  bufferingIdGeneratorFactory.Maintenance();
			  // ... the clock would still say "nope" so no interaction
			  verifyNoMoreInteractions( actual.Get( IdType.StringBlock ) );

			  // then finally after time has passed as well
			  clock.Forward( 70, SECONDS );
			  bufferingIdGeneratorFactory.Maintenance();

			  // THEN
			  verify( actual.Get( IdType.StringBlock ) ).freeId( 7 );
		 }

		 private class ControllableSnapshotSupplier : System.Func<KernelTransactionsSnapshot>
		 {
			  internal KernelTransactionsSnapshot MostRecentlyReturned;

			  public override KernelTransactionsSnapshot Get()
			  {
					return MostRecentlyReturned = mock( typeof( KernelTransactionsSnapshot ) );
			  }

			  internal virtual void SetMostRecentlyReturnedSnapshotToAllClosed()
			  {
					when( MostRecentlyReturned.allClosed() ).thenReturn(true);
			  }
		 }

		 private class MockedIdGeneratorFactory : IdGeneratorFactory
		 {
			  internal readonly IdGenerator[] Generators = new IdGenerator[Enum.GetValues( typeof( IdType ) ).length];

			  public override IdGenerator Open( File filename, IdType idType, System.Func<long> highId, long maxId )
			  {
					return Open( filename, 0, idType, highId, maxId );
			  }

			  public override IdGenerator Open( File filename, int grabSize, IdType idType, System.Func<long> highId, long maxId )
			  {
					return Generators[( int )idType] = mock( typeof( IdGenerator ) );
			  }

			  public override void Create( File filename, long highId, bool throwIfFileExists )
			  {
			  }

			  public override IdGenerator Get( IdType idType )
			  {
					return Generators[( int )idType];
			  }
		 }
	}

}