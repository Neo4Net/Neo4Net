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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;

	using Test = org.junit.Test;

	public class PaxosInstanceStoreTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnSameObjectWhenAskedById()
		 public virtual void ShouldReturnSameObjectWhenAskedById()
		 {
			  // Given
			  PaxosInstanceStore theStore = new PaxosInstanceStore();
			  InstanceId currentInstanceId = new InstanceId( 1 );

			  // When
			  PaxosInstance currentInstance = theStore.GetPaxosInstance( currentInstanceId );

			  // Then
			  assertSame( currentInstance, theStore.GetPaxosInstance( currentInstanceId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepAtMostGivenNumberOfInstances()
		 public virtual void ShouldKeepAtMostGivenNumberOfInstances()
		 {
			  // Given
			  const int instancesToKeep = 10;
			  PaxosInstanceStore theStore = new PaxosInstanceStore( instancesToKeep );

			  // Keeps the first instance inserted, which is the first to be removed
			  PaxosInstance firstInstance = null;

			  // When
			  for ( int i = 0; i < instancesToKeep + 1; i++ )
			  {
					InstanceId currentInstanceId = new InstanceId( i );
					PaxosInstance currentInstance = theStore.GetPaxosInstance( currentInstanceId );
					theStore.Delivered( currentInstance.Id );
					if ( firstInstance == null )
					{
						 firstInstance = currentInstance;
					}
			  }

			  // Then
			  // The first instance must have been removed now
			  PaxosInstance toTest = theStore.GetPaxosInstance( firstInstance.Id );
			  assertNotSame( firstInstance, toTest );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leaveShouldClearStoredInstances()
		 public virtual void LeaveShouldClearStoredInstances()
		 {
			  // Given
			  PaxosInstanceStore theStore = new PaxosInstanceStore();
			  InstanceId currentInstanceId = new InstanceId( 1 );

			  // When
			  PaxosInstance currentInstance = theStore.GetPaxosInstance( currentInstanceId );
			  theStore.Leave();

			  // Then
			  assertNotSame( currentInstance, theStore.GetPaxosInstance( currentInstanceId ) );
		 }
	}

}