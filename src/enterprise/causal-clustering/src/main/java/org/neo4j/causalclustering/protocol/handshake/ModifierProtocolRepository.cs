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

	using Neo4Net.causalclustering.protocol;

	public class ModifierProtocolRepository : ProtocolRepository<string, Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol>
	{
		 private readonly ICollection<ModifierSupportedProtocols> _supportedProtocols;
		 private readonly IDictionary<string, ModifierSupportedProtocols> _supportedProtocolsLookup;

		 public ModifierProtocolRepository( Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol[] protocols, ICollection<ModifierSupportedProtocols> supportedProtocols ) : base( protocols, GetModifierProtocolComparator( supportedProtocols ), ModifierProtocolSelection::new )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  this._supportedProtocols = Collections.unmodifiableCollection( supportedProtocols );
			  this._supportedProtocolsLookup = supportedProtocols.ToDictionary( supp => supp.identifier().canonicalName(), System.Func.identity() );
		 }

		 internal static System.Func<string, IComparer<Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol>> GetModifierProtocolComparator( ICollection<ModifierSupportedProtocols> supportedProtocols )
		 {
			  return GetModifierProtocolComparator( VersionMap( supportedProtocols ) );
		 }

		 private static IDictionary<string, IList<string>> VersionMap( ICollection<ModifierSupportedProtocols> supportedProtocols )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return supportedProtocols.ToDictionary( supportedProtocol => supportedProtocol.identifier().canonicalName(), SupportedProtocols::versions );
		 }

		 private static System.Func<string, IComparer<Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol>> GetModifierProtocolComparator( IDictionary<string, IList<string>> versionMap )
		 {
			  return protocolName =>
			  {
				IComparer<Protocol_ModifierProtocol> positionalComparator = System.Collections.IComparer.comparing( modifierProtocol => Optional.ofNullable( versionMap[protocolName] ).map( versions => ByPosition( modifierProtocol, versions ) ).orElse( 0 ) );

				return FallBackToVersionNumbers( positionalComparator );
			  };
		 }

		 // Needed if supported modifiers has an empty version list
		 private static IComparer<Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol> FallBackToVersionNumbers( IComparer<Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol> positionalComparator )
		 {
			  return positionalComparator.thenComparing( VersionNumberComparator() );
		 }

		 /// <returns> Greatest is head of versions, least is not included in versions </returns>
		 private static int? ByPosition( Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol modifierProtocol, IList<string> versions )
		 {
			  int index = versions.IndexOf( modifierProtocol.implementation() );
			  return index == -1 ? int.MinValue : -index;
		 }

		 public virtual Optional<SupportedProtocols<string, Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol>> SupportedProtocolFor( string protocolName )
		 {
			  return Optional.ofNullable( _supportedProtocolsLookup[protocolName] );
		 }

		 public virtual ICollection<ModifierSupportedProtocols> SupportedProtocols()
		 {
			  return _supportedProtocols;
		 }
	}

}