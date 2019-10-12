﻿/*
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
namespace Org.Neo4j.causalclustering.core.consensus
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.MajorityIncludingSelfQuorum.isQuorum;

	public class QuorumStrategyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDecideIfWeHaveAMajorityCorrectly()
		 public virtual void ShouldDecideIfWeHaveAMajorityCorrectly()
		 {
			  // the assumption in these tests is that we always vote for ourselves
			  assertTrue( isQuorum( 0, 1, 0 ) );

			  assertFalse( isQuorum( 0, 2, 0 ) );
			  assertTrue( isQuorum( 0, 2, 1 ) );

			  assertFalse( isQuorum( 0, 3, 0 ) );
			  assertTrue( isQuorum( 0, 3, 1 ) );
			  assertTrue( isQuorum( 0, 3, 2 ) );

			  assertFalse( isQuorum( 0, 4, 0 ) );
			  assertFalse( isQuorum( 0, 4, 1 ) );
			  assertTrue( isQuorum( 0, 4, 2 ) );
			  assertTrue( isQuorum( 0, 4, 3 ) );

			  assertFalse( isQuorum( 0, 5, 0 ) );
			  assertFalse( isQuorum( 0, 5, 1 ) );
			  assertTrue( isQuorum( 0, 5, 2 ) );
			  assertTrue( isQuorum( 0, 5, 3 ) );
			  assertTrue( isQuorum( 0, 5, 4 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDecideIfWeHaveAMajorityCorrectlyUsingMinQuorum()
		 public virtual void ShouldDecideIfWeHaveAMajorityCorrectlyUsingMinQuorum()
		 {
			  // Then
			  assertFalse( isQuorum( 2, 1, 0 ) );

			  assertFalse( isQuorum( 2, 2, 0 ) );
			  assertTrue( isQuorum( 2, 2, 1 ) );

			  assertFalse( isQuorum( 2, 3, 0 ) );
			  assertTrue( isQuorum( 2, 3, 1 ) );
			  assertTrue( isQuorum( 2, 3, 2 ) );

			  assertFalse( isQuorum( 2, 4, 0 ) );
			  assertFalse( isQuorum( 2, 4, 1 ) );
			  assertTrue( isQuorum( 2, 4, 2 ) );
			  assertTrue( isQuorum( 2, 4, 3 ) );

			  assertFalse( isQuorum( 2, 5, 0 ) );
			  assertFalse( isQuorum( 2, 5, 1 ) );
			  assertTrue( isQuorum( 2, 5, 2 ) );
			  assertTrue( isQuorum( 2, 5, 3 ) );
			  assertTrue( isQuorum( 2, 5, 4 ) );
		 }
	}

}