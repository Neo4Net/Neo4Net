using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.protocol.handshake
{
	using OptionalMatchers = co.unruly.matchers.OptionalMatchers;
	using Matchers = org.hamcrest.Matchers;
	using Test = org.junit.Test;


	using Neo4Net.causalclustering.protocol;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ApplicationProtocolCategory.RAFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class ApplicationProtocolRepositoryTest
	{
		 private ApplicationProtocolRepository _applicationProtocolRepository = new ApplicationProtocolRepository( TestProtocols_TestApplicationProtocols.values(), new ApplicationSupportedProtocols(RAFT, TestProtocols_TestApplicationProtocols.listVersionsOf(RAFT)) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptyIfUnknownVersion()
		 public virtual void ShouldReturnEmptyIfUnknownVersion()
		 {
			  // when
			  Optional<Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocol> applicationProtocol = _applicationProtocolRepository.select( RAFT.canonicalName(), -1 );

			  // then
			  assertThat( applicationProtocol, OptionalMatchers.empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptyIfUnknownName()
		 public virtual void ShouldReturnEmptyIfUnknownName()
		 {
			  // when
			  Optional<Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocol> applicationProtocol = _applicationProtocolRepository.select( "not a real protocol", 1 );

			  // then
			  assertThat( applicationProtocol, OptionalMatchers.empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptyIfNoVersions()
		 public virtual void ShouldReturnEmptyIfNoVersions()
		 {
			  // when
			  Optional<Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocol> applicationProtocol = _applicationProtocolRepository.select( RAFT.canonicalName(), emptySet() );

			  // then
			  assertThat( applicationProtocol, OptionalMatchers.empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnProtocolIfKnownNameAndVersion()
		 public virtual void ShouldReturnProtocolIfKnownNameAndVersion()
		 {
			  // when
			  Optional<Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocol> applicationProtocol = _applicationProtocolRepository.select( RAFT.canonicalName(), 1 );

			  // then
			  assertThat( applicationProtocol, OptionalMatchers.contains( TestProtocols_TestApplicationProtocols.RAFT_1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnKnownProtocolVersionWhenFirstGivenVersionNotKnown()
		 public virtual void ShouldReturnKnownProtocolVersionWhenFirstGivenVersionNotKnown()
		 {
			  // when
			  Optional<Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocol> applicationProtocol = _applicationProtocolRepository.select( RAFT.canonicalName(), asSet(-1, 1) );

			  // then
			  assertThat( applicationProtocol, OptionalMatchers.contains( TestProtocols_TestApplicationProtocols.RAFT_1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnApplicationProtocolOfHighestVersionNumberRequestedAndSupported()
		 public virtual void ShouldReturnApplicationProtocolOfHighestVersionNumberRequestedAndSupported()
		 {
			  // when
			  Optional<Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocol> applicationProtocol = _applicationProtocolRepository.select( RAFT.canonicalName(), asSet(389432, 1, 3, 2, 71234) );

			  // then
			  assertThat( applicationProtocol, OptionalMatchers.contains( TestProtocols_TestApplicationProtocols.RAFT_3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeAllProtocolsInSelectionIfEmptyVersionsProvided()
		 public virtual void ShouldIncludeAllProtocolsInSelectionIfEmptyVersionsProvided()
		 {
			  // when
			  ProtocolSelection<int, Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocol> protocolSelection = _applicationProtocolRepository.getAll( RAFT, emptyList() );

			  // then
			  int?[] expectedRaftVersions = TestProtocols_TestApplicationProtocols.allVersionsOf( RAFT );
			  assertThat( protocolSelection.Versions(), Matchers.containsInAnyOrder(expectedRaftVersions) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeProtocolsInSelectionWithVersionsLimitedByThoseConfigured()
		 public virtual void ShouldIncludeProtocolsInSelectionWithVersionsLimitedByThoseConfigured()
		 {
			  // given
			  int?[] expectedRaftVersions = new int?[] { 1 };

			  // when
			  ProtocolSelection<int, Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocol> protocolSelection = _applicationProtocolRepository.getAll( RAFT, asList( expectedRaftVersions ) );

			  // then
			  assertThat( protocolSelection.Versions(), Matchers.containsInAnyOrder(expectedRaftVersions) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeProtocolsInSelectionWithVersionsLimitedByThoseExisting()
		 public virtual void ShouldIncludeProtocolsInSelectionWithVersionsLimitedByThoseExisting()
		 {
			  // given
			  int?[] expectedRaftVersions = TestProtocols_TestApplicationProtocols.allVersionsOf( RAFT );
			  IList<int> configuredRaftVersions = Stream.concat( Stream.of( expectedRaftVersions ), Stream.of( int.MaxValue ) ).collect( Collectors.toList() );

			  // when
			  ProtocolSelection<int, Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocol> protocolSelection = _applicationProtocolRepository.getAll( RAFT, configuredRaftVersions );

			  // then
			  assertThat( protocolSelection.Versions(), Matchers.containsInAnyOrder(expectedRaftVersions) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void shouldThrowIfNoIntersectionBetweenExistingAndConfiguredVersions()
		 public virtual void ShouldThrowIfNoIntersectionBetweenExistingAndConfiguredVersions()
		 {
			  // given
			  IList<int> configuredRaftVersions = Arrays.asList( int.MaxValue );

			  // when
			  _applicationProtocolRepository.getAll( RAFT, configuredRaftVersions );

			  // then throw
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void shouldNotInstantiateIfDuplicateProtocolsSupplied()
		 public virtual void ShouldNotInstantiateIfDuplicateProtocolsSupplied()
		 {
			  // given
			  Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocol protocol = new Protocol_ApplicationProtocolAnonymousInnerClass( this );
			  Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocol[] protocols = new Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocol[] { protocol, protocol };

			  // when
			  new ApplicationProtocolRepository( protocols, new ApplicationSupportedProtocols( RAFT, TestProtocols_TestApplicationProtocols.listVersionsOf( RAFT ) ) );

			  // then throw
		 }

		 private class Protocol_ApplicationProtocolAnonymousInnerClass : Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocol
		 {
			 private readonly ApplicationProtocolRepositoryTest _outerInstance;

			 public Protocol_ApplicationProtocolAnonymousInnerClass( ApplicationProtocolRepositoryTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public string category()
			 {
				  return "foo";
			 }

			 public int? implementation()
			 {
				  return 1;
			 }
		 }
	}

}