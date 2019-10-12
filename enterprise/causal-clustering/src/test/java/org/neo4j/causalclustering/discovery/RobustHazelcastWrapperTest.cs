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
namespace Org.Neo4j.causalclustering.discovery
{
	using HazelcastInstance = com.hazelcast.core.HazelcastInstance;
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class RobustHazelcastWrapperTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReconnectIfHazelcastConnectionInvalidated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReconnectIfHazelcastConnectionInvalidated()
		 {
			  // given
			  HazelcastConnector connector = mock( typeof( HazelcastConnector ) );
			  HazelcastInstance hzInstance = mock( typeof( HazelcastInstance ) );

			  when( connector.ConnectToHazelcast() ).thenReturn(hzInstance);

			  RobustHazelcastWrapper hzWrapper = new RobustHazelcastWrapper( connector );

			  // when
			  hzWrapper.Perform(hz =>
			  {
			  });

			  // then
			  verify( connector, times( 1 ) ).connectToHazelcast();

			  // then
			  try
			  {
					hzWrapper.Perform(hz =>
					{
					 throw new com.hazelcast.core.HazelcastInstanceNotActiveException();
					});
					fail();
			  }
			  catch ( HazelcastInstanceNotActiveException )
			  {
					// expected
			  }

			  // when
			  hzWrapper.Perform(hz =>
			  {
			  });
			  hzWrapper.Perform(hz =>
			  {
			  });

			  // then
			  verify( connector, times( 2 ) ).connectToHazelcast();
		 }
	}

}