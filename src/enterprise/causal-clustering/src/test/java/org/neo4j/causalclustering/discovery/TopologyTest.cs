using System.Collections;
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
namespace Neo4Net.causalclustering.discovery
{

	using Test = org.junit.Test;

	using MemberId = Neo4Net.causalclustering.identity.MemberId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class TopologyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void identicalTopologiesShouldHaveNoDifference()
		 public virtual void IdenticalTopologiesShouldHaveNoDifference()
		 {
			  // given
			  IDictionary<MemberId, ReadReplicaInfo> readReplicaMembers = RandomMembers( 5 );

			  TestTopology topology = new TestTopology( readReplicaMembers );

			  // when
			  TopologyDifference diff = topology.difference( topology );

			  // then
			  assertThat( diff.Added(), hasSize(0) );
			  assertThat( diff.Removed(), hasSize(0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectAddedMembers()
		 public virtual void ShouldDetectAddedMembers()
		 {
			  // given
			  IDictionary<MemberId, ReadReplicaInfo> initialMembers = RandomMembers( 3 );

			  IDictionary<MemberId, ReadReplicaInfo> newMembers = new Dictionary<MemberId, ReadReplicaInfo>( initialMembers );
			  int newMemberQuantity = 2;
			  IntStream.range( 0, newMemberQuantity ).forEach( ignored => putRandomMember( newMembers ) );

			  TestTopology topology = new TestTopology( initialMembers );

			  // when
			  TopologyDifference diff = topology.difference( new TestTopology( newMembers ) );

			  // then
			  assertThat( diff.Added(), hasSize(newMemberQuantity) );
			  assertThat( diff.Removed(), hasSize(0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectRemovedMembers()
		 public virtual void ShouldDetectRemovedMembers()
		 {
			  IDictionary<MemberId, ReadReplicaInfo> initialMembers = RandomMembers( 3 );

			  IDictionary<MemberId, ReadReplicaInfo> newMembers = new Dictionary<MemberId, ReadReplicaInfo>( initialMembers );
			  int removedMemberQuantity = 2;
			  IntStream.range( 0, removedMemberQuantity ).forEach( ignored => removeArbitraryMember( newMembers ) );

			  TestTopology topology = new TestTopology( initialMembers );

			  // when
			  TopologyDifference diff = topology.difference( new TestTopology( newMembers ) );

			  // then
			  assertThat( diff.Added(), hasSize(0) );
			  assertThat( diff.Removed(), hasSize(removedMemberQuantity) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectAddedAndRemovedMembers()
		 public virtual void ShouldDetectAddedAndRemovedMembers()
		 {
			  // given
			  int initialQuantity = 4;
			  int newQuantity = 5;
			  IDictionary<MemberId, ReadReplicaInfo> initialMembers = RandomMembers( initialQuantity );
			  IDictionary<MemberId, ReadReplicaInfo> newMembers = RandomMembers( newQuantity );

			  TestTopology topology = new TestTopology( initialMembers );

			  // when
			  TopologyDifference diff = topology.difference( new TestTopology( newMembers ) );

			  // then
			  assertThat( diff.Added(), hasSize(newQuantity) );
			  assertThat( diff.Removed(), hasSize(initialQuantity) );
		 }

		 private class TestTopology : Topology<ReadReplicaInfo>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IDictionary<MemberId, ReadReplicaInfo> MembersConflict;

			  internal TestTopology( IDictionary<MemberId, ReadReplicaInfo> members )
			  {
					this.MembersConflict = members;
			  }

			  public override IDictionary<MemberId, ReadReplicaInfo> Members()
			  {
					return MembersConflict;
			  }

			  public override Topology<ReadReplicaInfo> FilterTopologyByDb( string dbName )
			  {
					IDictionary<MemberId, ReadReplicaInfo> newMembers = this.MembersConflict.SetOfKeyValuePairs().Where(e => e.Value.DatabaseName.Equals(dbName)).ToDictionary(DictionaryEntry.getKey, DictionaryEntry.getValue);
					return new TestTopology( newMembers );
			  }
		 }

		 private IDictionary<MemberId, ReadReplicaInfo> RandomMembers( int quantity )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Stream.generate( System.Guid.randomUUID ).limit( quantity ).collect( Collectors.toMap( MemberId::new, ignored => mock( typeof( ReadReplicaInfo ) ) ) );

		 }

		 private void PutRandomMember( IDictionary<MemberId, ReadReplicaInfo> newmembers )
		 {
			  newmembers[new MemberId( System.Guid.randomUUID() )] = mock(typeof(ReadReplicaInfo));
		 }

		 private void RemoveArbitraryMember( IDictionary<MemberId, ReadReplicaInfo> members )
		 {
			  members.Remove( members.Keys.First().orElseThrow(() => new AssertionError("Removing members of an empty map")) );
		 }
	}

}