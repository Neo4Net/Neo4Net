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
namespace Neo4Net.causalclustering.protocol.handshake
{
	using Test = org.junit.Test;


	using Iterators = Neo4Net.Collections.Helpers.Iterators;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocolCategory.RAFT;

	public class SupportedProtocolsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMutuallySupportIntersectionOfParameterVersionsSuperset()
		 public virtual void ShouldMutuallySupportIntersectionOfParameterVersionsSuperset()
		 {
			  // given
			  ApplicationSupportedProtocols supportedProtocols = new ApplicationSupportedProtocols( RAFT, Arrays.asList( 1, 2 ) );

			  // when
			  ISet<int> mutuallySupported = supportedProtocols.MutuallySupportedVersionsFor( Iterators.asSet( 1, 2, 3 ) );

			  // then
			  assertThat( mutuallySupported, containsInAnyOrder( 1, 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMutuallySupportIntersectionOfParameterVersionsSubset()
		 public virtual void ShouldMutuallySupportIntersectionOfParameterVersionsSubset()
		 {
			  // given
			  ApplicationSupportedProtocols supportedProtocols = new ApplicationSupportedProtocols( RAFT, Arrays.asList( 4, 5, 6 ) );

			  // when
			  ISet<int> mutuallySupported = supportedProtocols.MutuallySupportedVersionsFor( Iterators.asSet( 4, 5 ) );

			  // then
			  assertThat( mutuallySupported, containsInAnyOrder( 4, 5 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMutuallySupportParameterIfEmptyVersions()
		 public virtual void ShouldMutuallySupportParameterIfEmptyVersions()
		 {
			  // given
			  ApplicationSupportedProtocols supportedProtocols = new ApplicationSupportedProtocols( RAFT, emptyList() );

			  // when
			  ISet<int> mutuallySupported = supportedProtocols.MutuallySupportedVersionsFor( Iterators.asSet( 7, 8 ) );

			  // then
			  assertThat( mutuallySupported, containsInAnyOrder( 7, 8 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMutuallySupportNothingIfParametersEmpty()
		 public virtual void ShouldMutuallySupportNothingIfParametersEmpty()
		 {
			  // given
			  ApplicationSupportedProtocols supportedProtocols = new ApplicationSupportedProtocols( RAFT, Arrays.asList( 1, 2 ) );

			  // when
			  ISet<int> mutuallySupported = supportedProtocols.MutuallySupportedVersionsFor( emptySet() );

			  // then
			  assertThat( mutuallySupported, empty() );
		 }
	}

}