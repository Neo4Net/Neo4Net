using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.core
{
	using StreamMatchers = co.unruly.matchers.StreamMatchers;
	using Test = org.junit.Test;


	using Neo4Net.causalclustering.protocol;
	using ApplicationSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ApplicationSupportedProtocols;
	using ModifierSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ModifierSupportedProtocols;
	using Neo4Net.causalclustering.protocol.handshake;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocols.COMPRESSION_SNAPPY;

	public class SupportedProtocolCreatorTest
	{

		 private NullLogProvider _log = NullLogProvider.Instance;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnRaftProtocol()
		 public virtual void ShouldReturnRaftProtocol()
		 {
			  // given
			  Config config = Config.defaults();

			  // when
			  ApplicationSupportedProtocols supportedRaftProtocol = ( new SupportedProtocolCreator( config, _log ) ).createSupportedRaftProtocol();

			  // then
			  assertThat( supportedRaftProtocol.Identifier(), equalTo(Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocolCategory.Raft) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptyVersionSupportedRaftProtocolIfNoVersionsConfigured()
		 public virtual void ShouldReturnEmptyVersionSupportedRaftProtocolIfNoVersionsConfigured()
		 {
			  // given
			  Config config = Config.defaults();

			  // when
			  ApplicationSupportedProtocols supportedRaftProtocol = ( new SupportedProtocolCreator( config, _log ) ).createSupportedRaftProtocol();

			  // then
			  assertThat( supportedRaftProtocol.Versions(), empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFilterUnknownRaftImplementations()
		 public virtual void ShouldFilterUnknownRaftImplementations()
		 {
			  // given
			  Config config = Config.defaults( CausalClusteringSettings.RaftImplementations, "1, 2, 3" );

			  // when
			  ApplicationSupportedProtocols supportedRaftProtocol = ( new SupportedProtocolCreator( config, _log ) ).createSupportedRaftProtocol();

			  // then
			  assertThat( supportedRaftProtocol.Versions(), contains(1, 2) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnConfiguredRaftProtocolVersions()
		 public virtual void ShouldReturnConfiguredRaftProtocolVersions()
		 {
			  // given
			  Config config = Config.defaults( CausalClusteringSettings.RaftImplementations, "1" );

			  // when
			  ApplicationSupportedProtocols supportedRaftProtocol = ( new SupportedProtocolCreator( config, _log ) ).createSupportedRaftProtocol();

			  // then
			  assertThat( supportedRaftProtocol.Versions(), contains(1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void shouldThrowIfVersionsSpecifiedButAllUnknown()
		 public virtual void ShouldThrowIfVersionsSpecifiedButAllUnknown()
		 {
			  // given
			  Config config = Config.defaults( CausalClusteringSettings.RaftImplementations, int.MaxValue.ToString() );

			  // when
			  ApplicationSupportedProtocols supportedRaftProtocol = ( new SupportedProtocolCreator( config, _log ) ).createSupportedRaftProtocol();

			  // then throw
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnModifiersIfNoVersionsSpecified()
		 public virtual void ShouldNotReturnModifiersIfNoVersionsSpecified()
		 {
			  // given
			  Config config = Config.defaults();

			  // when
			  IList<ModifierSupportedProtocols> supportedModifierProtocols = ( new SupportedProtocolCreator( config, _log ) ).createSupportedModifierProtocols();

			  // then
			  assertThat( supportedModifierProtocols, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnACompressionModifierIfCompressionVersionsSpecified()
		 public virtual void ShouldReturnACompressionModifierIfCompressionVersionsSpecified()
		 {
			  // given
			  Config config = Config.defaults( CausalClusteringSettings.CompressionImplementations, COMPRESSION_SNAPPY.implementation() );

			  // when
			  IList<ModifierSupportedProtocols> supportedModifierProtocols = ( new SupportedProtocolCreator( config, _log ) ).createSupportedModifierProtocols();

			  // then
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  Stream<Neo4Net.causalclustering.protocol.Protocol_Category<Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol>> identifiers = supportedModifierProtocols.Select( SupportedProtocols::identifier );
			  assertThat( identifiers, StreamMatchers.contains( Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory.Compression ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnCompressionWithVersionsSpecified()
		 public virtual void ShouldReturnCompressionWithVersionsSpecified()
		 {
			  // given
			  Config config = Config.defaults( CausalClusteringSettings.CompressionImplementations, COMPRESSION_SNAPPY.implementation() );

			  // when
			  IList<ModifierSupportedProtocols> supportedModifierProtocols = ( new SupportedProtocolCreator( config, _log ) ).createSupportedModifierProtocols();

			  // then
			  IList<string> versions = supportedModifierProtocols[0].Versions();
			  assertThat( versions, contains( COMPRESSION_SNAPPY.implementation() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnCompressionWithVersionsSpecifiedCaseInsensitive()
		 public virtual void ShouldReturnCompressionWithVersionsSpecifiedCaseInsensitive()
		 {
			  // given
			  Config config = Config.defaults( CausalClusteringSettings.CompressionImplementations, COMPRESSION_SNAPPY.implementation().ToLower() );

			  // when
			  IList<ModifierSupportedProtocols> supportedModifierProtocols = ( new SupportedProtocolCreator( config, _log ) ).createSupportedModifierProtocols();

			  // then
			  IList<string> versions = supportedModifierProtocols[0].Versions();
			  assertThat( versions, contains( COMPRESSION_SNAPPY.implementation() ) );
		 }
	}

}