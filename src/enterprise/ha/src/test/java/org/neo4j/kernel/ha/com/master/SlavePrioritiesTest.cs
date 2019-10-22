using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.ha.com.master
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class SlavePrioritiesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void roundRobinWithTwoSlavesAndPushFactorTwo()
		 public virtual void RoundRobinWithTwoSlavesAndPushFactorTwo()
		 {
			  // Given
			  SlavePriority roundRobin = SlavePriorities.RoundRobin();

			  // When
			  IEnumerator<Slave> slaves = roundRobin.Prioritize( slaves( 2, 3 ) ).GetEnumerator();

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( 2, slaves.next().ServerId );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( 3, slaves.next().ServerId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void roundRobinWithTwoSlavesAndPushFactorOne()
		 public virtual void RoundRobinWithTwoSlavesAndPushFactorOne()
		 {
			  // Given
			  SlavePriority roundRobin = SlavePriorities.RoundRobin();

			  // When
			  Slave slave1 = roundRobin.Prioritize( Slaves( 2, 3 ) ).GetEnumerator().next();
			  Slave slave2 = roundRobin.Prioritize( Slaves( 2, 3 ) ).GetEnumerator().next();

			  // Then
			  assertEquals( 2, slave1.ServerId );
			  assertEquals( 3, slave2.ServerId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void roundRobinWithTwoSlavesAndPushFactorOneWhenSlaveIsAdded()
		 public virtual void RoundRobinWithTwoSlavesAndPushFactorOneWhenSlaveIsAdded()
		 {
			  // Given
			  SlavePriority roundRobin = SlavePriorities.RoundRobin();

			  // When
			  Slave slave1 = roundRobin.Prioritize( Slaves( 2, 3 ) ).GetEnumerator().next();
			  Slave slave2 = roundRobin.Prioritize( Slaves( 2, 3 ) ).GetEnumerator().next();
			  Slave slave3 = roundRobin.Prioritize( Slaves( 2, 3, 4 ) ).GetEnumerator().next();

			  // Then
			  assertEquals( 2, slave1.ServerId );
			  assertEquals( 3, slave2.ServerId );
			  assertEquals( 4, slave3.ServerId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void roundRobinWithTwoSlavesAndPushFactorOneWhenSlaveIsRemoved()
		 public virtual void RoundRobinWithTwoSlavesAndPushFactorOneWhenSlaveIsRemoved()
		 {
			  // Given
			  SlavePriority roundRobin = SlavePriorities.RoundRobin();

			  // When
			  Slave slave1 = roundRobin.Prioritize( Slaves( 2, 3, 4 ) ).GetEnumerator().next();
			  Slave slave2 = roundRobin.Prioritize( Slaves( 2, 3, 4 ) ).GetEnumerator().next();
			  Slave slave3 = roundRobin.Prioritize( Slaves( 2, 3 ) ).GetEnumerator().next();

			  // Then
			  assertEquals( 2, slave1.ServerId );
			  assertEquals( 3, slave2.ServerId );
			  assertEquals( 2, slave3.ServerId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void roundRobinWithSingleSlave()
		 public virtual void RoundRobinWithSingleSlave()
		 {
			  // Given
			  SlavePriority roundRobin = SlavePriorities.RoundRobin();

			  // When
			  IEnumerator<Slave> slaves = roundRobin.Prioritize( slaves( 2 ) ).GetEnumerator();

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( 2, slaves.next().ServerId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void roundRobinWithNoSlaves()
		 public virtual void RoundRobinWithNoSlaves()
		 {
			  // Given
			  SlavePriority roundRobin = SlavePriorities.RoundRobin();

			  // When
			  IEnumerator<Slave> slaves = roundRobin.Prioritize( slaves() ).GetEnumerator();

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( slaves.hasNext() );
		 }

		 private static IEnumerable<Slave> Slaves( params int[] ids )
		 {
			  IList<Slave> slaves = new List<Slave>( ids.Length );
			  foreach ( int id in ids )
			  {
					Slave slaveMock = mock( typeof( Slave ) );
					when( slaveMock.ServerId ).thenReturn( id );
					slaves.Add( slaveMock );
			  }
			  return slaves;
		 }
	}

}