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
namespace Neo4Net.ha
{
	using Test = org.junit.Test;


	using ElectionCredentials = Neo4Net.cluster.protocol.election.ElectionCredentials;
	using DefaultElectionCredentials = Neo4Net.Kernel.ha.cluster.DefaultElectionCredentials;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;

	public class DefaultElectionCredentialsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCompareToDifferentTxId()
		 public virtual void TestCompareToDifferentTxId()
		 {
			  DefaultElectionCredentials highTxId = new DefaultElectionCredentials( 3, 12, false );

			  DefaultElectionCredentials mediumTxId = new DefaultElectionCredentials( 1, 11, false );

			  DefaultElectionCredentials lowTxId = new DefaultElectionCredentials( 2, 10, false );

			  IList<ElectionCredentials> toSort = new List<ElectionCredentials>( 2 );
			  toSort.Add( mediumTxId );
			  toSort.Add( highTxId );
			  toSort.Add( lowTxId );
			  toSort.Sort();
			  assertEquals( toSort[0], lowTxId );
			  assertEquals( toSort[1], mediumTxId );
			  assertEquals( toSort[2], highTxId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCompareToSameTxId()
		 public virtual void TestCompareToSameTxId()
		 {
			  // Lower id means higher priority
			  DefaultElectionCredentials highSameTxId = new DefaultElectionCredentials( 1, 10, false );

			  DefaultElectionCredentials lowSameTxId = new DefaultElectionCredentials( 2, 10, false );

			  IList<ElectionCredentials> toSort = new List<ElectionCredentials>( 2 );
			  toSort.Add( highSameTxId );
			  toSort.Add( lowSameTxId );
			  toSort.Sort();
			  assertEquals( toSort[0], lowSameTxId );
			  assertEquals( toSort[1], highSameTxId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExistingMasterLosesWhenComparedToHigherTxIdHigherId()
		 public virtual void TestExistingMasterLosesWhenComparedToHigherTxIdHigherId()
		 {
			  DefaultElectionCredentials currentMaster = new DefaultElectionCredentials( 1, 10, true );
			  DefaultElectionCredentials incoming = new DefaultElectionCredentials( 2, 11, false );

			  IList<ElectionCredentials> toSort = new List<ElectionCredentials>( 2 );
			  toSort.Add( currentMaster );
			  toSort.Add( incoming );
			  toSort.Sort();

			  assertEquals( toSort[0], currentMaster );
			  assertEquals( toSort[1], incoming );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExistingMasterWinsWhenComparedToLowerIdSameTxId()
		 public virtual void TestExistingMasterWinsWhenComparedToLowerIdSameTxId()
		 {
			  DefaultElectionCredentials currentMaster = new DefaultElectionCredentials( 2, 10, true );
			  DefaultElectionCredentials incoming = new DefaultElectionCredentials( 1, 10, false );

			  IList<ElectionCredentials> toSort = new List<ElectionCredentials>( 2 );
			  toSort.Add( currentMaster );
			  toSort.Add( incoming );
			  toSort.Sort();

			  assertEquals( toSort[0], incoming );
			  assertEquals( toSort[1], currentMaster );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExistingMasterWinsWhenComparedToHigherIdLowerTxId()
		 public virtual void TestExistingMasterWinsWhenComparedToHigherIdLowerTxId()
		 {
			  DefaultElectionCredentials currentMaster = new DefaultElectionCredentials( 1, 10, true );
			  DefaultElectionCredentials incoming = new DefaultElectionCredentials( 2, 9, false );

			  IList<ElectionCredentials> toSort = new List<ElectionCredentials>( 2 );
			  toSort.Add( currentMaster );
			  toSort.Add( incoming );
			  toSort.Sort();

			  assertEquals( toSort[0], incoming );
			  assertEquals( toSort[1], currentMaster );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEquals()
		 public virtual void TestEquals()
		 {
			  DefaultElectionCredentials sameAsNext = new DefaultElectionCredentials( 1, 10, false );

			  DefaultElectionCredentials sameAsPrevious = new DefaultElectionCredentials( 1, 10, false );

			  assertEquals( sameAsNext, sameAsPrevious );
			  assertEquals( sameAsNext, sameAsNext );

			  DefaultElectionCredentials differentTxIdFromNext = new DefaultElectionCredentials( 1, 11, false );

			  DefaultElectionCredentials differentTxIdFromPrevious = new DefaultElectionCredentials( 1, 10, false );

			  assertNotEquals( differentTxIdFromNext, differentTxIdFromPrevious );
			  assertNotEquals( differentTxIdFromPrevious, differentTxIdFromNext );

			  DefaultElectionCredentials differentURIFromNext = new DefaultElectionCredentials( 1, 11, false );

			  DefaultElectionCredentials differentURIFromPrevious = new DefaultElectionCredentials( 2, 11, false );

			  assertNotEquals( differentTxIdFromNext, differentURIFromPrevious );
			  assertNotEquals( differentTxIdFromPrevious, differentURIFromNext );
		 }
	}

}