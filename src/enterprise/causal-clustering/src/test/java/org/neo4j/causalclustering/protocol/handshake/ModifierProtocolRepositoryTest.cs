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
	using OptionalMatchers = co.unruly.matchers.OptionalMatchers;
	using Matchers = org.hamcrest.Matchers;
	using Test = org.junit.Test;


	using Neo4Net.causalclustering.protocol;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory.COMPRESSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory.GRATUITOUS_OBFUSCATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.protocol.handshake.TestProtocols_TestModifierProtocols.LZ4;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.protocol.handshake.TestProtocols_TestModifierProtocols.LZO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.protocol.handshake.TestProtocols_TestModifierProtocols.NAME_CLASH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.protocol.handshake.TestProtocols_TestModifierProtocols.SNAPPY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;

	/// <seealso cref= ApplicationProtocolRepositoryTest for tests on base class </seealso>
	public class ModifierProtocolRepositoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnModifierProtocolOfFirstConfiguredVersionRequestedAndSupported()
		 public virtual void ShouldReturnModifierProtocolOfFirstConfiguredVersionRequestedAndSupported()
		 {
			  // given
			  IList<ModifierSupportedProtocols> supportedProtocols = asList( new ModifierSupportedProtocols( COMPRESSION, new IList<string> { LZO.implementation(), SNAPPY.implementation(), LZ4.implementation() } ), new ModifierSupportedProtocols(GRATUITOUS_OBFUSCATION, new IList<string> { NAME_CLASH.implementation() }) );
			  ModifierProtocolRepository modifierProtocolRepository = new ModifierProtocolRepository( TestProtocols_TestModifierProtocols.values(), supportedProtocols );
			  // when
			  Optional<Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol> modifierProtocol = modifierProtocolRepository.Select( COMPRESSION.canonicalName(), asSet("bzip2", SNAPPY.implementation(), LZ4.implementation(), LZO.implementation(), "fast_lz") );

			  // then
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( modifierProtocol.map( Protocol::implementation ), OptionalMatchers.contains( LZO.implementation() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnModifierProtocolOfSingleConfiguredVersionIfOthersRequested()
		 public virtual void ShouldReturnModifierProtocolOfSingleConfiguredVersionIfOthersRequested()
		 {
			  // given
			  IList<ModifierSupportedProtocols> supportedProtocols = asList( new ModifierSupportedProtocols( COMPRESSION, new IList<string> { LZO.implementation() } ) );
			  ModifierProtocolRepository modifierProtocolRepository = new ModifierProtocolRepository( TestProtocols_TestModifierProtocols.values(), supportedProtocols );
			  // when
			  Optional<Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol> modifierProtocol = modifierProtocolRepository.Select( COMPRESSION.canonicalName(), asSet(TestProtocols_TestModifierProtocols.allVersionsOf(COMPRESSION)) );

			  // then
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( modifierProtocol.map( Protocol::implementation ), OptionalMatchers.contains( LZO.implementation() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompareModifierProtocolsByListOrder()
		 public virtual void ShouldCompareModifierProtocolsByListOrder()
		 {
			  IList<ModifierSupportedProtocols> supportedProtocols = asList( new ModifierSupportedProtocols( COMPRESSION, new IList<string> { LZO.implementation(), SNAPPY.implementation(), LZ4.implementation() } ) );

			  IComparer<Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol> comparator = ModifierProtocolRepository.GetModifierProtocolComparator( supportedProtocols ).apply( COMPRESSION.canonicalName() );

			  assertThat( comparator.Compare( LZO, TestProtocols_TestModifierProtocols.Snappy ), Matchers.greaterThan( 0 ) );
			  assertThat( comparator.Compare( TestProtocols_TestModifierProtocols.Snappy, TestProtocols_TestModifierProtocols.Lz4 ), Matchers.greaterThan( 0 ) );
		 }
	}

}