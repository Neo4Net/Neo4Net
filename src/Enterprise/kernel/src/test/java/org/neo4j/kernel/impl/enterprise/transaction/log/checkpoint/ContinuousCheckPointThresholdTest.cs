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
namespace Neo4Net.Kernel.impl.enterprise.transaction.log.checkpoint
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ContinuousCheckPointThresholdTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void continuousCheckPointMustReachThresholdOnEveryCommit()
		 public virtual void ContinuousCheckPointMustReachThresholdOnEveryCommit()
		 {
			  ContinuousCheckPointThreshold threshold = new ContinuousCheckPointThreshold();
			  threshold.Initialize( 10 );
			  assertFalse( threshold.ThresholdReached( 10 ) );
			  assertTrue( threshold.ThresholdReached( 11 ) );
			  assertTrue( threshold.ThresholdReached( 11 ) );
			  threshold.CheckPointHappened( 12 );
			  assertFalse( threshold.ThresholdReached( 12 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void continuousThresholdMustNotBusySpin()
		 public virtual void ContinuousThresholdMustNotBusySpin()
		 {
			  ContinuousCheckPointThreshold threshold = new ContinuousCheckPointThreshold();
			  assertThat( threshold.CheckFrequencyMillis(), greaterThan(0L) );
		 }
	}

}